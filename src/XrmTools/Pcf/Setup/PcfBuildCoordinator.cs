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
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.Logging;

/// <summary>
/// Connects PCF setup to Visual Studio's own coordinated build. When the solution build manager starts a build it
/// decides, once, which Node.js installation the build uses and makes it visible to that build only; each project
/// then receives its decision as MSBuild properties. Visual Studio still schedules, logs and cancels the build.
/// </summary>
[Export]
[PartCreationPolicy(CreationPolicy.Shared)]
internal sealed class PcfBuildCoordinator : IVsUpdateSolutionEvents2
{
    private static readonly TimeSpan InspectionTimeout = TimeSpan.FromSeconds(30);
    private readonly IPcfSetupService _setup;
    private readonly IPcfSetupStore _store;
    private readonly PcfBuildSession _session;
    private readonly IOutputLoggerService _output;
    private readonly ConcurrentDictionary<string, bool> _results = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, PcfBuildBlock> _blocked = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, InfoBar> _infoBars = new(StringComparer.OrdinalIgnoreCase);
    private volatile PcfBuildPlan? _plan;
    private IVsSolution? _solution;
    private uint _cookie;

    [ImportingConstructor]
    public PcfBuildCoordinator(IPcfSetupService setup, IPcfSetupStore store, PcfBuildSession session, IOutputLoggerService output)
    {
        _setup = setup; _store = store; _session = session; _output = output;
        _setup.Changed += project => ThreadHelper.JoinableTaskFactory.RunAsync(() => CloseNoticeAsync(project)).FileAndForget("XrmTools/Pcf/CloseNotice");
    }

    /// <summary>Opens the shared setup experience for a project; set by the package.</summary>
    public Func<string, Task>? ShowSetup { get; set; }

    public void Advise(IServiceProvider services)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        if (_cookie != 0 || services.GetService(typeof(SVsSolutionBuildManager)) is not IVsSolutionBuildManager2 manager) return;
        _solution = services.GetService(typeof(SVsSolution)) as IVsSolution;
        ErrorHandler.ThrowOnFailure(manager.AdviseUpdateSolutionEvents(this, out _cookie));
    }

    /// <summary>The MSBuild global properties for one project's build, from the current build's plan.</summary>
    public async Task<IReadOnlyDictionary<string, string>> GetBuildPropertiesAsync(string projectFile, CancellationToken cancellationToken)
    {
        var plan = _plan;
        var state = plan?.Find(projectFile);
        if (state is null)
        {
            // Not part of a solution-build plan: nothing was activated, so the generated targets verify Node.js themselves.
            state = await _setup.InspectAsync(projectFile, cancellationToken).ConfigureAwait(false);
            plan = PcfBuildPlan.Create([state]);
        }
        if (plan!.BlockFor(state) is { } block) _blocked[projectFile] = block;
        return plan.PropertiesFor(state);
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
            await CloseNoticeAsync(projectFile);
            if (ShowSetup is not null) await ShowSetup(projectFile);
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
        ThreadHelper.ThrowIfNotOnUIThread();
        _results.Clear();
        _blocked.Clear();
        _plan = null;
        // Marked first: a setup that starts from here on sees the build, and one already running is seen below.
        _session.BeginBuild();
        try
        {
            var projects = LoadedPcfProjects();
            if (projects.Count == 0) return VSConstants.S_OK;
            var states = ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                using var timeout = new CancellationTokenSource(InspectionTimeout);
                return await Task.Run(() => Task.WhenAll(projects.Select(project => _setup.InspectAsync(project, timeout.Token))), timeout.Token);
            });
            var plan = PcfBuildPlan.Create(states);
            _session.Activate(plan.Activation);
            _plan = plan;
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            // The build continues exactly as it would without Xrm Tools' preparation.
            _output.LogWarning($"PCF build preparation was skipped: {ex.Message}");
        }
        return VSConstants.S_OK;
    }

    public int UpdateProjectCfg_Done(IVsHierarchy pHierProj, IVsCfg pCfgProj, IVsCfg pCfgSln, uint dwAction, int fSuccess, int fCancel)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        if (fCancel == 0 && ProjectFile(pHierProj) is { } project) _results[project] = fSuccess != 0;
        return VSConstants.S_OK;
    }

    public int UpdateSolution_Done(int fSucceeded, int fModified, int fCancelCommand)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        _session.EndBuild();
        var plan = _plan;
        _plan = null;
        foreach (var result in _results)
        {
            var project = result.Key;
            var directory = Path.GetDirectoryName(project)!;
            var installed = File.Exists(Path.Combine(directory, "node_modules", "pcf-scripts", "package.json"));
            if (result.Value)
            {
                // A successful build ran Microsoft's install where one was needed, which completes an interrupted setup.
                if (installed) _store.SetSetupIncomplete(directory, false);
                continue;
            }
            string? notice = null;
            var state = plan?.Find(project);
            if (_blocked.TryGetValue(project, out var block)) notice = block.Summary;
            else if (!installed && state is { PackageManager.Kind: PcfPackageManagerKind.Other })
                notice = $"'{state.ProjectName}' is configured for {state.PackageManager.Name}, so the build did not run npm install for it.";
            else if (!installed && state is not null)
                notice = $"The build could not install npm dependencies for '{state.ProjectName}'. Setup shows what npm reported and lets you retry.";
            if (notice is not null) ThreadHelper.JoinableTaskFactory.RunAsync(() => ShowNoticeAsync(project, notice)).FileAndForget("XrmTools/Pcf/BuildNotice");
        }
        return VSConstants.S_OK;
    }

    public int UpdateSolution_StartUpdate(ref int pfCancelUpdate) => VSConstants.S_OK;
    public int UpdateSolution_Cancel() => VSConstants.S_OK;
    public int OnActiveProjectCfgChange(IVsHierarchy pIVsHierarchy) => VSConstants.S_OK;
    public int UpdateProjectCfg_Begin(IVsHierarchy pHierProj, IVsCfg pCfgProj, IVsCfg pCfgSln, uint dwAction, ref int pfCancel) => VSConstants.S_OK;

    private List<string> LoadedPcfProjects()
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        var projects = new List<string>();
        var none = Guid.Empty;
        if (_solution is null || ErrorHandler.Failed(_solution.GetProjectEnum((uint)__VSENUMPROJFLAGS.EPF_LOADEDINSOLUTION, ref none, out var enumerator))) return projects;
        var hierarchy = new IVsHierarchy[1];
        while (enumerator.Next(1, hierarchy, out var fetched) == VSConstants.S_OK && fetched == 1)
            if (ProjectFile(hierarchy[0]) is { } project) projects.Add(project);
        return projects;
    }

    private static string? ProjectFile(IVsHierarchy? hierarchy)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        return hierarchy is IVsProject project && ErrorHandler.Succeeded(project.GetMkDocument(VSConstants.VSITEMID_ROOT, out var path))
            && string.Equals(Path.GetExtension(path), ".pcfproj", StringComparison.OrdinalIgnoreCase) ? path : null;
    }
}
#nullable restore
