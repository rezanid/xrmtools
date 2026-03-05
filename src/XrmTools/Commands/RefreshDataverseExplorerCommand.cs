#nullable enable
namespace XrmTools.Commands;

using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using System;
using System.Threading.Tasks;

[Command(PackageGuids.XrmToolsCmdSetIdString, PackageIds.RefreshDataverseExplorerCmdId)]
internal sealed class RefreshDataverseExplorerCommand : BaseCommand<RefreshDataverseExplorerCommand>
{
    protected override void BeforeQueryStatus(EventArgs e)
    {
        Command.Enabled = true;
        Command.Visible = true;
        Command.Text = "Refresh";
    }

    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
    {
        var toolWindow = await Package.FindWindowPaneAsync(typeof(DataverseExplorer.Views.DataverseExplorerWindow), 0, true, Package.DisposalToken);
        if (toolWindow is not DataverseExplorer.Views.DataverseExplorerWindow explorerWindow)
        {
            return;
        }

        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(Package.DisposalToken);
        await explorerWindow.RefreshAsync();
    }
}
#nullable restore
