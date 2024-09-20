namespace XrmTools.Commands;

using Community.VisualStudio.Toolkit;
using Community.VisualStudio.Toolkit.DependencyInjection;
using Community.VisualStudio.Toolkit.DependencyInjection.Core;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

/// <summary>
/// Command handler to set the custom tool of the selected item to the Xrm Entity Code Generator.
/// </summary>
[Command(PackageGuids.guidXrmEntityCodeGeneratorString, PackageIds.EntityGeneratorCommandId)]
internal sealed class ApplyEntityGeneratorCommand(DIToolkitPackage package) : BaseDICommand(package)
{
    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
    {
        var i = await VS.Solutions.GetActiveItemAsync();
        if (i is null || i.Type != SolutionItemType.PhysicalFile) return;
        await (i as PhysicalFile).TrySetAttributeAsync(PhysicalFileAttribute.CustomTool, EntityCodeGenerator.Name);
    }
}
