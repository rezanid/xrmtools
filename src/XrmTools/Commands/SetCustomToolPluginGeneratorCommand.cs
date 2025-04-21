namespace XrmTools.Commands;

using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;
using Community.VisualStudio.Toolkit;
using XrmTools.Helpers;
using System.IO;

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

        if (i.FindParent(SolutionItemType.Project) is not Project project) return;

        var file = i as PhysicalFile;
        if (project.IsSdkStyle())
        {
            var genFilePath = Path.Combine(
                Path.GetDirectoryName(file.FullPath), 
                Path.GetFileNameWithoutExtension(file.FullPath)) + ".Generated.cs";
            if (!File.Exists(genFilePath)) await FileHelper.AddItemAsync(genFilePath, "", file);
            var genFile = await PhysicalFile.FromFileAsync(genFilePath);
            await genFile.TrySetAttributeAsync(PhysicalFileAttribute.AutoGen, true);
            await genFile.TrySetAttributeAsync(PhysicalFileAttribute.DesignTime, true);
            await genFile.TrySetAttributeAsync(PhysicalFileAttribute.DependentUpon, Path.GetFileName(file.FullPath));
            await file.TrySetAttributeAsync(PhysicalFileAttribute.LastGenOutput, Path.GetFileName(genFilePath));
            await file.TrySetAttributeAsync(PhysicalFileAttribute.Generator, PluginCodeGenerator.Name);
        }
        else
        {
            await file.TrySetAttributeAsync(PhysicalFileAttribute.Generator, PluginCodeGenerator.Name);
        }
    }

    protected override Task InitializeCompletedAsync() 
    {
        Command.Supported = false;
        return Task.CompletedTask;
    }
}
