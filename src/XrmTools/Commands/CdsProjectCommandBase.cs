#nullable enable
namespace XrmTools.Commands;

using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using XrmTools.DataverseSolutions;
using XrmTools.Logging.Compatibility;

internal abstract class CdsProjectCommandBase<TCommand> : BaseCommand<TCommand>
    where TCommand : class, new()
{
    [Import]
    internal IDataverseSolutionCommandService CommandService { get; set; } = null!;

    [Import]
    internal ICdsProjectResolver CdsProjectResolver { get; set; } = null!;

    [Import]
    internal ILogger<TCommand> Logger { get; set; } = null!;

    protected abstract DataverseSolutionCommandKind CommandKind { get; }

    protected override async Task InitializeCompletedAsync()
    {
        var componentModel = await Package.GetServiceAsync<SComponentModel, IComponentModel>().ConfigureAwait(false);
        componentModel?.DefaultCompositionService.SatisfyImportsOnce(this);
    }

    protected override void BeforeQueryStatus(EventArgs e)
        => ThreadHelper.JoinableTaskFactory.Run(SetVisibilityAsync);

    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
    {
        try
        {
            await CommandService.ExecuteAsync(CommandKind, Package.DisposalToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Dataverse solution command failed.");
            await VS.MessageBox.ShowErrorAsync(Vsix.Name, ex.Message);
        }
        finally
        {
            await SetVisibilityAsync();
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            if (await Package.GetServiceAsync(typeof(SVsUIShell)) is IVsUIShell shell)
                shell.UpdateCommandUI(0);
        }
    }

    private async Task SetVisibilityAsync()
    {
        var isVisible = await CdsProjectResolver.IsSelectedItemCdsProjectAsync().ConfigureAwait(false);
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        Command.Visible = isVisible;
        Command.Enabled = isVisible && !CommandService.IsBusy;
    }
}
#nullable restore
