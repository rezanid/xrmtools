#nullable enable
namespace XrmTools.Pcf.Setup;

using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.Logging;

/// <summary>
/// Connects PCF setup to Visual Studio's own coordinated build. Each PCF project that takes part in a build is
/// checked against the environment of this Visual Studio instance, which is the environment the build runs with, and
/// receives the result as MSBuild properties. Visual Studio still schedules, logs and cancels the build, and its
/// environment is never changed.
/// </summary>
[Export]
[PartCreationPolicy(CreationPolicy.Shared)]
internal sealed class PcfBuildCoordinator : IVsUpdateSolutionEvents2
{
    private readonly IPcfSetupService _setup;
    private readonly IPcfSetupStore _store;
    private readonly PcfBuildSession _session;
    private readonly IPcfInstanceLauncher _launcher;
    private readonly IOutputLoggerService _output;
    private readonly ConcurrentDictionary<string, bool?> _results = new(StringComparer.OrdinalIgnoreCase);
    // Only projects Visual Studio actually builds are inspected, so a loaded project outside the build has no say.
    private readonly ConcurrentDictionary<string, PcfSetupState> _states = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, InfoBar> _infoBars = new(StringComparer.OrdinalIgnoreCase);
    private uint _cookie;

    [ImportingConstructor]
    public PcfBuildCoordinator(IPcfSetupService setup, IPcfSetupStore store, PcfBuildSession session, IPcfInstanceLauncher launcher, IOutputLoggerService output)
    {
        _setup = setup; _store = store; _session = session; _launcher = launcher; _output = output;
        _setup.Changed += project => ThreadHelper.JoinableTaskFactory.RunAsync(() => CloseNoticeAsync(project)).FileAndForget("XrmTools/Pcf/CloseNotice");
    }

    /// <summary>Starts listening to solution builds. Safe to call repeatedly, from any thread.</summary>
    public async Task EnsureAdvisedAsync()
    {
        if (_cookie != 0) return;
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        if (_cookie != 0 || ServiceProvider.GlobalProvider.GetService(typeof(SVsSolutionBuildManager)) is not IVsSolutionBuildManager2 manager) return;
        ErrorHandler.ThrowOnFailure(manager.AdviseUpdateSolutionEvents(this, out _cookie));
    }

    /// <summary>Opens the shared setup experience for a project and carries out a restart the developer chose there.</summary>
    public async Task ShowSetupAsync(string projectFile)
    {
        await CloseNoticeAsync(projectFile);
        _output.OutputString($"{Environment.NewLine}PCF build setup: {projectFile}{Environment.NewLine}");
        var request = await UI.PcfSetupDialog.ShowAsync(projectFile, _setup, line => _output.OutputString(line + Environment.NewLine), VsShellUtilities.ShutdownToken);
        if (request is null) return;
        try
        {
            if (request.CloseThisWindow && _session.IsBuilding)
                throw new InvalidOperationException("A build is running. Wait for it to finish or cancel it, then choose the restart again.");
            if (await _launcher.LaunchAsync(request))
                _output.OutputString($"Opened the solution in a new Visual Studio window with {request.Node.Describe()} first on its PATH ({request.Node.NodeDirectory}).{Environment.NewLine}");
        }
        catch (Exception ex) when (ex is Win32Exception or InvalidOperationException or COMException)
        {
            await VS.MessageBox.ShowErrorAsync(Vsix.Name, $"Visual Studio was not restarted with Node.js {request.Node.Version}. {ex.Message}");
        }
    }

    /// <summary>The MSBuild global properties for one project's build. Visual Studio asks only for projects it is building.</summary>
    public async Task<IReadOnlyDictionary<string, string>> GetBuildPropertiesAsync(string projectFile, CancellationToken cancellationToken)
    {
        var state = await _setup.InspectAsync(projectFile, cancellationToken).ConfigureAwait(false);
        _states[projectFile] = state;
        return PcfBuildProperties.For(state);
    }

    /// <summary>Tells the developer, right after a project was added, when it cannot build in this window as it is.</summary>
    public async Task ShowNoticeIfBuildIsBlockedAsync(string projectFile)
    {
        var state = await _setup.InspectAsync(projectFile, VsShellUtilities.ShutdownToken).ConfigureAwait(false);
        if (state.BuildBlock is { } block) await ShowNoticeAsync(projectFile, block.Summary).ConfigureAwait(false);
    }

    /// <summary>Shows a notification with the Set up PCF build action, at most once per project at a time.</summary>
    public async Task ShowNoticeAsync(string projectFile, string text)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        // One notification per project at a time; one the developer dismissed may come back after a later failed build.
        if (_infoBars.TryGetValue(projectFile, out var shown) && shown.IsVisible) return;
        var model = new InfoBarModel([new InfoBarTextSpan(text + " "), new InfoBarHyperlink("Set up PCF build")], KnownMonikers.StatusWarning, true);
        var infoBar = await VS.InfoBar.CreateAsync(model);
        if (infoBar is null) return;
        _infoBars[projectFile] = infoBar;
        infoBar.ActionItemClicked += (_, _) => ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
        {
            // Let the click finish before a modal dialog opens; the notification bar and accessibility clients
            // would otherwise wait on it for as long as the dialog stays open.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(alwaysYield: true);
            await ShowSetupAsync(projectFile);
        }).FileAndForget("XrmTools/Pcf/SetupNotice");
        if (!await infoBar.TryShowInfoBarUIAsync()) _infoBars.Remove(projectFile);
    }

    public async Task CloseNoticeAsync(string projectFile)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        if (!_infoBars.TryGetValue(projectFile, out var infoBar)) return;
        _infoBars.Remove(projectFile);
        infoBar.Close();
    }

    public int UpdateSolution_Begin(ref int pfCancelUpdate)
    {
        _results.Clear();
        _states.Clear();
        // Marked before any project is inspected: a setup that starts from here on sees the build, and one that is
        // already running is seen when its project is inspected.
        _session.BeginBuild();
        return VSConstants.S_OK;
    }

    public int UpdateProjectCfg_Done(IVsHierarchy pHierProj, IVsCfg pCfgProj, IVsCfg pCfgSln, uint dwAction, int fSuccess, int fCancel)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        if (ProjectFile(pHierProj) is { } project) _results[project] = fCancel != 0 ? null : fSuccess != 0;
        return VSConstants.S_OK;
    }

    public int UpdateSolution_Done(int fSucceeded, int fModified, int fCancelCommand)
    {
        _session.EndBuild();
        foreach (var result in _results)
        {
            var project = result.Key;
            var directory = Path.GetDirectoryName(project)!;
            var installed = File.Exists(Path.Combine(directory, "node_modules", "pcf-scripts", "package.json"));
            // A successful project needs nothing. An unfinished install is ended only by setup's repair; no build
            // clears it.
            if (result.Value == true) continue;
            _states.TryGetValue(project, out var state);
            string? notice = null;
            // This build had to install from nothing and did not succeed. npm writes node_modules\.package-lock.json
            // last; without it the install was cut short, and both npm and Microsoft's incremental check could take
            // the half-installed folder for current from here on.
            if (state is { BuildBlock: null, PackageManager.IsNpm: true, Dependencies.Status: PcfDependencyStatus.Missing } && Directory.Exists(Path.Combine(directory, "node_modules"))
                && !File.Exists(Path.Combine(directory, "node_modules", ".package-lock.json")))
            {
                _store.SetSetupIncomplete(directory, true);
                notice = $"The npm install for '{state.ProjectName}' did not finish. Repair its dependencies before building again.";
            }
            else if (result.Value is null) continue;
            else if (state?.BuildBlock is { } block) notice = block.Summary;
            else if (!installed && state is { PackageManager.Kind: PcfPackageManagerKind.Other })
                notice = $"'{state.ProjectName}' is configured for {state.PackageManager.Name}, so the build did not run npm install for it.";
            else if (!installed && state is not null)
                notice = $"'{state.ProjectName}' did not build because its npm dependencies are not installed. Setup can install them and shows what npm reports.";
            if (notice is not null) ThreadHelper.JoinableTaskFactory.RunAsync(() => ShowNoticeAsync(project, notice)).FileAndForget("XrmTools/Pcf/BuildNotice");
        }
        return VSConstants.S_OK;
    }

    public int UpdateSolution_StartUpdate(ref int pfCancelUpdate) => VSConstants.S_OK;
    public int UpdateSolution_Cancel() => VSConstants.S_OK;
    public int OnActiveProjectCfgChange(IVsHierarchy pIVsHierarchy) => VSConstants.S_OK;
    public int UpdateProjectCfg_Begin(IVsHierarchy pHierProj, IVsCfg pCfgProj, IVsCfg pCfgSln, uint dwAction, ref int pfCancel) => VSConstants.S_OK;

    private static string? ProjectFile(IVsHierarchy? hierarchy)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        return hierarchy is IVsProject project && ErrorHandler.Succeeded(project.GetMkDocument(VSConstants.VSITEMID_ROOT, out var path))
            && string.Equals(Path.GetExtension(path), ".pcfproj", StringComparison.OrdinalIgnoreCase) ? path : null;
    }
}
#nullable restore
