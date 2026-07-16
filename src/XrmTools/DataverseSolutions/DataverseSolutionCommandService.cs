#nullable enable
namespace XrmTools.DataverseSolutions;

using Community.VisualStudio.Toolkit;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.Environments;
using XrmTools.Logging.Compatibility;

internal interface IDataverseSolutionCommandService
{
    bool IsBusy { get; }

    Task ExecuteAsync(DataverseSolutionCommandKind commandKind, CancellationToken cancellationToken);
}

[Export(typeof(IDataverseSolutionCommandService))]
[method: ImportingConstructor]
internal sealed class DataverseSolutionCommandService(
    ICdsProjectResolver cdsProjectResolver,
    IProcessCommandRunner processCommandRunner,
    IPacCli pacCli,
    IPacAuthBridge pacAuthBridge,
    IEnvironmentProvider environmentProvider,
    DataverseSolutionOutput output,
    ILogger<DataverseSolutionCommandService> logger) : IDataverseSolutionCommandService
{
    private int _isBusy;
    private readonly ICdsProjectResolver _cdsProjectResolver = cdsProjectResolver;
    private readonly IProcessCommandRunner _processCommandRunner = processCommandRunner;
    private readonly IPacCli _pacCli = pacCli;
    private readonly IPacAuthBridge _pacAuthBridge = pacAuthBridge;
    private readonly IEnvironmentProvider _environmentProvider = environmentProvider;
    private readonly DataverseSolutionOutput _output = output;
    private readonly ILogger<DataverseSolutionCommandService> _logger = logger;

    public bool IsBusy => Volatile.Read(ref _isBusy) == 1;

    public async Task ExecuteAsync(DataverseSolutionCommandKind commandKind, CancellationToken cancellationToken)
    {
        if (Interlocked.CompareExchange(ref _isBusy, 1, 0) != 0)
        {
            await VS.StatusBar.ShowMessageAsync("A Dataverse solution command is already running.");
            return;
        }

        await _output.ShowPaneAsync().ConfigureAwait(false);
        await VS.StatusBar.StartAnimationAsync(StatusAnimation.General).ConfigureAwait(false);
        try
        {
            var project = await _cdsProjectResolver.TryResolveSelectedProjectAsync(cancellationToken).ConfigureAwait(false)
                ?? throw new InvalidOperationException("The selected item is not a .cdsproj project.");

            await VS.StatusBar.ShowMessageAsync(GetInProgressMessage(commandKind, project.ProjectName)).ConfigureAwait(false);

            switch (commandKind)
            {
                case DataverseSolutionCommandKind.Synchronize:
                    await SynchronizeAsync(project, cancellationToken).ConfigureAwait(false);
                    break;
                case DataverseSolutionCommandKind.Import:
                    await ImportAsync(project, project.ConfigurationName, cancellationToken).ConfigureAwait(false);
                    break;
                case DataverseSolutionCommandKind.Pack:
                    await PackAsync(project, project.ConfigurationName, cancellationToken).ConfigureAwait(false);
                    break;
                case DataverseSolutionCommandKind.Unpack:
                    await UnpackAsync(project, project.ConfigurationName, cancellationToken).ConfigureAwait(false);
                    break;
                case DataverseSolutionCommandKind.DeployAndOpen:
                    await DeployAndOpenAsync(project, cancellationToken).ConfigureAwait(false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(commandKind), commandKind, null);
            }

            await VS.StatusBar.ShowMessageAsync(GetCompletedMessage(commandKind, project.ProjectName)).ConfigureAwait(false);
        }
        finally
        {
            Interlocked.Exchange(ref _isBusy, 0);
            await VS.StatusBar.EndAnimationAsync(StatusAnimation.General).ConfigureAwait(false);
        }
    }

    private async Task SynchronizeAsync(CdsProjectInfo project, CancellationToken cancellationToken)
    {
        var environment = await EnsureEnvironmentAsync(cancellationToken).ConfigureAwait(false);
        await _pacAuthBridge.EnsurePacProfileForCurrentEnvironmentAsync(cancellationToken).ConfigureAwait(false);

        var arguments = new List<string>
        {
            "solution",
            "sync",
            "--environment",
            environment.Url!,
            "--solution-folder",
            project.ProjectDirectory
        };

        AddMapArgument(arguments, project);
        await RunPacCommandAsync(project, environment.Url!, arguments, cancellationToken).ConfigureAwait(false);
    }

    private async Task ImportAsync(CdsProjectInfo project, string configurationName, CancellationToken cancellationToken)
    {
        await BuildProjectAsync(project, configurationName, cancellationToken).ConfigureAwait(false);
        var environment = await EnsureEnvironmentAsync(cancellationToken).ConfigureAwait(false);
        await _pacAuthBridge.EnsurePacProfileForCurrentEnvironmentAsync(cancellationToken).ConfigureAwait(false);

        await RunPacCommandAsync(
            project,
            environment.Url!,
            ["solution", "import", "--environment", environment.Url!],
            cancellationToken).ConfigureAwait(false);
    }

    private Task PackAsync(CdsProjectInfo project, string configurationName, CancellationToken cancellationToken)
        => BuildProjectAsync(project, configurationName, cancellationToken);

    private async Task UnpackAsync(CdsProjectInfo project, string configurationName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(project.SolutionPackageZipFilePath))
        {
            throw new InvalidOperationException("The .cdsproj did not evaluate SolutionPackageZipFilePath.");
        }

        if (!File.Exists(project.SolutionPackageZipFilePath))
        {
            await BuildProjectAsync(project, configurationName, cancellationToken).ConfigureAwait(false);
        }

        var arguments = new List<string>
        {
            "solution",
            "unpack",
            "--zipfile",
            project.SolutionPackageZipFilePath,
            "--folder",
            project.SolutionRootPath,
            "--packagetype",
            "Both"
        };

        AddMapArgument(arguments, project);
        await RunPacCommandAsync(project, environmentUrl: null, arguments, cancellationToken).ConfigureAwait(false);
    }

    private async Task DeployAndOpenAsync(CdsProjectInfo project, CancellationToken cancellationToken)
    {
        await BuildProjectAsync(project, "Debug", cancellationToken).ConfigureAwait(false);
        var environment = await EnsureEnvironmentAsync(cancellationToken).ConfigureAwait(false);
        await _pacAuthBridge.EnsurePacProfileForCurrentEnvironmentAsync(cancellationToken).ConfigureAwait(false);

        await RunPacCommandAsync(
            project,
            environment.Url!,
            ["solution", "import", "--environment", environment.Url!],
            cancellationToken).ConfigureAwait(false);

        Process.Start(new ProcessStartInfo(environment.Url!)
        {
            UseShellExecute = true
        });
    }

    private async Task<DataverseEnvironment> EnsureEnvironmentAsync(CancellationToken cancellationToken)
    {
        var environment = await _environmentProvider.GetActiveEnvironmentAsync(true).ConfigureAwait(false);
        if (environment is null || !environment.IsValid || string.IsNullOrWhiteSpace(environment.Url))
        {
            throw new InvalidOperationException("No active Dataverse environment is available.");
        }

        return environment;
    }

    private async Task BuildProjectAsync(CdsProjectInfo project, string configurationName, CancellationToken cancellationToken)
    {
        var proj = await VS.Solutions.GetActiveProjectAsync().ConfigureAwait(false);
        var isUpToDate = await VS.Build.ProjectIsUpToDateAsync(proj).ConfigureAwait(false);
        if (!isUpToDate)
        {
            var isSuccess = await VS.Build.BuildProjectAsync(proj, BuildAction.Rebuild).ConfigureAwait(false);
            if (!isSuccess)
            {
                throw new InvalidOperationException("Project build failed. Please check the build output for details.");
            }
        }
    }
    private async Task BuildProjectCliAsync(CdsProjectInfo project, string configurationName, CancellationToken cancellationToken)
    { 
        var request = new ProcessCommandRequest
        {
            FileName = "dotnet",
            WorkingDirectory = project.ProjectDirectory,
            Arguments =
            [
                "msbuild",
                project.ProjectFilePath,
                "-restore",
                "-nologo",
                "-verbosity:minimal",
                "-target:Build",
                $"-property:Configuration={configurationName}"
            ]
        };

        _output.WriteHeader(project, environmentUrl: null, ProcessCommandFormatting.FormatCommand(request));
        await VS.StatusBar.ShowMessageAsync($"Building {project.ProjectName}...").ConfigureAwait(false);
        var result = await _processCommandRunner.RunAsync(request, _output.CreateProgress(), cancellationToken).ConfigureAwait(false);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException($"Build failed with exit code {result.ExitCode}.");
        }

        _output.WriteCompleted();
        _output.WriteBlankLine();
    }

    private async Task RunPacCommandAsync(
        CdsProjectInfo project,
        string? environmentUrl,
        IReadOnlyList<string> arguments,
        CancellationToken cancellationToken)
    {
        var request = (_pacCli as PacCli)?.CreatePacCommandRequest(arguments, project.ProjectDirectory, null);
        if (request is not null)
        {
            _output.WriteHeader(project, environmentUrl, ProcessCommandFormatting.FormatCommand(request));
        }

        await VS.StatusBar.ShowMessageAsync($"Running {commandName(arguments)} for {project.ProjectName}...").ConfigureAwait(false);
        var result = await _pacCli.RunSolutionCommandAsync(project, arguments, _output.CreateProgress(), cancellationToken).ConfigureAwait(false);
        if (!result.Succeeded)
        {
            _logger.LogError("PAC solution command failed.");
            throw new InvalidOperationException($"PAC solution command failed with exit code {result.ExitCode}.");
        }

        _output.WriteCompleted();
        _output.WriteBlankLine();
    }

    private static void AddMapArgument(List<string> arguments, CdsProjectInfo project)
    {
        if (!string.IsNullOrWhiteSpace(project.SolutionPackageMapFilePath)
            && File.Exists(project.SolutionPackageMapFilePath))
        {
            arguments.Add("--map");
            arguments.Add(project.SolutionPackageMapFilePath!);
        }
    }

    private static string GetInProgressMessage(DataverseSolutionCommandKind commandKind, string projectName)
        => commandKind switch
        {
            DataverseSolutionCommandKind.Synchronize => $"Synchronizing {projectName} with Dataverse...",
            DataverseSolutionCommandKind.Import => $"Importing {projectName} to Dataverse...",
            DataverseSolutionCommandKind.Pack => $"Packing {projectName}...",
            DataverseSolutionCommandKind.Unpack => $"Unpacking {projectName}...",
            DataverseSolutionCommandKind.DeployAndOpen => $"Deploying {projectName} and opening Dataverse...",
            _ => $"Running Dataverse solution command for {projectName}..."
        };

    private static string GetCompletedMessage(DataverseSolutionCommandKind commandKind, string projectName)
        => commandKind switch
        {
            DataverseSolutionCommandKind.Synchronize => $"Synchronized {projectName} with Dataverse.",
            DataverseSolutionCommandKind.Import => $"Imported {projectName} to Dataverse.",
            DataverseSolutionCommandKind.Pack => $"Packed {projectName}.",
            DataverseSolutionCommandKind.Unpack => $"Unpacked {projectName}.",
            DataverseSolutionCommandKind.DeployAndOpen => $"Deployed {projectName} and opened Dataverse.",
            _ => $"Completed Dataverse solution command for {projectName}."
        };

    private static string commandName(IReadOnlyList<string> arguments)
    {
        if (arguments.Count >= 2)
        {
            return $"{arguments[0]} {arguments[1]}";
        }

        return arguments.Count == 1 ? arguments[0] : "command";
    }
}
#nullable restore
