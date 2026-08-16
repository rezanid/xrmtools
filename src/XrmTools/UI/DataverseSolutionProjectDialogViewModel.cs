#nullable enable
namespace XrmTools.UI;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using XrmTools.DataverseSolutions;
using XrmTools.WebApi.Entities;

internal sealed class DataverseSolutionProjectDialogViewModel : ViewModelBase
{
    private bool _createEmptyProject;
    private string _parentDirectory;
    private string _projectName = string.Empty;
    private string _publisherName = string.Empty;
    private string _publisherPrefix = string.Empty;
    private Solution? _selectedSolution;
    private string? _solutionLoadError;

    public DataverseSolutionProjectDialogViewModel(string parentDirectory)
    {
        _parentDirectory = parentDirectory;
        Solutions = [];
    }

    public ObservableCollection<Solution> Solutions { get; }

    public bool CreateEmptyProject
    {
        get => _createEmptyProject;
        set
        {
            if (SetProperty(ref _createEmptyProject, value))
            {
                OnPropertyChanged(nameof(CloneExistingSolution));
                OnPropertyChanged(nameof(EmptyProjectVisibility));
                OnPropertyChanged(nameof(CloneProjectVisibility));
            }
        }
    }

    public bool CloneExistingSolution
    {
        get => !CreateEmptyProject;
        set
        {
            if (value)
            {
                CreateEmptyProject = false;
            }
        }
    }

    public System.Windows.Visibility EmptyProjectVisibility
        => CreateEmptyProject ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

    public System.Windows.Visibility CloneProjectVisibility
        => CloneExistingSolution ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

    public string ProjectName
    {
        get => _projectName;
        set => SetProperty(ref _projectName, value);
    }

    public string ParentDirectory
    {
        get => _parentDirectory;
        set
        {
            if (SetProperty(ref _parentDirectory, value))
            {
                OnPropertyChanged(nameof(CloneDestinationPath));
            }
        }
    }

    public string CloneDestinationPath
    {
        get
        {
            var solutionUniqueName = SelectedSolution?.UniqueName?.Trim();
            return string.IsNullOrWhiteSpace(ParentDirectory) || string.IsNullOrWhiteSpace(solutionUniqueName)
                ? string.Empty
                : Path.Combine(ParentDirectory, solutionUniqueName!);
        }
    }

    public string PublisherName
    {
        get => _publisherName;
        set => SetProperty(ref _publisherName, value);
    }

    public string PublisherPrefix
    {
        get => _publisherPrefix;
        set => SetProperty(ref _publisherPrefix, value);
    }

    public Solution? SelectedSolution
    {
        get => _selectedSolution;
        set
        {
            if (SetProperty(ref _selectedSolution, value))
            {
                OnPropertyChanged(nameof(CloneDestinationPath));
            }
        }
    }

    public string? SolutionLoadError
    {
        get => _solutionLoadError;
        private set => SetProperty(ref _solutionLoadError, value);
    }

    public void SetSolutions(IEnumerable<Solution> solutions)
    {
        Solutions.Clear();
        foreach (var solution in solutions.Where(solution => !string.IsNullOrWhiteSpace(solution.UniqueName)))
        {
            Solutions.Add(solution);
        }

        SelectedSolution = Solutions.FirstOrDefault();
        SolutionLoadError = Solutions.Count == 0 ? "No unmanaged solutions were found in the active environment." : null;
    }

    public void SetSolutionLoadError(string message)
        => SolutionLoadError = message;

    public bool TryCreateRequest(out DataverseSolutionProjectCreationRequest? request, out string? validationError)
    {
        request = null;
        validationError = null;

        if (CloneExistingSolution && string.IsNullOrWhiteSpace(SelectedSolution?.UniqueName))
        {
            validationError = SolutionLoadError ?? "Select an unmanaged solution to clone.";
            return false;
        }

        var projectName = CreateEmptyProject ? ProjectName.Trim() : SelectedSolution!.UniqueName!.Trim();
        if (string.IsNullOrWhiteSpace(projectName) || projectName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            validationError = CreateEmptyProject
                ? "Enter a valid project name."
                : "The selected solution does not have a valid project name.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(ParentDirectory) || !Directory.Exists(ParentDirectory))
        {
            validationError = "Select an existing project location.";
            return false;
        }

        var projectDirectory = Path.Combine(ParentDirectory, projectName);
        if (Directory.Exists(projectDirectory) || File.Exists(projectDirectory))
        {
            validationError = $"A file or folder named '{projectName}' already exists in the selected location.";
            return false;
        }

        if (CreateEmptyProject && (string.IsNullOrWhiteSpace(PublisherName) || string.IsNullOrWhiteSpace(PublisherPrefix)))
        {
            validationError = "Publisher name and publisher prefix are required for an empty solution project.";
            return false;
        }

        request = new DataverseSolutionProjectCreationRequest
        {
            Mode = CreateEmptyProject ? DataverseSolutionProjectCreationMode.Empty : DataverseSolutionProjectCreationMode.Clone,
            ProjectName = projectName,
            ParentDirectory = Path.GetFullPath(ParentDirectory),
            PublisherName = CreateEmptyProject ? PublisherName.Trim() : null,
            PublisherPrefix = CreateEmptyProject ? PublisherPrefix.Trim() : null,
            SolutionUniqueName = CloneExistingSolution ? SelectedSolution!.UniqueName : null
        };
        return true;
    }
}
#nullable restore
