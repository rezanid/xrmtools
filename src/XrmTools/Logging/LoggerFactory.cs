namespace XrmTools.Logging;
using System;
using System.ComponentModel.Composition;
using XrmTools.Logging.Compatibility;

[Export(typeof(ILogger<>))]
[method: ImportingConstructor]
internal class LoggerFactory<T>(ILoggerProvider loggerProvider) : ILogger<T>
{
    private readonly ILogger logger = loggerProvider.CreateLogger(typeof(T).Name);

    public IDisposable BeginScope<TState>(TState state) => logger.BeginScope(state);
    public bool IsEnabled(LogLevel logLevel) => logger.IsEnabled(logLevel);
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) 
        => logger.Log(logLevel, eventId, state, exception, formatter);
}