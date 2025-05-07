#nullable enable
namespace XrmTools.Commands;

using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;
using Community.VisualStudio.Toolkit;
using XrmTools.Helpers;
using System.IO;
using System;
using System.Threading.Tasks;
using System.Diagnostics;

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

        if (i is not PhysicalFile file) return;

        if (await file.IsXrmPluginFileAsync())
        {
            await DisableCustomToolAsync(file, project);
        }
        else
        {
            await EnableCustomToolAsync(file, project);
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
            Command.Checked = await IsPluginFileAsync(item).ConfigureAwait(false);
            Command.Text = Command.Checked ? "Disable Xrm Plugin Code Generation" : "Enable Xrm Plugin Code Generation";
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

        static async Task<bool> IsPluginFileAsync(SolutionItem? item)
        {
            if (item is null)
                return false;
            if (item.Type != SolutionItemType.PhysicalFile || item is not PhysicalFile file)
                return false;
            var isXrmPlugin = await file.IsXrmPluginFileAsync().ConfigureAwait(false);
            return isXrmPlugin;
        }
    }

    private async Task EnableCustomToolAsync(PhysicalFile file, Project project)
    {
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
            await file.TrySetAttributeAsync(PhysicalFileAttribute.CustomTool, PluginCodeGenerator.Name);
        }
        await file.TrySetAttributeAsync("IsXrmPlugin", true);
    }

    private async Task DisableCustomToolAsync(PhysicalFile file, Project project)
    {
        if (project.IsSdkStyle())
        {
            var genFilePath = Path.Combine(
                Path.GetDirectoryName(file.FullPath),
                Path.GetFileNameWithoutExtension(file.FullPath)) + ".Generated.cs";
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
        await file.TrySetAttributeAsync("IsXrmPlugin", null);
    }
}
#nullable restore