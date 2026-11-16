#nullable enable
namespace XrmTools.UI;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.DataverseSolutions;
using XrmTools.Pcf.Setup;
using XrmTools.WebApi.Entities;

internal abstract class PcfDialogViewModel : ViewModelBase
{
    private bool _busy;
    private string? _status;
    public bool IsIdle => !_busy;
    public bool IsBusy => _busy;
    public string? Status { get => _status; protected set => SetProperty(ref _status, value); }
    protected virtual string CancellationStatus => "Operation canceled.";
    public void ReportFailure(Exception exception)
        => Status = exception is OperationCanceledException ? CancellationStatus : exception.Message;
    protected async Task<T?> RunAsync<T>(Func<Task<T>> action) where T : class
    {
        if (_busy) return null;
        _busy = true; OnPropertyChanged(nameof(IsIdle)); OnPropertyChanged(nameof(IsBusy)); Status = "Working… See the XrmTools output pane for progress.";
        try { return await action(); }
        catch (OperationCanceledException) { Status = CancellationStatus; return null; }
        catch (Exception ex) { Status = ex.Message; return null; }
        finally { _busy = false; OnPropertyChanged(nameof(IsIdle)); OnPropertyChanged(nameof(IsBusy)); }
    }
}

internal sealed class PcfCreateDialogViewModel(string location, PcfSetupViewModel? setup = null) : PcfDialogViewModel
{
    private string _location = location;
    private string _projectName = string.Empty;
    private string? _createdProject;
    private bool _setupIncomplete;
    private bool _installStarted;
    public string Location { get => _location; set => SetProperty(ref _location, value); }
    public string ProjectName { get => _projectName; set => SetProperty(ref _projectName, value); }
    public string ControlName { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public string Template { get; set; } = "field";
    public string Framework { get; set; } = "none";
    /// <summary>Node.js and dependency setup; <c>null</c> only where the wizard runs without it.</summary>
    public PcfSetupViewModel? Setup { get; } = setup;
    /// <summary>Set once PAC has generated the project, even if a later step failed.</summary>
    public string? CreatedProject
    {
        get => _createdProject;
        private set { if (SetProperty(ref _createdProject, value)) { OnPropertyChanged(nameof(CanEdit)); OnPropertyChanged(nameof(IsCreated)); } }
    }
    public bool IsCreated => CreatedProject is not null;
    public bool CanEdit => !IsCreated;
    /// <summary>The project exists, but the dependencies the developer asked for are not installed.</summary>
    public bool SetupIncomplete { get => _setupIncomplete; private set => SetProperty(ref _setupIncomplete, value); }
    protected override string CancellationStatus => IsCreated
        ? "Setup was canceled. The project was created; dependencies may be only partly installed. Retry setup removes them and installs again."
        : "Creation was canceled. Any generated files were left in the project folder.";

    /// <summary>The folder the project will be created in, used to find Node.js declarations in the surrounding repository.</summary>
    public string TargetDirectory => System.IO.Path.Combine(Location ?? string.Empty, ProjectName ?? string.Empty);
    public void ReportCanceling() => Status = IsCreated ? "Canceling setup and stopping npm…" : "Canceling project creation…";

    /// <returns>The project to add to the solution when everything requested was done; otherwise <c>null</c>.</returns>
    public Task<string?> CreateAsync(Func<PcfCreateRequest, CancellationToken, Task<string>> create, CancellationToken token)
        => RunAsync(async () =>
        {
            if (CreatedProject is null)
            {
                Status = "Creating the project with pac pcf init…";
                // Dependencies are installed as a separate step so that step can use the selected Node.js, be canceled
                // with all of its child processes, and fail without losing the created project.
                CreatedProject = await create(new PcfCreateRequest { ParentDirectory = Location, ProjectName = ProjectName, ControlName = ControlName, Namespace = Namespace, Template = Template, Framework = Framework }, token);
                Setup?.SaveSelection(CreatedProject);
            }
            if (Setup is null || !Setup.InstallDependencies) { SetupIncomplete = false; return CreatedProject; }
            SetupIncomplete = true;
            // Checked again for the folder the project really is in, and on every retry, so a Node.js installed or
            // selected after a failure is picked up without reopening the wizard.
            await Setup.RefreshAsync(CreatedProject, System.IO.Path.GetDirectoryName(CreatedProject)!, token);
            if (!Setup.IsReady)
                throw new InvalidOperationException($"The project was created, but dependencies were not installed. {Setup.Problem ?? "Choose a Node.js installation."} Then choose Retry setup, or open the project and set it up later.");
            Status = "Project created. Installing npm dependencies; this can take several minutes…";
            // A retry starts from nothing: what an interrupted npm install left in this new project cannot be built on.
            var retry = _installStarted;
            _installStarted = true;
            var result = await Setup.InstallAsync(CreatedProject, false, retry, token);
            if (result.Outcome == PcfInstallOutcome.Canceled) throw new OperationCanceledException();
            if (!result.Succeeded)
                throw new InvalidOperationException($"The project was created, but setup is incomplete. {result.Message} Choose Retry setup, or open the project and set it up later.");
            SetupIncomplete = false;
            return CreatedProject;
        });
}

internal sealed class PcfPushDialogViewModel(string projectPath, string environmentUrl) : PcfDialogViewModel
{
    private bool _useExistingSolution;
    private bool _solutionsLoaded;
    private bool _showDestinationOptions = true;
    private bool _requireExplicitSelection;
    private bool _lastOperationWasPush;
    private string? _destinationNotice;
    private IReadOnlyList<PcfSolutionDestination> _references = [];
    private Solution? _selectedSolution;
    public string EnvironmentUrl { get; } = environmentUrl;
    public string PublisherPrefix { get; set; } = string.Empty;
    public ObservableCollection<Solution> Solutions { get; } = [];
    public Solution? SelectedSolution
    {
        get => _selectedSolution;
        set { if (SetProperty(ref _selectedSolution, value)) OnPropertyChanged(nameof(DestinationDisplayName)); }
    }
    public string DestinationDisplayName => SelectedSolution is null ? "Select a destination below." : $"{SelectedSolution.FriendlyName} ({SelectedSolution.UniqueName})";
    public string? DestinationNotice { get => _destinationNotice; private set => SetProperty(ref _destinationNotice, value); }
    public bool ShowDestinationOptions
    {
        get => _showDestinationOptions;
        private set
        {
            if (!SetProperty(ref _showDestinationOptions, value)) return;
            OnPropertyChanged(nameof(ShowProjectDestination));
            OnPropertyChanged(nameof(ShowProjectRefresh));
            OnPropertyChanged(nameof(ShowSolutionPicker));
            OnPropertyChanged(nameof(SolutionChoices));
        }
    }
    public bool ShowProjectDestination => !ShowDestinationOptions;
    public bool ShowProjectRefresh => ShowProjectDestination && ReferencedSolutionNames.Count == 1;
    public bool ShowSolutionPicker => UseExistingSolution && (ShowDestinationOptions || ReferencedSolutionNames.Count > 1);
    private IReadOnlyList<string> ReferencedSolutionNames => _references.Select(reference => reference.SolutionUniqueName)
        .Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    public IEnumerable<Solution> SolutionChoices => ShowDestinationOptions ? Solutions
        : Solutions.Where(solution => _references.Any(reference => string.Equals(reference.SolutionUniqueName, solution.UniqueName, StringComparison.OrdinalIgnoreCase)));

    public Task InitializeAsync(Func<CancellationToken, Task<IReadOnlyList<PcfSolutionDestination>>> findReferences,
        Func<CancellationToken, Task<IEnumerable<Solution>>> load, CancellationToken token)
        => RunAsync(async () =>
        {
            _lastOperationWasPush = false;
            Status = "Checking solution project references...";
            try
            {
                token.ThrowIfCancellationRequested();
                _references = await findReferences(token);
                token.ThrowIfCancellationRequested();
                if (_references.Count == 0) { Status = null; return "ready"; }
                _requireExplicitSelection = true;
                UseExistingSolution = true;
                ShowDestinationOptions = false;
                await LoadSolutionListAsync(load, token);
                return "ready";
            }
            catch (Exception ex)
            {
                _requireExplicitSelection = true;
                SelectedSolution = null;
                UseExistingSolution = true;
                ShowDestinationOptions = true;
                DestinationNotice = ex is OperationCanceledException
                    ? "Destination detection was canceled. Choose a destination explicitly."
                    : $"The project destination could not be verified: {ex.Message} Choose a destination explicitly; no alternative has been selected.";
                throw;
            }
        });

    public void ChangeDestination()
    {
        ShowDestinationOptions = true;
        DestinationNotice = "Choose an unmanaged solution or a publisher prefix explicitly. This changes only the push destination, not project references.";
    }

    public Task LoadSolutionsAsync(Func<CancellationToken, Task<IEnumerable<Solution>>> load, CancellationToken token, bool refresh = false)
    {
        if (!UseExistingSolution || (_solutionsLoaded && !refresh)) return Task.CompletedTask;
        return RunAsync(async () => { await LoadSolutionListAsync(load, token); return "loaded"; });
    }

    private async Task LoadSolutionListAsync(Func<CancellationToken, Task<IEnumerable<Solution>>> load, CancellationToken token)
    {
        _lastOperationWasPush = false;
        token.ThrowIfCancellationRequested();
        Status = "Loading unmanaged solutions from the selected environment...";
        var previousName = SelectedSolution?.UniqueName;
        _solutionsLoaded = false;
        SelectedSolution = null;
        Solutions.Clear();
        OnPropertyChanged(nameof(SolutionChoices));
        try
        {
            var solutions = await load(token);
            token.ThrowIfCancellationRequested();
            foreach (var solution in solutions.Where(item => !string.IsNullOrWhiteSpace(item.UniqueName)))
                Solutions.Add(solution);
            OnPropertyChanged(nameof(SolutionChoices));
            if (!ShowDestinationOptions) ApplyProjectDestination(previousName);
            else
            {
                SelectedSolution = previousName is not null
                    ? Solutions.FirstOrDefault(item => string.Equals(item.UniqueName, previousName, StringComparison.OrdinalIgnoreCase))
                    : _requireExplicitSelection ? null : Solutions.FirstOrDefault();
                if (previousName is not null && SelectedSolution is null) _requireExplicitSelection = true;
                if (_references.Count > 0) DestinationNotice = "Choose a destination explicitly. Only unmanaged solutions in the active environment are listed.";
            }
            _solutionsLoaded = true;
            Status = Solutions.Count == 0 ? "No unmanaged solutions were found. Refresh to retry, or use a publisher prefix."
                : previousName is not null && SelectedSolution is null ? "The previous destination is no longer available. Select a destination explicitly." : null;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new InvalidOperationException($"Could not load unmanaged solutions: {ex.Message} Use Refresh to retry, or use a publisher prefix.", ex);
        }
    }

    private void ApplyProjectDestination(string? previousName)
    {
        var choices = SolutionChoices.ToArray();
        var referencedNames = _references.Select(reference => reference.SolutionUniqueName)
            .Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        var projects = string.Join(", ", _references.Select(reference => $"{System.IO.Path.GetFileName(reference.ProjectFilePath)} ({reference.SolutionUniqueName})"));
        if (referencedNames.Length == 1)
        {
            SelectedSolution = choices.SingleOrDefault();
            DestinationNotice = $"Inferred from {projects}.";
        }
        else
        {
            SelectedSolution = choices.FirstOrDefault(item => string.Equals(item.UniqueName, previousName, StringComparison.OrdinalIgnoreCase));
            DestinationNotice = $"Multiple solution projects reference this control. Choose a destination below, or use Change. Related projects: {projects}.";
        }
        var missing = _references.Where(reference => !choices.Any(solution => string.Equals(solution.UniqueName, reference.SolutionUniqueName, StringComparison.OrdinalIgnoreCase))).ToArray();
        if (missing.Length > 0)
        {
            _requireExplicitSelection = true;
            DestinationNotice += $" Not available as unmanaged solutions in the active environment: {string.Join(", ", missing.Select(reference => reference.SolutionUniqueName))}. Refresh to retry, or use Change to choose explicitly.";
        }
        if (choices.Length == 0) ShowDestinationOptions = true;
    }

    public bool UseExistingSolution
    {
        get => _useExistingSolution;
        set
        {
            if (!SetProperty(ref _useExistingSolution, value)) return;
            OnPropertyChanged(nameof(UsePublisherPrefix));
            OnPropertyChanged(nameof(ShowSolutionPicker));
        }
    }
    public bool UsePublisherPrefix
    {
        get => !UseExistingSolution;
        set { if (value) UseExistingSolution = false; }
    }
    public bool Incremental { get; set; }
    protected override string CancellationStatus => _lastOperationWasPush
        ? "Local PAC operation canceled. Dataverse changes already submitted may still complete." : "Solution loading canceled. No deployment was started.";
    public void ReportCanceling() => Status = _lastOperationWasPush
        ? "Canceling the local PAC operation. Dataverse changes already submitted may still complete." : "Canceling solution loading...";
    public Task<string?> PushAsync(Func<PcfPushRequest, CancellationToken, IProgress<string>?, Task> push, CancellationToken token)
        => RunAsync(async () =>
        {
            _lastOperationWasPush = true;
            token.ThrowIfCancellationRequested();
            var request = new PcfPushRequest
            {
                ProjectFilePath = projectPath, EnvironmentUrl = EnvironmentUrl,
                PublisherPrefix = UseExistingSolution ? string.Empty : PublisherPrefix,
                SolutionUniqueName = UseExistingSolution ? SelectedSolution?.UniqueName : null, Incremental = Incremental
            };
            if (UseExistingSolution && (SelectedSolution is null || !SolutionChoices.Contains(SelectedSolution)))
                throw new InvalidOperationException("Select an unmanaged solution from the list. Use Refresh if the list could not be loaded.");
            PcfArguments.Push(request);
            var active = true;
            var progress = new Progress<string>(message =>
            {
                if (active && !token.IsCancellationRequested) Status = message;
            });
            try
            {
                await push(request, token, progress);
                token.ThrowIfCancellationRequested();
                return "done";
            }
            finally { active = false; }
        });
}
#nullable restore
