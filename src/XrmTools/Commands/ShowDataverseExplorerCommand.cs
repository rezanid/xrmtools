namespace XrmTools.Commands;

using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using XrmTools.Environments;
using XrmTools.UI;

//   A DropDownCombo combobox requires two commands:
//     One command (ManageEnvironmentsCommand) is used to ask for the current value for the display area of the combo box 
//     and to set the new value when the user makes a choice in the combo box.
//
//     The second command (ManageEnvironmentsGetListCommand) is used to retrieve the list of choices for the combo box drop
//     down area.
// 
// Normally IOleCommandTarget::QueryStatus is used to determine the state of a command, e.g.
// enable vs. disable, shown vs. hidden, etc. The QueryStatus method does not have enough
// flexibility for combos which need to be able to indicate a currently selected (displayed)
// item as well as provide a list of items for their dropdown area. In order to communicate 
// this information actually IOleCommandTarget::Exec is used with a non-NULL varOut parameter. 
// You can think of these Exec calls as extended QueryStatus calls. There are two pieces of 
// information needed for a combo, thus it takes two commands to retrieve this information. 
// The main command id for the command is used to retrieve the current value and the second 
// command is used to retrieve the full list of choices to be displayed as an array of strings.

[Command(PackageGuids.XrmToolsCmdSetIdString, PackageIds.ShowDataverseExplorerCmdId)]
internal class ShowDataverseExplorerCommand : BaseCommand<ShowDataverseExplorerCommand>
{
    [Import]
    public IEnvironmentEditor EnvironmentEditor { get; set; }

    [Import]
    public IEnvironmentProvider EnvironmentProvider { get; set; }

    protected override async Task InitializeCompletedAsync()
    {
        var componentModel = await Package.GetServiceAsync<SComponentModel, IComponentModel>();
        componentModel?.DefaultCompositionService.SatisfyImportsOnce(this);
    }

    protected override void BeforeQueryStatus(EventArgs e)
    {
        Command.Enabled = true;
        Command.Text = "Show Dataverse Explorer";
    }

    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
    {
        if (e is not OleMenuCmdEventArgs args)
            return;

        var toolWindow = await Package.FindWindowPaneAsync(typeof(DataverseExplorer.Views.DataverseExplorerWindow), 0, true, Package.DisposalToken);

        if (toolWindow is not DataverseExplorer.Views.DataverseExplorerWindow explorerWindow)
            return;

        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(Package.DisposalToken);

        IVsWindowFrame windowFrame = (IVsWindowFrame)explorerWindow.Frame;
        Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
    }
}