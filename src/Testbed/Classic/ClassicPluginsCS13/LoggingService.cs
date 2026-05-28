namespace XrmGenTest;
using Microsoft.Xrm.Sdk;

using Microsoft.Xrm.Sdk.PluginTelemetry;
using System;

public interface ILoggingService
{
    void Log(string message);
    void LogVerbose(string message);
    void LogCritical(string message);
    void LogWarning(string message);
    void LogError(Exception ex, string message);
    void LogError(Exception ex);
    void LogError(string message);
}


/// <summary>
/// Specialized tracing service that prefixes all traced messages 
/// with a time delta for Plugin performance diagnostics
/// </summary>
internal class LoggingService : ILoggingService
{
    private readonly ITracingService _tracingService;
    private readonly ILogger _logger;
    private readonly DateTime _previousTraceTime;

    public LoggingService(IServiceProvider serviceProvider)
    {
        var utcNow = DateTime.UtcNow;
        var context = (IExecutionContext)serviceProvider.GetService(typeof(IExecutionContext));
        var initialTimestamp = context.OperationCreatedOn;
        if (initialTimestamp > utcNow)
        {
            initialTimestamp = utcNow;
        }
        _tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
        _logger = (ILogger)serviceProvider.GetService(typeof(ILogger));
        _previousTraceTime = initialTimestamp;
    }

    public void Log(string message)
    {
        _logger?.LogInformation(message);
        _tracingService.Trace($"[inf][+{CalculateElapsed():N0}ms] {message}");
    }

    public void LogVerbose(string message)
    {
        _logger?.LogDebug(message);
        _tracingService.Trace($"[vrb][+{CalculateElapsed():N0}ms] {message}");
    }

    public void LogCritical(string message)
    {
        _logger?.LogCritical(message);
        _tracingService.Trace($"[crt][+{CalculateElapsed():N0}ms] {message}");
    }

    public void LogWarning(string message)
    {
        _logger?.LogWarning(message);
        _tracingService.Trace($"[wrn][+{CalculateElapsed():N0}ms] {message}");
    }

    public void LogError(Exception ex, string message)
    {
        _logger?.LogError(ex, message);
        _tracingService.Trace($"[err][+{CalculateElapsed():N0}ms] {message} {ex}");
    }

    public void LogError(Exception ex)
    {
        _logger?.LogError(ex, ex.Message);
        _tracingService.Trace($"[err][+{CalculateElapsed():N0}ms] {ex}");
    }

    public void LogError(string message)
    {
        _logger?.LogError(message);
        _tracingService.Trace($"[err][+{CalculateElapsed():N0}ms] {message}");
    }

    private double CalculateElapsed() => DateTime.UtcNow.Subtract(_previousTraceTime).TotalMilliseconds;
}
