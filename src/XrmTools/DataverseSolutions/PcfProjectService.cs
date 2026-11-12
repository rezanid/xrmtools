#nullable enable
namespace XrmTools.DataverseSolutions;

using Community.VisualStudio.Toolkit;
using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.Environments;

internal interface IPcfProjectService
{
    bool IsBusy { get; }
    Task<string> CreateAsync(PcfCreateRequest request, CancellationToken token);
    Task PushAsync(PcfPushRequest request, CancellationToken token, IProgress<string>? progress = null);
}

internal interface IPcfOperationUi
{
    Task ShowPaneAsync();
    Task StartAsync();
    Task EndAsync();
    void WriteHeader(CdsProjectInfo project, string? environmentUrl, string command);
    IProgress<ProcessOutputLine> CreateProgress();
    void WriteCompleted();
    void WriteWarning(string message);
}

[Export(typeof(IPcfOperationUi))]
[method: ImportingConstructor]
internal sealed class PcfOperationUi(DataverseSolutionOutput output) : IPcfOperationUi
{
    public Task ShowPaneAsync() => output.ShowPaneAsync();
    public Task StartAsync() => VS.StatusBar.StartAnimationAsync(StatusAnimation.General);
    public Task EndAsync() => VS.StatusBar.EndAnimationAsync(StatusAnimation.General);
    public void WriteHeader(CdsProjectInfo project, string? environmentUrl, string command) => output.WriteHeader(project, environmentUrl, command);
    public IProgress<ProcessOutputLine> CreateProgress() => output.CreateProgress();
    public void WriteCompleted() => output.WriteCompleted();
    public void WriteWarning(string message) => output.WriteWarning(message);
}

[Export(typeof(IPcfProjectService))]
[method: ImportingConstructor]
internal sealed class PcfProjectService(IPacCli pac, IPacAuthBridge auth, IEnvironmentProvider environments,
    IPcfOperationUi output, DataverseSolutionOperationState state) : IPcfProjectService
{
    public bool IsBusy => state.IsBusy;

    public async Task<string> CreateAsync(PcfCreateRequest request, CancellationToken token)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));
        var arguments = PcfArguments.Create(request);
        var directory = Path.GetFullPath(Path.Combine(request.ParentDirectory, request.ProjectName));
        if (!Directory.Exists(request.ParentDirectory)) throw new InvalidOperationException("The location must be an existing directory.");
        if (Directory.Exists(directory) || File.Exists(directory)) throw new InvalidOperationException($"The project path '{directory}' already exists.");
        if (!state.TryEnter()) throw new InvalidOperationException("Another Dataverse operation is already running.");
        try
        {
            token.ThrowIfCancellationRequested();
            // Preserve generated files on failure so user data is never silently removed.
            Directory.CreateDirectory(directory);
            await output.ShowPaneAsync().ConfigureAwait(false);
            output.WriteHeader(Project(directory, request.ProjectName), null, "pac pcf init");
            await output.StartAsync().ConfigureAwait(false);
            try
            {
                var result = await pac.RunPcfCommandAsync(directory, arguments, output.CreateProgress(), token).ConfigureAwait(false);
                if (!result.Succeeded) throw new InvalidOperationException($"PAC exited with code {result.ExitCode}.");
                var projects = Directory.GetFiles(directory, "*.pcfproj", SearchOption.TopDirectoryOnly);
                if (projects.Length != 1) throw new InvalidOperationException($"PAC did not generate exactly one PCF project in '{directory}'.");
                output.WriteCompleted();
                return projects[0];
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                throw new InvalidOperationException($"PCF creation failed. Generated files, if any, were retained in '{directory}'. {ex.Message}", ex);
            }
            finally { await output.EndAsync().ConfigureAwait(false); }
        }
        finally { state.Exit(); }
    }

    public async Task PushAsync(PcfPushRequest request, CancellationToken token, IProgress<string>? progress = null)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));
        var arguments = PcfArguments.Push(request);
        progress?.Report("Checking the selected environment...");
        if (!File.Exists(request.ProjectFilePath) || !string.Equals(Path.GetExtension(request.ProjectFilePath), ".pcfproj", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Select an existing PCF project.");
        if (!state.TryEnter()) throw new InvalidOperationException("Another Dataverse operation is already running.");
        try
        {
            token.ThrowIfCancellationRequested();
            var environment = await environments.GetActiveEnvironmentAsync(true).ConfigureAwait(false);
            if (environment is null || !environment.IsValid || !DataverseEnvironmentUrl.TryNormalize(environment.Url, out var activeUrl)
                || !string.Equals(activeUrl, DataverseEnvironmentUrl.Normalize(request.EnvironmentUrl), StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("The active environment changed. Select the intended environment and reopen Push to Environment.");
            await output.ShowPaneAsync().ConfigureAwait(false);
            var directory = Path.GetDirectoryName(Path.GetFullPath(request.ProjectFilePath))!;
            var projects = Directory.GetFiles(directory, "*.pcfproj", SearchOption.TopDirectoryOnly);
            if (projects.Length != 1 || !string.Equals(Path.GetFullPath(projects[0]), Path.GetFullPath(request.ProjectFilePath), StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("PAC push requires the selected PCF project to be the only PCF project in its directory.");
            output.WriteHeader(Project(directory, Path.GetFileNameWithoutExtension(request.ProjectFilePath)), request.EnvironmentUrl, "pac pcf push (development deployment)");
            token.ThrowIfCancellationRequested();
            progress?.Report("Checking PAC authentication. If a browser sign-in page opens, complete sign-in there. You can cancel here.");
            await auth.EnsurePacProfileForCurrentEnvironmentAsync(token).ConfigureAwait(false);
            token.ThrowIfCancellationRequested();
            // Authentication may prompt; do not deploy if the active selection changed during that prompt.
            environment = await environments.GetActiveEnvironmentAsync(false).ConfigureAwait(false);
            if (environment is null || !DataverseEnvironmentUrl.TryNormalize(environment.Url, out activeUrl)
                || !string.Equals(activeUrl, DataverseEnvironmentUrl.Normalize(request.EnvironmentUrl), StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("The active environment changed before deployment. No push was started.");
            await output.StartAsync().ConfigureAwait(false);
            try
            {
                var commandProgress = new PushProgress(output.CreateProgress(), progress);
                progress?.Report("Building and pushing the PCF control. PAC output will appear here; full details are in the Xrm Tools output pane.");
                try { EnsureSuccess(await pac.RunPcfCommandAsync(directory, arguments, commandProgress, token).ConfigureAwait(false)); }
                catch (PacCommandFailedException ex) when (PacAuthenticationFailure.RequiresReauthentication(ex))
                {
                    output.WriteWarning("PAC authentication expired. Reauthenticate before retrying.");
                    progress?.Report("PAC authentication expired. Complete sign-in in your browser to retry, or cancel here.");
                    environment = await environments.GetActiveEnvironmentAsync(false).ConfigureAwait(false);
                    if (environment is null || !DataverseEnvironmentUrl.TryNormalize(environment.Url, out activeUrl)
                        || !string.Equals(activeUrl, DataverseEnvironmentUrl.Normalize(request.EnvironmentUrl), StringComparison.OrdinalIgnoreCase))
                        throw new InvalidOperationException("The active environment changed before reauthentication. No further push was started.");
                    token.ThrowIfCancellationRequested();
                    await auth.ReauthenticatePacProfileForCurrentEnvironmentAsync(token).ConfigureAwait(false);
                    token.ThrowIfCancellationRequested();
                    environment = await environments.GetActiveEnvironmentAsync(false).ConfigureAwait(false);
                    if (environment is null || !DataverseEnvironmentUrl.TryNormalize(environment.Url, out activeUrl)
                        || !string.Equals(activeUrl, DataverseEnvironmentUrl.Normalize(request.EnvironmentUrl), StringComparison.OrdinalIgnoreCase))
                        throw new InvalidOperationException("The active environment changed before retry. No further push was started.");
                    progress?.Report("Authentication completed. Retrying the PCF build and push...");
                    EnsureSuccess(await pac.RunPcfCommandAsync(directory, arguments, commandProgress, token).ConfigureAwait(false));
                }
                token.ThrowIfCancellationRequested();
                output.WriteCompleted();
            }
            finally { await output.EndAsync().ConfigureAwait(false); }
        }
        finally { state.Exit(); }
    }

    private sealed class PushProgress(IProgress<ProcessOutputLine> output, IProgress<string>? progress) : IProgress<ProcessOutputLine>
    {
        public void Report(ProcessOutputLine line)
        {
            output.Report(line);
            if (!string.IsNullOrWhiteSpace(line.Text)) progress?.Report(line.Text);
        }
    }

    private static void EnsureSuccess(ProcessCommandResult result)
    {
        if (!result.Succeeded) throw new InvalidOperationException($"PAC exited with code {result.ExitCode}.");
    }

    private static CdsProjectInfo Project(string directory, string name) => new() { ProjectDirectory = directory, ProjectName = name };
}

#nullable restore
