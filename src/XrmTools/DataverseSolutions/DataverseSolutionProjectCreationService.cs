#nullable enable
namespace XrmTools.DataverseSolutions;

using Community.VisualStudio.Toolkit;
using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.Environments;
using XrmTools.Options;

internal interface IDataverseSolutionProjectCreationService
{
    bool IsBusy { get; }

    Task<string> CreateAsync(DataverseSolutionProjectCreationRequest request, CancellationToken cancellationToken);
}

[Export(typeof(IDataverseSolutionProjectCreationService))]
[method: ImportingConstructor]
internal sealed class DataverseSolutionProjectCreationService(
    IPacCli pacCli,
    IPacAuthBridge pacAuthBridge,
    IEnvironmentProvider environmentProvider,
    IDataverseSolutionProjectFileService projectFileService,
    DataverseSolutionOutput output) : IDataverseSolutionProjectCreationService
{
    private readonly IPacCli _pacCli = pacCli;
    private readonly IPacAuthBridge _pacAuthBridge = pacAuthBridge;
    private readonly IEnvironmentProvider _environmentProvider = environmentProvider;
    private readonly IDataverseSolutionProjectFileService _projectFileService = projectFileService;
    private readonly DataverseSolutionOutput _output = output;
    private int _isBusy;

    public bool IsBusy => Volatile.Read(ref _isBusy) == 1;

    public async Task<string> CreateAsync(DataverseSolutionProjectCreationRequest request, CancellationToken cancellationToken)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));
        if (Interlocked.CompareExchange(ref _isBusy, 1, 0) != 0)
        {
            throw new InvalidOperationException("A Dataverse solution project is already being created.");
        }

        var projectDirectory = Path.Combine(request.ParentDirectory, request.ProjectName);
        var cloneOutputDirectory = request.Mode == DataverseSolutionProjectCreationMode.Clone
            ? Path.Combine(request.ParentDirectory, ".xpc-" + Guid.NewGuid().ToString("N").Substring(0, 12))
            : null;
        if (Directory.Exists(projectDirectory) || File.Exists(projectDirectory))
        {
            Interlocked.Exchange(ref _isBusy, 0);
            throw new InvalidOperationException($"The project path '{projectDirectory}' already exists.");
        }

        await _output.ShowPaneAsync().ConfigureAwait(false);
        await VS.StatusBar.StartAnimationAsync(StatusAnimation.General).ConfigureAwait(false);
        try
        {
            var projectSdk = (await GeneralOptions.GetLiveInstanceAsync().ConfigureAwait(false)).DataverseSolutionProjectSdk;
            var project = new CdsProjectInfo
            {
                ProjectName = request.ProjectName,
                ProjectDirectory = projectDirectory,
                ProjectFilePath = Path.Combine(projectDirectory, request.ProjectName + ".cdsproj")
            };

            ProcessCommandResult result;
            if (request.Mode == DataverseSolutionProjectCreationMode.Empty)
            {
                _output.WriteHeader(project, environmentUrl: null, "pac solution init");
                result = await _pacCli.InitializeSolutionAsync(
                    new PacSolutionInitRequest
                    {
                        OutputDirectory = projectDirectory,
                        PublisherName = request.PublisherName!,
                        PublisherPrefix = request.PublisherPrefix!
                    },
                    _output.CreateProgress(),
                    cancellationToken).ConfigureAwait(false);
            }
            else
            {
                var environment = await EnsureEnvironmentAsync().ConfigureAwait(false);
                await _pacAuthBridge.EnsurePacProfileForCurrentEnvironmentAsync(cancellationToken).ConfigureAwait(false);
                _output.WriteHeader(project, environment.Url, $"pac solution clone --name {request.SolutionUniqueName}");
                result = await _pacCli.CloneSolutionAsync(
                    new PacSolutionCloneRequest
                    {
                        OutputDirectory = cloneOutputDirectory!,
                        SolutionUniqueName = request.SolutionUniqueName!,
                        EnvironmentUrl = environment.Url
                    },
                    _output.CreateProgress(),
                    cancellationToken).ConfigureAwait(false);
            }

            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"PAC CLI failed with exit code {result.ExitCode}.");
            }

            if (cloneOutputDirectory is not null)
            {
                _projectFileService.CopyGeneratedProjectDirectory(cloneOutputDirectory, projectDirectory);
                TryCleanupDirectory(cloneOutputDirectory);
            }

            var projectFilePath = _projectFileService.FinalizeProjectFile(projectDirectory, request.ProjectName, projectSdk);
            _output.WriteCompleted();
            _output.WriteBlankLine();
            await VS.StatusBar.ShowMessageAsync($"Created {request.ProjectName}.").ConfigureAwait(false);
            return projectFilePath;
        }
        catch
        {
            TryCleanupDirectory(projectDirectory);
            TryCleanupDirectory(cloneOutputDirectory);

            throw;
        }
        finally
        {
            Interlocked.Exchange(ref _isBusy, 0);
            await VS.StatusBar.EndAnimationAsync(StatusAnimation.General).ConfigureAwait(false);
        }
    }

    private async Task<DataverseEnvironment> EnsureEnvironmentAsync()
    {
        var environment = await _environmentProvider.GetActiveEnvironmentAsync(true).ConfigureAwait(false);
        if (environment is null || !environment.IsValid || string.IsNullOrWhiteSpace(environment.Url))
        {
            throw new InvalidOperationException("No active Dataverse environment is available.");
        }

        return environment;
    }

    private void TryCleanupDirectory(string? directoryPath)
    {
        if (string.IsNullOrWhiteSpace(directoryPath))
        {
            return;
        }

        var cleanupException = _projectFileService.TryDeleteDirectory(directoryPath!);
        if (cleanupException is not null)
        {
            _output.WriteWarning($"Could not remove temporary directory '{directoryPath}': {cleanupException.Message}");
        }
    }
}
#nullable restore
