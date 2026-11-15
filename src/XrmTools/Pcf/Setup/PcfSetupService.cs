#nullable enable
namespace XrmTools.Pcf.Setup;

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

/// <summary>Why a PCF build must stop before Microsoft's npm commands, with the text shown in the Error List.</summary>
internal sealed class PcfBuildBlock(string code, string summary, string message)
{
    public string Code { get; } = code;
    /// <summary>One sentence for the notification bar.</summary>
    public string Summary { get; } = summary;
    public string Message { get; } = message;
}

internal sealed class PcfSetupState
{
    public string ProjectFile { get; init; } = string.Empty;
    public string ProjectDirectory { get; init; } = string.Empty;
    public string ProjectName => Path.GetFileNameWithoutExtension(ProjectFile);
    public PcfPackageManagerInfo PackageManager { get; init; } = null!;
    public PcfEnvironmentResolution Environment { get; init; } = null!;
    public PcfDependencyState Dependencies { get; init; } = null!;
    /// <summary>The marker of an unfinished install, for the build to remove once its own install has succeeded.</summary>
    public string? IncompleteMarker { get; init; }
    public bool SetupRunning { get; init; }

    /// <summary>Set when a build must not reach Microsoft's npm commands until the developer has decided something.</summary>
    public PcfBuildBlock? Block
    {
        get
        {
            const string action = " To do that, choose 'Set Up PCF Build' on the project's context menu.";
            var prefix = $"PCF project '{ProjectName}': ";
            if (SetupRunning)
                return new("XTPCF006", $"Setup is still installing npm dependencies for '{ProjectName}'.",
                    prefix + "setup is still installing npm dependencies. Build again when setup has finished.");
            if (PackageManager.Kind == PcfPackageManagerKind.Conflict)
                return new("XTPCF004", $"'{ProjectName}' has lock files for more than one package manager.",
                    prefix + $"lock files for more than one package manager were found ({PackageManager.Evidence}), so it is unclear which one owns node_modules. " +
                    "Keep the lock file of the package manager you use, or declare it with \"packageManager\" in package.json, then build again. No files were changed.");
            if (Environment.IsReady) return null;
            var code = Environment.Status switch
            {
                PcfEnvironmentStatus.NodeMissing => "XTPCF001",
                PcfEnvironmentStatus.NpmMissing => "XTPCF003",
                _ => "XTPCF002"
            };
            return new(code, $"'{ProjectName}' cannot build: {Environment.Problem}", prefix + Environment.Problem + " " + Environment.Remedy + action);
        }
    }

    /// <summary>Set for a project that belongs to another package manager; Microsoft's <c>npm install</c> must not run for it.</summary>
    public string? PackageManagerNotice => PackageManager.Kind != PcfPackageManagerKind.Other ? null
        : $"PCF project '{ProjectName}' is configured for {PackageManager.Name} ({PackageManager.Evidence}). Microsoft's PCF build installs dependencies with npm, " +
          "which would add npm files to this project, so npm install was not run and no files were changed. " +
          $"Install dependencies with {PackageManager.Name} and set <PcfEnableAutoNpmInstall>false</PcfEnableAutoNpmInstall> in the project file so builds do not run npm install, then build again.";
}

internal enum PcfInstallOutcome { Succeeded, Failed, Canceled }

internal sealed class PcfInstallResult
{
    public PcfInstallOutcome Outcome { get; init; }
    public string Summary { get; init; } = string.Empty;
    public string Remedy { get; init; } = string.Empty;
    public IReadOnlyList<string> Details { get; init; } = [];
    public PcfSetupFailureKind? FailureKind { get; init; }
    public bool Succeeded => Outcome == PcfInstallOutcome.Succeeded;
    public string Message => string.Join(" ", new[] { Summary, Remedy }.Where(text => text.Length > 0));
}

internal interface IPcfSetupService
{
    /// <summary>Raised with the project file after a selection or the installed dependencies changed.</summary>
    event Action<string>? Changed;
    Task<PcfSetupState> InspectAsync(string projectFile, CancellationToken cancellationToken);
    /// <param name="projectFile"><c>null</c> for a project that does not exist yet.</param>
    Task<PcfEnvironmentResolution> ResolveEnvironmentAsync(string? projectFile, string projectDirectory, CancellationToken cancellationToken);
    Task<IReadOnlyList<NodeInstallation>> GetCandidatesAsync(string? projectFile, CancellationToken cancellationToken);
    Task<NodeInstallation?> InspectDirectoryAsync(string nodeDirectory, CancellationToken cancellationToken);
    /// <summary>Remembers an explicit choice for this project on this machine; <c>null</c> returns to automatic selection.</summary>
    void Select(string projectFile, NodeInstallation? installation);
    /// <param name="updateLockFile">Run <c>npm install</c> even though a lock file exists. Only ever set by an explicit user action.</param>
    Task<PcfInstallResult> InstallAsync(string projectFile, NodeInstallation node, bool updateLockFile, IProgress<string> output, CancellationToken cancellationToken);
}

[Export(typeof(IPcfSetupService))]
[PartCreationPolicy(CreationPolicy.Shared)]
internal sealed class PcfSetupService : IPcfSetupService
{
    private readonly INodeEnvironmentProbe _probe;
    private readonly IPcfSetupStore _store;
    private readonly IPcfNpmRunner _npm;
    private readonly PcfBuildSession _session;
    private readonly PcfEnvironmentResolver _resolver;

    [ImportingConstructor]
    public PcfSetupService(INodeEnvironmentProbe probe, IPcfSetupStore store, IPcfNpmRunner npm, PcfBuildSession session)
    {
        _probe = probe; _store = store; _npm = npm; _session = session;
        _resolver = new PcfEnvironmentResolver(probe, store);
    }

    public event Action<string>? Changed;

    public async Task<PcfSetupState> InspectAsync(string projectFile, CancellationToken cancellationToken)
    {
        var directory = Path.GetDirectoryName(Path.GetFullPath(projectFile))!;
        var incomplete = _store.IsSetupIncomplete(directory);
        return new PcfSetupState
        {
            ProjectFile = projectFile,
            ProjectDirectory = directory,
            PackageManager = PcfPackageManagerInfo.Detect(directory),
            Environment = await _resolver.ResolveAsync(projectFile, directory, cancellationToken).ConfigureAwait(false),
            Dependencies = PcfDependencyState.Read(directory, incomplete),
            IncompleteMarker = incomplete ? _store.IncompleteMarker(directory) : null,
            SetupRunning = _store.IsLocked(directory)
        };
    }

    public Task<PcfEnvironmentResolution> ResolveEnvironmentAsync(string? projectFile, string projectDirectory, CancellationToken cancellationToken)
        => _resolver.ResolveAsync(projectFile, projectDirectory, cancellationToken);

    public Task<IReadOnlyList<NodeInstallation>> GetCandidatesAsync(string? projectFile, CancellationToken cancellationToken)
        => _resolver.GetCandidatesAsync(projectFile, cancellationToken);

    public Task<NodeInstallation?> InspectDirectoryAsync(string nodeDirectory, CancellationToken cancellationToken)
        => _probe.InspectAsync(nodeDirectory, nodeDirectory, NodeEnvironmentSource.Selected, "selected for this project", cancellationToken);

    public void Select(string projectFile, NodeInstallation? installation)
    {
        _store.SetSelection(projectFile, installation is null ? null : new PcfNodeSelection { NodeDirectory = installation.NodeDirectory, Version = installation.Version.ToString() });
        Changed?.Invoke(projectFile);
    }

    public async Task<PcfInstallResult> InstallAsync(string projectFile, NodeInstallation node, bool updateLockFile, IProgress<string> output, CancellationToken cancellationToken)
    {
        var directory = Path.GetDirectoryName(Path.GetFullPath(projectFile))!;
        var name = Path.GetFileNameWithoutExtension(projectFile);
        var manager = PcfPackageManagerInfo.Detect(directory);
        if (!manager.IsNpm)
            return Failed($"'{name}' is configured for {manager.Name} ({manager.Evidence}), so npm was not run and no files were changed.",
                manager.Kind == PcfPackageManagerKind.Conflict ? "Keep the lock file of the package manager you use, then retry." : $"Install dependencies with {manager.Name}.");
        if (node.NpmLauncher is null) return Failed($"Node.js {node.Version} in '{node.NodeDirectory}' has no npm.", "Choose an installation that includes npm.");
        using var owner = _store.TryLock(directory);
        if (owner is null) return Failed($"Setup is already running for '{name}', here or in another Visual Studio window.", "Wait for it to finish.");
        // Checked after taking the lock: a build that starts from here on sees the lock and stops for this project,
        // and a build that started earlier is seen here. Two installers in one folder would corrupt node_modules.
        if (_session.IsBuilding) return Failed("A build is running.", "Wait for it to finish or cancel it, then retry setup.");
        if (cancellationToken.IsCancellationRequested) return Canceled(false);

        var command = updateLockFile || !PcfDependencyState.Read(directory, false).HasLockFile ? "install" : "ci";
        // Recorded before npm touches node_modules so an interrupted install is never mistaken for a finished one.
        _store.SetSetupIncomplete(directory, true);
        output.Report($"> npm {command}   [{node.Describe()}; {node.NodeDirectory}]");
        PcfNpmResult result;
        try { result = await _npm.RunAsync(node, directory, command, output, cancellationToken).ConfigureAwait(false); }
        catch (OperationCanceledException) { return Canceled(true); }
        catch (InvalidOperationException ex) { return Failed(ex.Message, "Choose a working Node.js installation, then retry."); }
        if (!result.Succeeded)
        {
            var failure = PcfNpmFailure.Classify(command, result);
            return new PcfInstallResult { Outcome = PcfInstallOutcome.Failed, Summary = failure.Summary, Remedy = failure.Remedy, Details = PcfNpmFailure.ErrorLines(result), FailureKind = failure.Kind };
        }
        var marker = Path.Combine(directory, "node_modules", "pcf-scripts", "package.json");
        if (!File.Exists(marker))
            return Failed($"npm {command} finished, but pcf-scripts was not installed.", "Check that package.json lists pcf-scripts in devDependencies, then retry.");
        // Microsoft's incremental install compares this file with package.json; like its own restore, mark it current
        // so the next build does not install a second time.
        try { File.SetLastWriteTimeUtc(marker, DateTime.UtcNow); } catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { }
        _store.SetSetupIncomplete(directory, false);
        Changed?.Invoke(projectFile);
        return new PcfInstallResult { Outcome = PcfInstallOutcome.Succeeded, Summary = $"Dependencies were installed with npm {command} using Node.js {node.Version}." };
    }

    private static PcfInstallResult Failed(string summary, string remedy) => new() { Outcome = PcfInstallOutcome.Failed, Summary = summary, Remedy = remedy };

    private static PcfInstallResult Canceled(bool started) => new()
    {
        Outcome = PcfInstallOutcome.Canceled,
        Summary = started ? "Setup was canceled; dependencies may be only partly installed." : "Setup was canceled before npm started.",
        Remedy = "Run setup again to finish."
    };
}
#nullable restore
