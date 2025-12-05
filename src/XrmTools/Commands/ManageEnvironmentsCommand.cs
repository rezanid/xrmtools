namespace XrmTools.Commands;

using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.InteropServices;
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

[Command(PackageGuids.XrmToolsCmdSetIdString, PackageIds.ManageEnvironmentCmdId)]
internal class ManageEnvironmentsCommand : BaseCommand<ManageEnvironmentsCommand>
{
    private const string ManageItem = "Manage environments...";
    private const string DefaultItem = "Select environment...";

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
        Command.Text = DefaultItem;
    }

    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
    {
        if (e is not OleMenuCmdEventArgs args)
            return;

        if (args.OutValue != IntPtr.Zero)
        {
            var activeEnvironment = await EnvironmentProvider.GetActiveEnvironmentAsync(true);
            var environmentName = activeEnvironment?.Name ?? DefaultItem;
            Marshal.GetNativeVariantForObject(environmentName, args.OutValue);
        }
        else if (args.InValue is string selected)
        {
            if (selected == ManageItem)
            {
                await EnvironmentEditor.EditEnvironmentsAsync();
                return;
            }

            await SetActiveEnvironmentAsync(selected);
        }
    }

    private async Task SetActiveEnvironmentAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name == ManageItem)
        {
            return;
        }
        var environments = await EnvironmentProvider.GetAvailableEnvironmentsAsync();
        var environment = environments.FirstOrDefault(e => e.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (environment == null)
        {
            await VS.MessageBox.ShowErrorAsync("Manage Environments", $"Environment '{name}' not found.");
            return;
        }
        await EnvironmentProvider.SetActiveEnvironmentAsync(environment, true);
    }
}

[Command(PackageGuids.XrmToolsCmdSetIdString, PackageIds.ManageEnvironmenGetListCmdId)]
internal class ManageEnvironmentsGetListCommand : BaseCommand<ManageEnvironmentsGetListCommand>
{
    private const string ManageItem = "Manage environments...";

    [Import]
    public IEnvironmentProvider EnvironmentProvider { get; set; }

    protected override async Task InitializeCompletedAsync()
    {
        var componentModel = await Package.GetServiceAsync<SComponentModel, IComponentModel>();
        componentModel?.DefaultCompositionService.SatisfyImportsOnce(this);
    }

    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
    {
        if (e is not OleMenuCmdEventArgs args)
            return;

        if (args.InValue is not null)
        {
            throw new ArgumentException("In parameter may not be specified");
        }
        else if (args.OutValue != IntPtr.Zero)
        {
            var environments = await EnvironmentProvider.GetAvailableEnvironmentsAsync();
            string[] envNames = [.. environments.Select(e => e.Name), ManageItem];
            Marshal.GetNativeVariantForObject(envNames, args.OutValue);
        }
        else
        {
            throw new ArgumentException("Out parameter must be specified");
        }
    }
}