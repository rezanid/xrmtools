#nullable enable
namespace XrmTools.Commands;

using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using System;
using System.IO;
using XrmTools.Helpers;
using Task = System.Threading.Tasks.Task;
using System.Threading.Tasks;

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

        if (i.FindParent(SolutionItemType.Project) is not Project project) return;

        if (i is not PhysicalFile file) return;

        if (await file.IsXrmEntityFileAsync())
        {
            await DisableCustomToolAsync(file, project);
        }
        else
        {
            await EnableCustomToolAsync(file, project);
        }
    }

    private async Task EnableCustomToolAsync(PhysicalFile file, Project project)
    {
        if (project.IsSdkStyle())
        {
            var genFilePath = Path.Combine(
                Path.GetDirectoryName(file.FullPath),
                Path.GetFileNameWithoutExtension(file.FullPath)) + ".g.cs";
            if (!File.Exists(genFilePath)) await FileHelper.AddItemAsync(genFilePath, "", file);
            var genFile = await PhysicalFile.FromFileAsync(genFilePath);
            await genFile.TrySetAttributeAsync(PhysicalFileAttribute.AutoGen, true);
            await genFile.TrySetAttributeAsync(PhysicalFileAttribute.DesignTime, true);
            await genFile.TrySetAttributeAsync(PhysicalFileAttribute.DependentUpon, Path.GetFileName(file.FullPath));
            await file.TrySetAttributeAsync(PhysicalFileAttribute.LastGenOutput, Path.GetFileName(genFilePath));
            await file.TrySetAttributeAsync(PhysicalFileAttribute.Generator, EntityCodeGenerator.Name);
        }
        else
        {
            await file.TrySetAttributeAsync(PhysicalFileAttribute.CustomTool, EntityCodeGenerator.Name);
        }
    }

    private async Task DisableCustomToolAsync(PhysicalFile file, Project project)
    {
        if (project.IsSdkStyle())
        {
            var genFilePath = Path.Combine(
                Path.GetDirectoryName(file.FullPath),
                Path.GetFileNameWithoutExtension(file.FullPath)) + ".g.cs";
            if (File.Exists(genFilePath))
            {
                if ((await PhysicalFile.FromFileAsync(genFilePath)) is PhysicalFile genFile)
                    await genFile.TryRemoveAsync();
            }
            await file.TrySetAttributeAsync(PhysicalFileAttribute.LastGenOutput, null);
            await file.TrySetAttributeAsync(PhysicalFileAttribute.Generator, null);
        }
        else
        {
            await file.TrySetAttributeAsync(PhysicalFileAttribute.CustomTool, null);
        }
    }

    protected override Task InitializeCompletedAsync()
    {
        Command.Supported = false;
        return Task.CompletedTask;
    }

    protected override void BeforeQueryStatus(EventArgs e)
    {
        ThreadHelper.JoinableTaskFactory.Run(async () =>
        {
            var item = await VS.Solutions.GetActiveItemAsync();
            Command.Supported = true;
            Command.Visible = await IsVisibleAsync(item).ConfigureAwait(false);
            Command.Checked = await IsEntityFileAsync(item).ConfigureAwait(false);
            Command.Text = Command.Checked ? "Disable Entity Code Generation" : "Enable Entity Code Generation";
        });

        static async Task<bool> IsVisibleAsync(SolutionItem? item)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            if (!KnownUIContexts.SolutionHasSingleProjectContext.IsActive && !KnownUIContexts.SolutionHasMultipleProjectsContext.IsActive)
                return false;

            var proj = await VS.Solutions.GetActiveProjectAsync();
            if (!proj?.IsCapabilityMatch("CSharp") ?? false)
                return false;

            if (item is null)
                return false;

            if (item.Type != SolutionItemType.PhysicalFile || item is not PhysicalFile file)
                return false;

            if (item.Name.EndsWith(".Designer.cs", StringComparison.OrdinalIgnoreCase))
                return false;

            if (!item.Name.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                return false;

            return true;
        }

        static async Task<bool> IsEntityFileAsync(SolutionItem item)
        {
            if (item is null || item.Type != SolutionItemType.PhysicalFile) return false;
            var file = item as PhysicalFile;
            return await file.IsXrmEntityFileAsync();
        }
    }
}
#nullable restore