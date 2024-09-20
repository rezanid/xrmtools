#nullable enable
namespace XrmTools.Logging;

using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Threading;

public class OutputLogger(string name, IVsOutputWindowPane outputPane, LogLevel minLogLevel) : ILogger
{
    private readonly string _name = name ?? throw new ArgumentNullException(nameof(name));
    private readonly IVsOutputWindowPane _outputPane = outputPane ?? throw new ArgumentNullException(nameof(outputPane));

    // A thread-local dictionary to store scopes for each thread
    private static readonly AsyncLocal<Scope> _currentScope = new();

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        var scope = new Scope(this, state);
        _currentScope.Value = scope;
        return scope;
    }

    public bool IsEnabled(LogLevel logLevel) => logLevel >= minLogLevel;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;

        if (formatter == null) throw new ArgumentNullException(nameof(formatter));

        var message = formatter(state, exception);
        if (string.IsNullOrEmpty(message)) return;

        var logRecord = $"[{logLevel}] {_name}: {message}";

        // Append scope information, if any
        var currentScope = _currentScope.Value;
        if (currentScope != null)
        {
            var scopeInfo = new List<string>();
            while (currentScope != null)
            {
                scopeInfo.Insert(0, currentScope.State.ToString());
                currentScope = currentScope.Parent;
            }
            logRecord = $"{string.Join(" => ", scopeInfo)}: {logRecord}";
        }

        if (exception != null)
        {
            logRecord += Environment.NewLine + exception.ToString();
        }

        // Ensure we're on the main thread to write to the Output window
        ThreadHelper.ThrowIfNotOnUIThread();
        _outputPane.OutputString(logRecord + Environment.NewLine);
    }

    private class Scope(OutputLogger logger, object state) : IDisposable
    {
        public readonly object State = state;
        public readonly Scope Parent = _currentScope.Value;

        public void Dispose()
        {
            if (_currentScope.Value == this)
            {
                // Restore the parent scope when disposed
                _currentScope.Value = Parent;
            }
        }
    }
}
#nullable restore