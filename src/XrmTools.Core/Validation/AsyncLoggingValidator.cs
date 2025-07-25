#nullable enable
namespace XrmTools.Validation;

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.Logging.Compatibility;

public class AsyncLoggingValidator<T>(IAsyncValidator<T> inner, ILogger logger) : LoggingValidatorBase<T>(logger), IAsyncValidator<T>
{
    private readonly IAsyncValidator<T> inner = inner;

    public Type TargetType => inner.TargetType;

    public async Task<ValidationResult> ValidateAsync(T instance, CancellationToken cancellationToken = default)
    {
        Logger.LogDebug($"Validating {typeof(T).Name} asynchronously...");
        var result = await inner.ValidateAsync(instance, cancellationToken);
        LogResult(result);
        return result;
    }
}
#nullable restore