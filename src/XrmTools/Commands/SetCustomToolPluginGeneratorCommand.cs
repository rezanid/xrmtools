namespace XrmTools.Commands;

using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;
using Community.VisualStudio.Toolkit;

/// <summary>
/// Command handler to set the custom tool of the selected item to the Xrm Plugin Code Generator.
/// </summary>
[Command(PackageGuids.XrmToolsCmdSetIdString, PackageIds.SetCustomToolPluginGeneratorCmdId)]
internal sealed class SetCustomToolPluginGeneratorCommand : BaseCommand<SetCustomToolPluginGeneratorCommand>
{
    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
    {
        var i = await VS.Solutions.GetActiveItemAsync();
        if (i is null || i.Type != SolutionItemType.PhysicalFile) return;
        await (i as PhysicalFile).TrySetAttributeAsync(PhysicalFileAttribute.CustomTool, PluginCodeGenerator.Name);
    }

    protected override Task InitializeCompletedAsync() 
    {
        Command.Supported = false;
        return Task.CompletedTask;
    }
}
