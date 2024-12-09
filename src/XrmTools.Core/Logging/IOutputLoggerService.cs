namespace XrmTools.Logging;

public interface IOutputLoggerService
{
    void OutputString(string value);
    void Log(Compatibility.LogLevel level, string message);
    void LogWarning(string message);
    void LogInformation(string message);
    void LogCritical(string message);
    void LogDebug(string message);
    void LogError(string message);
}
