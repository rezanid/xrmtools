#nullable enable
namespace XrmTools.Validation;

using System.ComponentModel.DataAnnotations;
using XrmTools.Logging.Compatibility;

public abstract class LoggingValidatorBase<T>(ILogger logger)
{
    protected void LogResult(ValidationResult result)
    {
        if (result != ValidationResult.Success)
        {
            logger.LogWarning($"Validation failed for {typeof(T).Name}: {result.ErrorMessage}");
        }
        else
        {
            logger.LogDebug($"{typeof(T).Name} is valid.");
        }
    }
}
#nullable restore