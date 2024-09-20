namespace XrmTools.Logging;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Concurrent;

public class OutputLoggerProvider : ILoggerProvider
{
    private readonly ConcurrentDictionary<string, OutputLogger> _loggers = new();
    private readonly IVsOutputWindowPane _outputPane;
    private readonly IOptionsMonitor<LoggerFilterOptions> _options;

    public OutputLoggerProvider(IOptionsMonitor<LoggerFilterOptions> options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));

        // Get the Output window pane for our logger
        ThreadHelper.ThrowIfNotOnUIThread();
        var outputWindow = Package.GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;

        // Use a GUID to uniquely identify your output pane (use any GUID you prefer)
        outputWindow.CreatePane(ref PackageGuids.guidXrmCodeGenPackage, Vsix.Name, 1, 1);
        outputWindow.GetPane(ref PackageGuids.guidXrmCodeGenPackage, out _outputPane);
    }

    public ILogger CreateLogger(string categoryName)
    {
        // Retrieve the log level from the LoggerFilterOptions
        var minLogLevel = _options.CurrentValue.MinLevel;
        return _loggers.GetOrAdd(categoryName, name => new OutputLogger(name, _outputPane, minLogLevel));
    }

    public void Dispose()
    {
        _loggers.Clear();
    }
}
