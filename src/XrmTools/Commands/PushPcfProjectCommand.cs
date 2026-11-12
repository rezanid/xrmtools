#nullable enable
namespace XrmTools.Commands;

using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading.Tasks;
using XrmTools.DataverseSolutions;
using XrmTools.Environments;
using XrmTools.Logging.Compatibility;
using XrmTools.UI;
using XrmTools.Xrm.Repositories;

[Command(PackageGuids.XrmToolsCmdSetIdString, PackageIds.PushPcfProjectCmdId)]
internal sealed class PushPcfProjectCommand : BaseCommand<PushPcfProjectCommand>
{
    [Import] internal IPcfProjectService Service { get; set; } = null!;
    [Import] internal IEnvironmentProvider EnvironmentProvider { get; set; } = null!;
    [Import] internal IRepositoryFactory RepositoryFactory { get; set; } = null!;
    [Import] internal IPcfPushDestinationResolver DestinationResolver { get; set; } = null!;
    [Import] internal ILogger<PushPcfProjectCommand> Logger { get; set; } = null!;
    protected override async Task InitializeCompletedAsync()
    {
        var model = await Package.GetServiceAsync<SComponentModel, IComponentModel>().ConfigureAwait(false);
        model?.DefaultCompositionService.SatisfyImportsOnce(this);
    }
    protected override void BeforeQueryStatus(EventArgs e) => ThreadHelper.JoinableTaskFactory.Run(UpdateStatusAsync);
    private async Task UpdateStatusAsync()
    {
        var path = await SelectedProjectAsync().ConfigureAwait(false);
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        Command.Visible = path is not null;
        Command.Enabled = path is not null && !Service.IsBusy;
    }
    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
    {
        try
        {
            var path = await SelectedProjectAsync().ConfigureAwait(false) ?? throw new InvalidOperationException("Select a PCF project.");
            var environment = await EnvironmentProvider.GetActiveEnvironmentAsync(true).ConfigureAwait(false);
            if (environment is null || !environment.IsValid || string.IsNullOrWhiteSpace(environment.Url)) throw new InvalidOperationException("Select an active Dataverse environment before pushing.");
            var targetUrl = DataverseEnvironmentUrl.Normalize(environment.Url);
            await PcfPushDialog.ShowAsync(path, targetUrl, async (request, token, progress) =>
            {
                try { await Service.PushAsync(request, token, progress).ConfigureAwait(false); }
                catch (OperationCanceledException) { throw; }
                catch (Exception ex) { Logger.LogError(ex, "PCF push failed."); throw; }
            }, async token =>
            {
                async Task CheckEnvironmentAsync()
                {
                    token.ThrowIfCancellationRequested();
                    var active = await EnvironmentProvider.GetActiveEnvironmentAsync(false).ConfigureAwait(false);
                    if (active is null || !active.IsValid || !DataverseEnvironmentUrl.TryNormalize(active.Url, out var url)
                        || !string.Equals(url, targetUrl, StringComparison.OrdinalIgnoreCase))
                        throw new InvalidOperationException("The active environment changed. Reopen Push to Environment to load its solutions.");
                }
                try
                {
                    await CheckEnvironmentAsync().ConfigureAwait(false);
                    var repository = RepositoryFactory.CreateRepository<ISolutionRepository>();
                    var solutions = await repository.GetUnmanagedAsync(token).ConfigureAwait(false);
                    await CheckEnvironmentAsync().ConfigureAwait(false);
                    return solutions;
                }
                catch (OperationCanceledException) { throw; }
                catch (Exception ex) { Logger.LogError(ex, "Could not load unmanaged solutions for PCF push."); throw; }
            }, async token =>
            {
                try { return await DestinationResolver.FindAsync(path, token).ConfigureAwait(false); }
                catch (OperationCanceledException) { throw; }
                catch (Exception ex) { Logger.LogError(ex, "Could not infer the PCF push destination from project references."); throw; }
            }, Package.DisposalToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { Logger.LogError(ex, "Could not push PCF project."); await VS.MessageBox.ShowErrorAsync(Vsix.Name, ex.Message); }
        finally
        {
            await UpdateStatusAsync();
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            if (await Package.GetServiceAsync(typeof(SVsUIShell)) is IVsUIShell shell) shell.UpdateCommandUI(0);
        }
    }
    private static async Task<string?> SelectedProjectAsync()
    {
        var item = await VS.Solutions.GetActiveItemAsync();
        if (item is null) return null;
        if (IsPcf(item.FullPath)) return item.FullPath;
        if (item.Type is SolutionItemType.PhysicalFile or SolutionItemType.PhysicalFolder)
        {
            var project = item.FindParent(SolutionItemType.Project) as Community.VisualStudio.Toolkit.Project;
            if (IsPcf(project?.FullPath)) return project!.FullPath;
        }
        return null;
    }
    private static bool IsPcf(string? path) => !string.IsNullOrWhiteSpace(path) && string.Equals(Path.GetExtension(path), ".pcfproj", StringComparison.OrdinalIgnoreCase);
}
#nullable restore
