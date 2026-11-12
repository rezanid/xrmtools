#nullable enable
namespace XrmTools.Pcf;

using EnvDTE;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;
using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.DataverseSolutions;
using XrmTools.Logging;

[Export]
[AppliesTo(PcfProjectRegistration.UniqueCapability)]
internal sealed class PcfHarnessSession : IDisposable
{
    private readonly UnconfiguredProject _project;
    private readonly IOutputLoggerService _output;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly object _sync = new();
    private CancellationTokenSource? _launchCancellation;
    private PcfHarnessProcess? _process;
    private DebuggerEvents? _debuggerEvents;
    private bool _debugging;
    private bool _disposed;

    [ImportingConstructor]
    public PcfHarnessSession(UnconfiguredProject project, IOutputLoggerService output)
    {
        _project = project;
        _output = output;
        _project.ProjectUnloading += OnProjectUnloadingAsync;
    }

    public bool IsRunning
    {
        get { lock (_sync) return _launchCancellation is not null || _process?.IsRunning == true; }
    }

    public async Task<Uri> StartAsync(bool debugging, CancellationToken cancellationToken)
    {
        if (!await _gate.WaitAsync(0, cancellationToken).ConfigureAwait(false))
            throw new InvalidOperationException("The local harness is already starting for this project.");
        CancellationTokenSource? launchCancellation = null;
        try
        {
            if (_disposed) throw new ObjectDisposedException(nameof(PcfHarnessSession));
            if (IsRunning) throw new InvalidOperationException("The local harness is already running for this project. Stop it before starting a new session.");
            _process?.Dispose();
            launchCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            lock (_sync)
            {
                if (_disposed) throw new ObjectDisposedException(nameof(PcfHarnessSession));
                _launchCancellation = launchCancellation;
            }
            cancellationToken = launchCancellation.Token;
            await Community.VisualStudio.Toolkit.VS.Windows.CreateOutputWindowPaneAsync(Vsix.Name).ConfigureAwait(false);
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            if (_debuggerEvents is null)
            {
                var dte = (DTE?)await AsyncServiceProvider.GlobalProvider.GetServiceAsync(typeof(DTE));
                cancellationToken.ThrowIfCancellationRequested();
                if (dte is null) throw new InvalidOperationException("Visual Studio debugger automation is unavailable.");
                _debuggerEvents = dte.Events.DebuggerEvents;
                _debuggerEvents.OnEnterDesignMode += OnEnterDesignMode;
            }
            _debugging = debugging;
            var directory = Path.GetDirectoryName(_project.FullPath)!;
            Write($"Starting PCF local harness: {directory}");
            if (debugging)
                Write("Original TypeScript breakpoints require source maps from your bundler. Stock PCF development bundles may expose only transpiled JavaScript; see the PCF project guide.");
            await TaskScheduler.Default;
            PcfHarnessProcess process;
            lock (_sync)
            {
                cancellationToken.ThrowIfCancellationRequested();
                process = PcfHarnessProcess.Start(directory, Write);
                _process = process;
            }
            try
            {
                return await process.WaitForReadyAsync(cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                Stop();
                throw;
            }
        }
        finally
        {
            lock (_sync)
            {
                _launchCancellation = null;
                launchCancellation?.Dispose();
            }
            _gate.Release();
        }
    }

    public void Stop()
    {
        PcfHarnessProcess? process;
        lock (_sync)
        {
            _launchCancellation?.Cancel();
            process = _process;
            _process = null;
            _debugging = false;
        }
        if (process is null) return;
        process.Dispose();
        Write("PCF local harness stopped.");
    }

    private void OnEnterDesignMode(dbgEventReason reason)
    {
        if (_debugging) Stop();
    }

    private Task OnProjectUnloadingAsync(object? sender, EventArgs e)
    {
        Dispose();
        return Task.CompletedTask;
    }

    private void Write(string line) => _output.OutputString(line + Environment.NewLine);

    public void Dispose()
    {
        lock (_sync)
        {
            if (_disposed) return;
            _disposed = true;
        }
        _project.ProjectUnloading -= OnProjectUnloadingAsync;
        Stop();
        if (_debuggerEvents is not null)
        {
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                _debuggerEvents.OnEnterDesignMode -= OnEnterDesignMode;
                _debuggerEvents = null;
            });
        }
    }
}
#nullable restore
