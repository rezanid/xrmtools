#nullable enable
namespace XrmTools.Logging;
using System;
using System.Collections.Concurrent;
using System.ComponentModel.Composition;
using XrmTools.Logging.Compatibility;

[Export(typeof(ILoggerProvider))]
internal class OutputLoggerProvider : ILoggerProvider
{
    private readonly ConcurrentDictionary<string, OutputLogger> _loggers = new();

    [Import]
    IOutputLoggerService? OutputLoggerService { get; set; }

    public ILogger CreateLogger(string categoryName)
    {
        if (OutputLoggerService == null) throw new InvalidOperationException("Failed to create Logger. OutputLoggerService not available.");
        return _loggers.GetOrAdd(categoryName, name => new OutputLogger(name, OutputLoggerService));
    }

    public void Dispose() => _loggers.Clear();
}
#nullable restore