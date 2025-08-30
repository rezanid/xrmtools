#nullable enable
namespace XrmTools.Validation;

using System;
using System.ComponentModel.DataAnnotations;
using XrmTools.Logging.Compatibility;

public class LoggingValidator<T>(IValidator<T> inner, ILogger logger) : LoggingValidatorBase<T>(logger), IValidator<T>
{
    private readonly IValidator<T> inner = inner;

    public Type TargetType => inner.TargetType;

    public ValidationResult Validate(T instance)
    {
        Logger.LogDebug($"Validating {typeof(T).Name} synchronously...");
        var result = inner.Validate(instance);
        LogResult(result);
        return result;
    }
}
#nullable restore