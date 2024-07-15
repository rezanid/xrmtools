using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using System;

namespace XrmGen._Core;

class Logger
{
    private static OutputWindowPane pane;
    private static readonly object _syncRoot = new();
    private static readonly DTE2 dte = Package.GetGlobalService(typeof(EnvDTE.DTE)) as DTE2;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread", Justification = "Already done by calling Ensure method.")]
    public static void Log(string message)
    {
        if (EnsurePane())
        {
            pane.OutputString("[" + DateTime.Now.ToLongTimeString() + "] " + message + Environment.NewLine);
        }
    }

    private static bool EnsurePane()
    {
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
