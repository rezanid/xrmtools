#nullable enable
namespace XrmTools.Logging;

using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using XrmTools.Logging.Compatibility;
using System;
using System.ComponentModel.Composition;

[Export(typeof(IOutputLoggerService))]
public class OutputLoggerService : IOutputLoggerService
{
    private readonly ConcurrentQueue<string> logQueue = new();
    private static IVsOutputWindowPane? _outputPane;
    private readonly object _lock = new();
    private IVsOutputWindowPane? OutputPane 
    {
        get
        {
            if (_outputPane != null) return _outputPane;
            lock (_lock)
            {
                return _outputPane ??= InitializeOutputPane();
            }
        }
    }

    private IVsOutputWindowPane? InitializeOutputPane()
    {
        if (!ThreadHelper.JoinableTaskFactory.Context.IsOnMainThread) return null;
        ThreadHelper.ThrowIfNotOnUIThread();

        if (Package.GetGlobalService(typeof(SVsOutputWindow)) is not IVsOutputWindow outputWindow) return null;
        outputWindow.CreatePane(ref PackageGuids.XrmToolsPackageId, Vsix.Name, 1, 1);
        outputWindow.GetPane(ref PackageGuids.XrmToolsPackageId, out var outputPane);
        return outputPane;
    }

    [SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread", Justification = "We are checking if this is the UI thread.")]
    public void OutputString(string value)
    {
        if (ThreadHelper.JoinableTaskFactory.Context.IsOnMainThread)
        {
            if (logQueue.Count > 0)
            {
                while (logQueue.TryDequeue(out var log))
                {
                    OutputPane?.OutputString(log);
                }
            }
            OutputPane?.OutputString(value);
        }
        else
        {
            logQueue.Enqueue(value);
        }
    }
    public void Log(LogLevel level, string message) => OutputString($"[{level}] : {message}{Environment.NewLine}");
    public void LogWarning(string message) => Log(LogLevel.Warning, message);
    public void LogInformation(string message) => Log(LogLevel.Information, message);
    public void LogCritical(string message) => Log(LogLevel.Critical, message);
    public void LogDebug(string message) => Log(LogLevel.Debug, message);
    public void LogError(string message) => Log(LogLevel.Error, message);
}
#nullable restore