#nullable enable
namespace XrmTools.Commands;

using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading.Tasks;
using XrmTools.Logging;
using XrmTools.Logging.Compatibility;
using XrmTools.Pcf.Setup;
using XrmTools.UI;

/// <summary>Opens the shared PCF setup experience for the selected project; also reached from the build notification.</summary>
[Command(PackageGuids.XrmToolsCmdSetIdString, PackageIds.SetUpPcfBuildCmdId)]
internal sealed class SetUpPcfBuildCommand : BaseCommand<SetUpPcfBuildCommand>
{
    [Import] internal IPcfSetupService Setup { get; set; } = null!;
    [Import] internal PcfBuildCoordinator Coordinator { get; set; } = null!;
    [Import] internal IOutputLoggerService Output { get; set; } = null!;
    [Import] internal ILogger<SetUpPcfBuildCommand> Logger { get; set; } = null!;

    protected override async Task InitializeCompletedAsync()
    {
        var model = await Package.GetServiceAsync<SComponentModel, IComponentModel>().ConfigureAwait(false);
        model?.DefaultCompositionService.SatisfyImportsOnce(this);
        Coordinator.ShowSetup = ShowAsync;
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(Package.DisposalToken);
        Coordinator.Advise(Package);
    }

    protected override void BeforeQueryStatus(EventArgs e) => ThreadHelper.JoinableTaskFactory.Run(async () =>
    {
        var path = await SelectedProjectAsync().ConfigureAwait(false);
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        Command.Visible = Command.Enabled = path is not null;
    });

    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
    {
        try { await ShowAsync(await SelectedProjectAsync().ConfigureAwait(false) ?? throw new InvalidOperationException("Select a PCF project.")).ConfigureAwait(false); }
        catch (OperationCanceledException) { }
        catch (Exception ex) { Logger.LogError(ex, "Could not open PCF build setup."); await VS.MessageBox.ShowErrorAsync(Vsix.Name, ex.Message); }
    }

    internal async Task ShowAsync(string projectFile)
    {
        await Coordinator.CloseNoticeAsync(projectFile).ConfigureAwait(false);
        Output.OutputString($"{Environment.NewLine}PCF build setup: {projectFile}{Environment.NewLine}");
        await PcfSetupDialog.ShowAsync(projectFile, Setup, line => Output.OutputString(line + Environment.NewLine), Package.DisposalToken).ConfigureAwait(false);
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
