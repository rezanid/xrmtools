#nullable enable
namespace XrmTools.Logging;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.ComponentModelHost;
using System;
using System.Collections.Concurrent;
using System.ComponentModel.Composition;

public class OutputLoggerProvider : ILoggerProvider
{
    private readonly ConcurrentDictionary<string, OutputLogger> _loggers = new();
    private readonly IOptionsMonitor<LoggerFilterOptions> _options;

    [Import]
    IOutputLoggerService? OutputLoggerService { get; set; }

    public OutputLoggerProvider(IOptionsMonitor<LoggerFilterOptions> options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));

        var componentModel = (IComponentModel)Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(SComponentModel));
        if (componentModel == null) return;
        componentModel.DefaultCompositionService.SatisfyImportsOnce(this);
    }

    public ILogger CreateLogger(string categoryName)
    {
        // Retrieve the log level from the LoggerFilterOptions
        var minLogLevel = _options.CurrentValue.MinLevel;
        return _loggers.GetOrAdd(categoryName, name => new OutputLogger(name, OutputLoggerService, minLogLevel));
    }

    public void Dispose() => _loggers.Clear();
}
#nullable restore