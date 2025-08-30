#nullable enable
namespace XrmTools.Services;

public sealed class PluginRegistrationResult
{
    public bool Succeeded { get; }
    public string Message { get; }

    public PluginRegistrationResult(bool succeeded, string message)
    {
        Succeeded = succeeded;
        Message = message;
    }

    public static PluginRegistrationResult Success(string message = "Plugin(s) registered successfully.") =>
        new(true, message);

    public static PluginRegistrationResult Failure(string message) =>
        new(false, message);
}
#nullable restore