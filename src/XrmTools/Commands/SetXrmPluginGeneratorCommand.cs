namespace XrmTools.Commands;

using Community.VisualStudio.Toolkit.DependencyInjection.Core;
using Community.VisualStudio.Toolkit.DependencyInjection;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;
using Community.VisualStudio.Toolkit;

/// <summary>
/// Command handler to set the custom tool of the selected item to the Xrm Plugin Code Generator.
/// </summary>
[Command(PackageGuids.XrmToolsCmdSetIdString, PackageIds.SetCustomToolPluginDefitionCmdId)]
internal sealed class SetXrmPluginGeneratorCommand : BaseDICommand
{
    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
    {
        var i = await VS.Solutions.GetActiveItemAsync();
        if (i is null || i.Type != SolutionItemType.PhysicalFile) return;
        await (i as PhysicalFile).TrySetAttributeAsync(PhysicalFileAttribute.CustomTool, PluginCodeGenerator.Name);
    }

    public SetXrmPluginGeneratorCommand(DIToolkitPackage package): base(package)
        => Command.Supported = false;
}
