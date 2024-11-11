namespace XrmTools.Commands;

using Community.VisualStudio.Toolkit;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

/// <summary>
/// Command handler to set the custom tool of the selected item to the Xrm Entity Code Generator.
/// </summary>
[Command(PackageGuids.XrmToolsCmdSetIdString, PackageIds.SetCustomToolEntityGeneratorCmdId)]
internal sealed class SetCustomToolEntityGeneratorCommand : BaseCommand<SetCustomToolEntityGeneratorCommand>
{
    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
    {
        var i = await VS.Solutions.GetActiveItemAsync();
        if (i is null || i.Type != SolutionItemType.PhysicalFile) return;
        await (i as PhysicalFile).TrySetAttributeAsync(PhysicalFileAttribute.CustomTool, EntityCodeGenerator.Name);
    }

    protected override Task InitializeCompletedAsync()
    {
        Command.Supported = false;
        return Task.CompletedTask;
    }
}