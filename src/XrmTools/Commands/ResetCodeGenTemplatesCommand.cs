#nullable enable
namespace XrmTools.Commands;

using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

[Command(PackageGuids.XrmToolsCmdSetIdString, PackageIds.ResetCodeGenTemplatesCmdId)]
internal sealed class ResetCodeGenTemplatesCommand : BaseCommand<ResetCodeGenTemplatesCommand>
{
    [Import]
    internal ITemplateFileGenerator TemplateGenerator { get; set; } = null!;

    protected override async Task InitializeCompletedAsync()
    {
        Command.Supported = false;
        var componentModel = await Package.GetServiceAsync<SComponentModel, IComponentModel>().ConfigureAwait(false);
        componentModel?.DefaultCompositionService.SatisfyImportsOnce(this);
        Command.ParametersDescription = "$";
    }

    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
    {
        var argPath = e?.InValue as string;

        var targetProject = await ResolveTargetProjectAsync(argPath);
        if (targetProject is null)
        {
            await VS.MessageBox.ShowErrorAsync(
                Vsix.Name,
                "No target project found. Select a project in Solution Explorer or pass a valid .csproj path."
            );
            return;
        }

        var confirmed = await VS.MessageBox.ShowConfirmAsync(
            "Reset Code Generation Templates", 
            "This will overwrite existing code generation templates in " + 
            targetProject.Name +
            " project. Do you want to continue?");
        if (!confirmed) return;

        await TemplateGenerator.GenerateTemplatesAsync(targetProject, overwrite: true);
    }

    // Pseudocode:
    // - If argPath is not null/whitespace:
    //   - Sanitize by trimming whitespace, then trimming surrounding double quotes (").
    //   - If it looks like a .csproj path:
    //       - Try to find the project by the provided (sanitized) path.
    //       - If not found, normalize to absolute path and try again.
    // - If no valid project resolved, fallback to the active project:
    //     - If active item is a file/folder, find its parent project.
    //     - Return the project or null.

    private async Task<Project?> ResolveTargetProjectAsync(string? argPath)
    {
        if (!string.IsNullOrWhiteSpace(argPath))
        {
            // Trim whitespace and potential surrounding quotes
            var sanitized = argPath!.Trim().Trim('"');

            //if (LooksLikeProjectPath(sanitized))
            //{
                var byProvidedPath = await FindProjectByFullPathOrNameAsync(sanitized);
                if (byProvidedPath is not null) return byProvidedPath;

                // Try normalized absolute path
                try
                {
                    var normalized = Path.GetFullPath(sanitized);
                    var byNormalizedPath = await FindProjectByFullPathOrNameAsync(normalized);
                    if (byNormalizedPath is not null) return byNormalizedPath;
                }
                catch
                {
                    // Ignore invalid path formats. We'll fallback to active project.
                }
            //}
        }

        // Fallback to active project.
        var activeItem = await VS.Solutions.GetActiveItemAsync();
        if (activeItem?.Type is SolutionItemType.PhysicalFile or SolutionItemType.PhysicalFolder)
        {
            activeItem = activeItem.FindParent(SolutionItemType.Project);
        }
        return activeItem as Project;
    }

    private static bool LooksLikeProjectPath(string candidate)
        => string.Equals(Path.GetExtension(candidate), ".csproj", StringComparison.OrdinalIgnoreCase);

    private static async Task<Project?> FindProjectByFullPathOrNameAsync(string path)
    {
        var projects = await VS.Solutions.GetAllProjectsAsync();
        return projects.FirstOrDefault(
            p => PathUtil.GetCommonPathPrefix(p.FullPath, path) == p.FullPath
            || string.Equals(p.Name, path, StringComparison.OrdinalIgnoreCase));
    }
}
#nullable restore