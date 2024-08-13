using EnvDTE;
using EnvDTE80;
using Microsoft;
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using Task = System.Threading.Tasks.Task;

namespace XrmGen.Commands;

/// <summary>
/// Command handler to set the custom tool of the selected item to the Xrm Plugin Code Generator.
/// </summary>
internal sealed class SetXrmPluginGeneratorCommand
{
    /// <summary>
    /// VS Package that provides this command, not null.
    /// </summary>
    private readonly AsyncPackage package;
    private readonly DTE2 dte;

    /// <summary>
    /// Initializes a new instance of the <see cref="SetXrmPluginGeneratorCommand"/> class.
    /// Adds our command handlers for menu (commands must exist in the command table file)
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    /// <param name="commandService">Command service to add command to, not null.</param>
    private SetXrmPluginGeneratorCommand(AsyncPackage package, IMenuCommandService commandService, DTE2 dte)
    {
        this.package = package ?? throw new ArgumentNullException(nameof(package));
        this.dte = dte ?? throw new ArgumentNullException(nameof(dte));
        commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

        var cmdId = new CommandID(PackageGuids.guidXrmCodeGenPackageCmdSet, PackageIds.SetXrmPluginGeneratorCommandId);
        var cmd = new OleMenuCommand(OnExecute, cmdId, queryStatusSupported: false);
        commandService.AddCommand(cmd);
    }

    /// <summary>
    /// Gets the instance of the command.
    /// </summary>
    public static SetXrmPluginGeneratorCommand Instance { get; private set; }

    /// <summary>
    /// Gets the service provider from the owner package.
    /// </summary>
    private IAsyncServiceProvider ServiceProvider { get => package; }

    /// <summary>
    /// Initializes the singleton instance of the command.
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    public static async Task InitializeAsync(AsyncPackage package)
    {
        // Switch to the main thread - the call to AddCommand in SetXrmPluginGeneratorCommand's constructor requires
        // the UI thread.
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

        var commandService = await package.GetServiceAsync<IMenuCommandService, IMenuCommandService>();

        // Unless there is a better / easier  way to do it instead of using DTE, we will. For now, it's OK.
        // Note! DTE2 is the new DTE, but the service is registered in DTE, so we get it and cast it like following.
        var dte = await package.GetServiceAsync<DTE, DTE2>();
        Assumes.Present(dte);

        Instance = new SetXrmPluginGeneratorCommand(package, commandService, dte);
    }

    private void OnExecute(object sender, EventArgs e)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        // Yes, it's a 1-based array!
        var item = dte.SelectedItems.Item(1).ProjectItem;
        if (item != null)
        {
            item.Properties.Item("CustomTool").Value = PluginCodeGenerator.Name;
        }
    }
}
