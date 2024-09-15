#nullable enable
namespace XrmGen.Logging;

using EnvDTE;
using EnvDTE80;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Shell;
using System;
using System.Diagnostics.CodeAnalysis;

public class OutputLogger(string categoryName) : ILogger
{
    private static OutputWindowPane? pane;
    private static readonly object _syncRoot = new();
    private static DTE2? dte = null;

    [SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread", Justification = "Already done by calling Ensure method.")]
    public static void Log(string message)
    {
        lock (_syncRoot)
        {
            if (EnsurePane())
            {
                pane.OutputString("[" + DateTime.Now.ToLongTimeString() + "] " + message + Environment.NewLine);
            }
        }
    }

    #region ILogger implementation
    public IDisposable? BeginScope<TState>(TState state) => null;

    public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;

        var message = formatter(state, exception);
        Log($"[{logLevel}] {categoryName}: {message}");

        if (exception != null) Log($"Exception: {exception}");
    }
    #endregion

    [MemberNotNull(nameof(dte))]
    private static void EnsureDte()
    {
        if (dte != null) { return; }
        ThreadHelper.ThrowIfNotOnUIThread();
        dte = (Package.GetGlobalService(typeof(DTE)) as DTE2)!;
    }

    [MemberNotNullWhen(true, nameof(pane))]
    private static bool EnsurePane()
    {
        EnsureDte();
        ThreadHelper.ThrowIfNotOnUIThread();
        if (pane == null)
        {
            lock (_syncRoot)
            {
                pane ??= dte.ToolWindows.OutputWindow.OutputWindowPanes.Add(Vsix.Name);
            }
        }
        return pane != null;
    }
}
#nullable restore