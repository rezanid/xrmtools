#nullable enable
namespace XrmTools.UI;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.Pcf.Setup;

internal sealed class PcfNodeChoice(NodeInstallation installation, string? mismatch)
{
    public NodeInstallation Installation { get; } = installation;
    /// <summary>Why this installation does not fit the project; <c>null</c> when it does.</summary>
    public string? Mismatch { get; } = mismatch;
    public string Display => Installation.Describe() + (Mismatch is null ? string.Empty : " — does not fit this project");
    public override string ToString() => Display;
}

/// <summary>
/// The Node.js and dependency part of PCF setup, shared by the new-project wizard and Set Up PCF Build so that both
/// show the same choices, wording and recovery.
/// </summary>
internal sealed class PcfSetupViewModel(IPcfSetupService service, Action<string>? log = null) : ViewModelBase
{
    private const int OutputLines = 200;
    private readonly List<string> _output = [];
    private PcfNodeChoice? _selected;
    private PcfNodeChoice? _automatic;
    private PcfEnvironmentResolution? _resolution;
    private bool _checking, _expanded, _installing, _install = true, _hasStoredSelection;
    private string? _projectFile;
    private string _outputText = string.Empty;
    private PcfInstallResult? _lastResult;

    public ObservableCollection<PcfNodeChoice> Choices { get; } = [];

    public PcfNodeChoice? Selected
    {
        get => _selected;
        set
        {
            if (!SetProperty(ref _selected, value)) return;
            Notify();
        }
    }

    public NodeInstallation? Installation => Selected?.Installation;
    public bool IsChecking { get => _checking; private set { if (SetProperty(ref _checking, value)) Notify(); } }
    public bool IsInstalling { get => _installing; private set => SetProperty(ref _installing, value); }
    public bool IsExpanded { get => _expanded; set => SetProperty(ref _expanded, value); }
    public bool InstallDependencies { get => _install; set { if (SetProperty(ref _install, value)) Notify(); } }
    public string Output { get => _outputText; private set => SetProperty(ref _outputText, value); }
    public PcfInstallResult? LastResult { get => _lastResult; private set { if (SetProperty(ref _lastResult, value)) OnPropertyChanged(nameof(CanUpdateLockFile)); } }
    public bool CanUpdateLockFile => LastResult?.FailureKind == PcfSetupFailureKind.LockFileMismatch;

    /// <summary>The developer picked something other than what Xrm Tools would use on its own.</summary>
    public bool IsExplicitChoice => Selected is not null && !Selected.Installation.IsSameEnvironment(_automatic?.Installation);
    public bool HasStoredSelection { get => _hasStoredSelection; private set => SetProperty(ref _hasStoredSelection, value); }

    /// <summary>A usable Node.js with npm that fits the project is selected.</summary>
    public bool IsReady => Selected is { Mismatch: null, Installation.NpmLauncher: not null };

    public string Summary => IsChecking ? "Checking for Node.js and npm…"
        : IsReady ? $"Node.js {Installation!.Version} with npm {Installation.NpmVersion ?? "(version unknown)"} — {Installation.Label}"
        : "Node.js needs your attention before npm can run";

    public string? Problem
    {
        get
        {
            if (IsChecking) return null;
            if (Selected is null) return _resolution is null || _resolution.IsReady ? null : _resolution.Problem + " " + _resolution.Remedy;
            if (Selected.Installation.NpmLauncher is null) return $"Node.js {Selected.Installation.Version} in '{Selected.Installation.NodeDirectory}' has no npm. Choose an installation that includes npm.";
            return Selected.Mismatch is null ? null : $"Node.js {Selected.Installation.Version} does not fit this project: {Selected.Mismatch}. Choose a matching installation.";
        }
    }

    public bool HasProblem => Problem is not null;
    /// <summary>What will happen to dependencies when a new project is created.</summary>
    public string InstallSummary => !InstallDependencies ? "Project dependencies will not be installed now; the first build installs them."
        : IsChecking || IsReady ? "Project dependencies will be installed after the project is created."
        : "Project dependencies cannot be installed until Node.js is set up.";
    public string? Requirement => _resolution?.Requirement.HasAny == true ? _resolution.Requirement.Describe() : null;
    public bool HasRequirement => Requirement is not null;

    public async Task RefreshAsync(string? projectFile, string projectDirectory, CancellationToken token)
    {
        _projectFile = projectFile;
        var previous = Selected?.Installation;
        var explicitChoice = IsExplicitChoice;
        IsChecking = true;
        try
        {
            var resolution = await service.ResolveEnvironmentAsync(projectFile, projectDirectory, token);
            var candidates = await service.GetCandidatesAsync(projectFile, token);
            token.ThrowIfCancellationRequested();
            _resolution = resolution;
            Choices.Clear();
            foreach (var candidate in candidates) Choices.Add(Choice(candidate));
            _automatic = Find(resolution.Installation) ?? Add(resolution.Installation);
            HasStoredSelection = resolution.Installation?.Source == NodeEnvironmentSource.Selected || resolution.Status == PcfEnvironmentStatus.SelectionUnavailable;
            // A choice made in this window survives a refresh; otherwise follow what resolution picked.
            _selected = (explicitChoice ? Find(previous) : null) ?? _automatic;
            OnPropertyChanged(nameof(Selected));
        }
        finally { IsChecking = false; }
        if (HasProblem) IsExpanded = true;
    }

    /// <summary>Adds an installation the developer browsed to and selects it.</summary>
    public async Task<bool> SelectDirectoryAsync(string directory, CancellationToken token)
    {
        var installation = await service.InspectDirectoryAsync(directory, token);
        if (installation is null) return false;
        Selected = Find(installation) ?? Add(installation);
        return true;
    }

    /// <summary>Remembers the selection for the project when it differs from the automatic choice.</summary>
    public void SaveSelection(string projectFile)
    {
        if (IsExplicitChoice && IsReady) service.Select(projectFile, Installation);
    }

    public void ClearSelection()
    {
        if (_projectFile is not null) service.Select(_projectFile, null);
    }

    public async Task<PcfInstallResult> InstallAsync(string projectFile, bool updateLockFile, CancellationToken token)
    {
        var node = Installation ?? throw new InvalidOperationException("Choose a Node.js installation first.");
        lock (_output) _output.Clear();
        Output = string.Empty;
        LastResult = null;
        IsInstalling = true;
        try
        {
            var result = await service.InstallAsync(projectFile, node, updateLockFile, new LineProgress(this), token);
            LastResult = result;
            return result;
        }
        finally { IsInstalling = false; }
    }

    // Called from npm's output threads; WPF marshals the resulting property change to the dialog itself.
    private void Append(string line)
    {
        log?.Invoke(line);
        lock (_output)
        {
            _output.Add(line);
            if (_output.Count > OutputLines) _output.RemoveRange(0, _output.Count - OutputLines);
            Output = string.Join(Environment.NewLine, _output);
        }
    }

    private PcfNodeChoice Choice(NodeInstallation installation)
    {
        string? mismatch = null;
        if (_resolution is { Requirement: { Conflict: null } requirement }) requirement.Check(installation.Version, out mismatch);
        return new PcfNodeChoice(installation, mismatch);
    }

    private PcfNodeChoice? Find(NodeInstallation? installation)
        => installation is null ? null : Choices.FirstOrDefault(choice => choice.Installation.IsSameEnvironment(installation));

    private PcfNodeChoice? Add(NodeInstallation? installation)
    {
        if (installation is null) return null;
        var choice = Choice(installation);
        Choices.Insert(0, choice);
        return choice;
    }

    private void Notify()
    {
        foreach (var name in new[] { nameof(Installation), nameof(IsReady), nameof(Summary), nameof(Problem), nameof(HasProblem), nameof(Requirement), nameof(HasRequirement), nameof(IsExplicitChoice), nameof(InstallSummary) })
            OnPropertyChanged(name);
    }

    private sealed class LineProgress(PcfSetupViewModel owner) : IProgress<string>
    {
        public void Report(string value) => owner.Append(value);
    }
}

/// <summary>Set Up PCF Build for an existing project.</summary>
internal sealed class PcfSetupDialogViewModel(string projectFile, IPcfSetupService service, PcfSetupViewModel setup) : PcfDialogViewModel
{
    private PcfSetupState? _state;
    private bool _complete;

    public string ProjectFile { get; } = projectFile;
    public string ProjectName => Path.GetFileNameWithoutExtension(ProjectFile);
    public PcfSetupViewModel Setup { get; } = setup;
    /// <summary>Nothing is left for setup to do; the footer then offers Close only.</summary>
    public bool IsComplete { get => _complete; private set => SetProperty(ref _complete, value); }

    public string PackageManager => _state is null ? string.Empty
        : _state.PackageManager.IsNpm ? "npm" + (_state.PackageManager.Evidence.Length == 0 ? string.Empty : $" ({_state.PackageManager.Evidence})")
        : _state.PackageManager.Kind == PcfPackageManagerKind.Conflict
            ? $"Unclear: lock files for {_state.PackageManager.Name} were found ({_state.PackageManager.Evidence}). Keep the lock file of the package manager you use. Xrm Tools will not run npm here and has not changed any file."
            : $"{_state.PackageManager.Name} ({_state.PackageManager.Evidence}). Xrm Tools prepares npm projects only, so it will not run npm here and has not changed any file. Install dependencies with {_state.PackageManager.Name}; to keep builds from running npm install, set PcfEnableAutoNpmInstall to false in the project file.";

    public string Dependencies => _state is null ? string.Empty
        : _state.SetupRunning && IsIdle ? "Setup is running for this project in another window."
        : !_state.PackageManager.IsNpm ? (_state.Dependencies.Status == PcfDependencyStatus.Missing ? "Not installed." : "Installed.")
        : _state.Dependencies.Status switch
        {
            PcfDependencyStatus.Installed => "Installed.",
            PcfDependencyStatus.Incomplete => $"A previous setup did not finish. Setup runs {InstallCommand} again.",
            _ => $"Not installed. Setup runs {InstallCommand}."
        };

    private string InstallCommand => _state!.Dependencies.HasLockFile
        ? "npm ci, which installs exactly what package-lock.json lists" : "npm install, because the project has no package-lock.json yet";

    protected override string CancellationStatus => "Setup was canceled. Dependencies may be only partly installed; run setup again to finish.";
    public void ReportCanceling() => Status = "Canceling setup and stopping npm…";

    public Task LoadAsync(CancellationToken token) => RunAsync(async () =>
    {
        Status = "Checking the project…";
        await RefreshAsync(token);
        Status = Describe();
        return "loaded";
    });

    /// <returns><c>true</c> when setup finished and the project is ready to build.</returns>
    public async Task<bool> ApplyAsync(bool updateLockFile, CancellationToken token) => await RunAsync(async () =>
    {
        if (!Setup.IsReady) throw new InvalidOperationException(Setup.Problem ?? "Choose a Node.js installation.");
        Setup.SaveSelection(ProjectFile);
        await RefreshAsync(token);
        var state = _state!;
        if (state.PackageManager.IsNpm && Setup.InstallDependencies && (state.Dependencies.Status != PcfDependencyStatus.Installed || updateLockFile))
        {
            Status = "Installing npm dependencies; this can take several minutes…";
            var result = await Setup.InstallAsync(ProjectFile, updateLockFile, token);
            // Re-read what is on disk before reporting, so a failed or canceled install is never shown as ready.
            await RefreshAsync(CancellationToken.None);
            if (result.Outcome == PcfInstallOutcome.Canceled) throw new OperationCanceledException();
            if (!result.Succeeded) throw new InvalidOperationException(string.Join(Environment.NewLine, new[] { result.Message }.Concat(result.Details)));
        }
        Status = Describe();
        return IsComplete ? "done" : "pending";
    }) == "done";

    private async Task RefreshAsync(CancellationToken token)
    {
        _state = await service.InspectAsync(ProjectFile, token);
        await Setup.RefreshAsync(ProjectFile, _state.ProjectDirectory, token);
        OnPropertyChanged(nameof(PackageManager));
        OnPropertyChanged(nameof(Dependencies));
    }

    private string Describe()
    {
        var state = _state!;
        IsComplete = false;
        if (state.Block is { } block) return block.Summary;
        if (!Setup.IsReady) return Setup.Problem ?? "Choose a Node.js installation.";
        if (!state.PackageManager.IsNpm)
            return state.Dependencies.Status == PcfDependencyStatus.Missing
                ? $"Node.js is ready. Install dependencies with {state.PackageManager.Name}, then build."
                : $"Node.js is ready and dependencies are present. Builds run npm install unless PcfEnableAutoNpmInstall is false in the project file.";
        if (state.Dependencies.Status != PcfDependencyStatus.Installed)
            return Setup.InstallDependencies ? "Choose Set up to install the project's dependencies." : "Node.js is ready. Dependencies are not installed; the next build installs them.";
        IsComplete = true;
        return $"'{ProjectName}' is ready to build with Node.js {Setup.Installation!.Version}.";
    }
}
#nullable restore
