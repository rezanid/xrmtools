#nullable enable
namespace XrmTools.Validation;

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ILogger = Logging.Compatibility.ILogger;

public interface IValidationService
{
    Task<ValidationResult> ValidateIfAvailableAsync<T>(T value, string? category = null, CancellationToken cancellationToken = default);
    ValidationResult ValidateIfAvailable<T>(T value, string? category = null);
}

[Export(typeof(IValidationService))]
public class ValidationService : IValidationService
{
    private readonly ILogger logger;

    private readonly Dictionary<Type, List<(IValidator Validator, ValidatorMetadata Metadata)>> _validators = [];

    [ImportingConstructor]
    public ValidationService([ImportMany] IEnumerable<IValidator> validators, Logging.Compatibility.ILogger<ValidationService> logger)
    {
        this.logger = logger;
        foreach (var validator in validators)
        {
            var validatorType = validator.GetType();
            var attr = validatorType.GetCustomAttribute<ValidatorAttribute>();

            if (attr is { Enabled: false }) continue;

            var category = attr?.Category;
            var priority = attr?.Priority ?? 0;
            var targetType = validator.TargetType;

            var wrapped = Decorate(validator);

            if (!_validators.TryGetValue(targetType, out var list))
                _validators[targetType] = list = [];

            list.Add((wrapped, new ValidatorMetadata(priority, category)));
        }
    }

    /// <summary>
    /// Validates the value using the available validators, skipping async validators.
    /// </summary>
    /// <exception cref="InvalidOperationException">If for any unknown reason a validator does not 
    /// implement <see cref="IAsyncValidator{T}"/> or <see cref="IValidator{T}"/>.</exception>
    public ValidationResult ValidateIfAvailable<T>(T value, string? category = null)
    {
        if (_validators.TryGetValue(typeof(T), out var list))
        {
            foreach (var (validator, metadata) in list)
            {
                if (category != null && metadata.Category != null && !metadata.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
                    continue;
                var result = validator switch
                {
                    // Let's ignore IAsyncValidator<T>s
                    IAsyncValidator<T> asyncValidator => ValidationResult.Success,
                    IValidator<T> syncValidator => syncValidator.Validate(value!),
                    _ => throw new InvalidOperationException($"Validator {validator.GetType()} is not supported.")
                }; 
                if (result != ValidationResult.Success) return result;
            }
        }
        return ValidationResult.Success;
    }

    public async Task<ValidationResult> ValidateIfAvailableAsync<T>(
        T value,
        string? category = null,
        CancellationToken cancellationToken = default)
    {
        if (_validators.TryGetValue(typeof(T), out var list))
        {
            foreach (var (validator, metadata) in list)
            {
                var result = validator switch
                {
                    IAsyncValidator<T> asyncValidator => await asyncValidator.ValidateAsync(value!, cancellationToken),
                    IValidator<T> syncValidator => syncValidator.Validate(value!),
                    _ => throw new InvalidOperationException($"Validator {validator.GetType()} is not supported.")
                };
                if (result != ValidationResult.Success) return result;
            }
        }

        return ValidationResult.Success;
    }

    private IValidator Decorate(IValidator validator)
    {
        var validatorType = validator.GetType();
        var interfaces = validatorType.GetInterfaces();
        var genericInterface = 
            interfaces.FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IAsyncValidator<>));

        if (genericInterface != null)
        {
            var targetType = genericInterface.GetGenericArguments()[0];
            var wrapperType = typeof(AsyncLoggingValidator<>).MakeGenericType(targetType);
            return (IValidator)Activator.CreateInstance(wrapperType, validator, logger)!;
        }

        var syncGenericInterface =
            interfaces.FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IValidator<>));

        if (syncGenericInterface != null)
        {
            var targetType = syncGenericInterface.GetGenericArguments()[0];
            var wrapperType = typeof(LoggingValidator<>).MakeGenericType(targetType);
            return (IValidator)Activator.CreateInstance(wrapperType, validator, logger)!;
        }

        return validator;
    }

    private sealed record ValidatorMetadata(int Priority, string? Category);
}
#nullable restore