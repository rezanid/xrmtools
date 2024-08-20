#nullable enable
using EnvDTE;
using EnvDTE80;
using Microsoft;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Design;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using XrmGen._Core;
using XrmGen.Extensions;
using XrmGen.UI;
using XrmGen.Xrm;
using XrmGen.Xrm.Model;
using Task = System.Threading.Tasks.Task;

namespace XrmGen.Commands;

/// <summary>
/// Command handler to generate the registration file for the selected assembly.
/// </summary>
internal sealed class GenerateRegistrationFileCommand
{
    private readonly AsyncPackage package;
    private readonly DTE2 dte;
    private IXrmSchemaProviderFactory? _schemaProviderFactory;

    [Import]
    IXrmSchemaProviderFactory? SchemaProviderFactory
    {
        // MEF does not work here, so this is our only option.
        get => _schemaProviderFactory ??= ServiceProvider.GetServiceAsync<IXrmSchemaProviderFactory, IXrmSchemaProviderFactory>().Result;
        set => _schemaProviderFactory = value;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="GenerateRegistrationFileCommand"/> class.
    /// Adds our command handlers for menu (commands must exist in the command table file)
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    /// <param name="commandService">Command service to add command to, not null.</param>
    private GenerateRegistrationFileCommand(AsyncPackage package, IMenuCommandService commandService, DTE2 dte)
    {
        this.package = package ?? throw new ArgumentNullException(nameof(package));
        this.dte = dte ?? throw new ArgumentNullException(nameof(dte));
        commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

        var cmdId = new CommandID(PackageGuids.guidXrmCodeGenPackageCmdSet, PackageIds.GeneratePluginConfigFileCommandId);
        var menuItem = new OleMenuCommand(OnExecute, cmdId, queryStatusSupported: false);
        commandService.AddCommand(menuItem);
    }

    /// <summary>
    /// Gets the instance of the command.
    /// </summary>
    public static GenerateRegistrationFileCommand Instance { get; private set; }

    /// <summary>
    /// Gets the service provider from the owner package.
    /// </summary>
    private IAsyncServiceProvider ServiceProvider { get => this.package; }

    /// <summary>
    /// Initializes the singleton instance of the command.
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    public static async Task InitializeAsync(AsyncPackage package)
    {
        // Switch to the main thread - the call to AddCommand in ToolWindow1Command's constructor requires
        // the UI thread.
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

        var commandService = await package.GetServiceAsync<IMenuCommandService, IMenuCommandService>();

        // Unless there is a better / easier  way to do it instead of using DTE, we will. For now, it's OK.
        // Note! DTE2 is the new DTE, but the service is registered in DTE, so we get it and cast it like following.
        var dte = await package.GetServiceAsync<DTE, DTE2>();
        Assumes.Present(dte);

        Instance = new GenerateRegistrationFileCommand(package, commandService, dte);
    }

    private void OnExecute(object sender, EventArgs e)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        // Yes, it's a 1-based array!
        var item = dte.SelectedItems.Item(1).ProjectItem;
        if (item == null) { return; }
        
        //item.Properties.Item("CustomTool").Value = PluginCodeGenerator.Name;

        if (ChooseAssembly() is PluginAssemblyConfig assembly)
        {
            //var assembly = dialog.SelectedAssembly;
            //var generator = new RegistrationFileGenerator(assembly);
            //generator.Generate();
            Logger.Log("Assembly selected: " + assembly.Name);
        }
        else
        {
            Logger.Log("No assembly selected.");
        }
    }

    private string? GetProjectProperty(string propertyName)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        IVsSolution solution = (IVsSolution)ServiceProvider.GetServiceAsync(typeof(SVsSolution)).Result;
        Assumes.Present(solution);
        solution.GetProjectOfUniqueName(dte.SelectedItems.Item(1).ProjectItem.ContainingProject.UniqueName, out var hierarchy);

        return hierarchy.GetProjectProperty(propertyName);
    }


    private PluginAssemblyConfig? ChooseAssembly()
    {
        var url = GetProjectProperty("EnvironmentUrl");
        if (string.IsNullOrWhiteSpace(url)) { return null; }
        var schemaProvider = SchemaProviderFactory?.Get(url!);
        if (schemaProvider == null) 
        {
            Logger.Log(url + " used in your EnvironmentUrl build property is not a valid environment URL.");
            return null; 
        }
        var dialog = new AssemblySelectionDialog(schemaProvider);
        if (dialog.ShowDialog() == true)
        {
            return ((AssemblySelectionViewModel)dialog.DataContext).SelectedAssembly;
        }
        return null;
    }

}
#nullable restore