#nullable enable
namespace XrmTools.Logging;

using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

[Guid(PackageGuids.guidOutputLoggerServiceString)]
[ComVisible(true)]
public interface IOutputLoggerService
{
    void OutputString(string value);
    void Log(LogLevel level, string message);
    void LogWarning(string message);
    void LogInformation(string message);
    void LogCritical(string message);
    void LogDebug(string message);
    void LogError(string message);
}

public class OutputLoggerService : IOutputLoggerService
{
    private readonly ConcurrentQueue<string> logQueue = new();
    private IVsOutputWindowPane? _outputPane;
    private IVsOutputWindowPane? OutputPane { get => _outputPane ??= InitializeOutputPane(); }

    private IVsOutputWindowPane? InitializeOutputPane()
    {
        if (!ThreadHelper.JoinableTaskFactory.Context.IsOnMainThread) return null;
        ThreadHelper.ThrowIfNotOnUIThread();

        if (Package.GetGlobalService(typeof(SVsOutputWindow)) is not IVsOutputWindow outputWindow) return null;
        outputWindow.CreatePane(ref PackageGuids.guidXrmCodeGenPackage, Vsix.Name, 1, 1);
        outputWindow.GetPane(ref PackageGuids.guidXrmCodeGenPackage, out var outputPane);
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
    public void Log(LogLevel level, string message) => OutputString($"[{level}] : {message}");
    public void LogWarning(string message) => Log(LogLevel.Warning, message);
    public void LogInformation(string message) => Log(LogLevel.Information, message);
    public void LogCritical(string message) => Log(LogLevel.Critical, message);
    public void LogDebug(string message) => Log(LogLevel.Debug, message);
    public void LogError(string message) => Log(LogLevel.Error, message);
}
#nullable restore