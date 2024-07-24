using EnvDTE;
using EnvDTE80;
using Microsoft;
using Microsoft.VisualStudio.Shell;
using System.ComponentModel.Design;
using Task = System.Threading.Tasks.Task;

namespace XrmGen;
/// <summary>
/// Command handler
/// </summary>
internal sealed class ApplyEntityGeneratorCommand
{
    /// <summary>
    /// Initializes the singleton instance of the command.
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    public static async Task InitializeAsync(AsyncPackage package)
    {
        // Switch to the main thread - the call to AddCommand in EntityGeneratorCommand's constructor requires
        // the UI thread.
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

        var commandService = await package.GetServiceAsync<IMenuCommandService,IMenuCommandService>();
        Assumes.Present(commandService);
		
		// Unless there is a better / easier  way to do it instead of using DTE, we will. For now, it's OK.
		// Note! DTE2 is the new DTE, but the service is registered in DTE, so we get it and cast it like following.
		var dte = await package.GetServiceAsync<DTE, DTE2>();
        Assumes.Present(dte);

        var cmdId = new CommandID(PackageGuids.guidXrmCodeGenPackageCmdSet, PackageIds.EntityGeneratorCommandId);
        //var cmd = new MenuCommand(Execute, cmdId);
        //var cmd = new MenuCommand((s, e) => Execute(dte), cmdId);
		// queryStatusSupported: false means the visibility rule set before loading the command still applies.
		var cmd = new OleMenuCommand((s,e) => OnExecute(dte), cmdId, queryStatusSupported: false);
        commandService.AddCommand(cmd);
    }

    /// <summary>
    /// This function is the callback used to execute the command when the menu item is clicked.
    /// See the constructor to see how the menu item is associated with this function using
    /// OleMenuCommandService service and MenuCommand class.
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="e">Event args.</param>
    private static void OnExecute(DTE2 dte)// object sender, EventArgs e)
    {
		ThreadHelper.ThrowIfNotOnUIThread();
		
		// Yes, it's a 1-based array!
        var item = dte.SelectedItems.Item(1).ProjectItem;
        if (item != null)
        {
            item.Properties.Item("CustomTool").Value = EntityGenerator.Name;
        }
    }
}
