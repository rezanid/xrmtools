#nullable enable
namespace XrmTools.DataverseSolutions;

using Community.VisualStudio.Toolkit;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal interface ICdsProjectResolver
{
    Task<bool> IsSelectedItemCdsProjectAsync();

    Task<CdsProjectInfo?> TryResolveSelectedProjectAsync(CancellationToken cancellationToken);
}

internal sealed class SelectedCdsProject
{
    public string ProjectFilePath { get; set; } = string.Empty;

    public string ProjectName { get; set; } = string.Empty;
}

[Export(typeof(ICdsProjectResolver))]
[method: ImportingConstructor]
internal sealed class CdsProjectResolver(IMsBuildProjectPropertyEvaluator msBuildProjectPropertyEvaluator) : ICdsProjectResolver
{
    private readonly IMsBuildProjectPropertyEvaluator _msBuildProjectPropertyEvaluator = msBuildProjectPropertyEvaluator;

    public async Task<bool> IsSelectedItemCdsProjectAsync()
    {
        var selection = await ResolveSelectionAsync().ConfigureAwait(false);
        return selection is not null;
    }

    public async Task<CdsProjectInfo?> TryResolveSelectedProjectAsync(CancellationToken cancellationToken)
    {
        var selection = await ResolveSelectionAsync().ConfigureAwait(false);
        if (selection is null)
        {
            return null;
        }

        var configurationName = await GetActiveConfigurationNameAsync(cancellationToken).ConfigureAwait(false);
        var properties = await _msBuildProjectPropertyEvaluator.EvaluateAsync(
            selection.ProjectFilePath,
            configurationName,
            ["SolutionPackageMapFilePath", "SolutionRootPath", "SolutionPackageZipFilePath"],
            cancellationToken).ConfigureAwait(false);
        var projectDirectory = Path.GetDirectoryName(selection.ProjectFilePath)
            ?? throw new InvalidOperationException($"Could not determine the directory of '{selection.ProjectFilePath}'.");

        return new CdsProjectInfo
        {
            ProjectFilePath = selection.ProjectFilePath,
            ProjectDirectory = projectDirectory,
            ProjectName = selection.ProjectName,
            ConfigurationName = configurationName,
            SolutionPackageMapFilePath = ResolvePath(projectDirectory, properties.TryGetValue("SolutionPackageMapFilePath", out var mapPath) ? mapPath : null),
            SolutionRootPath = ResolvePath(projectDirectory, properties.TryGetValue("SolutionRootPath", out var solutionRootPath) ? solutionRootPath : null) ?? Path.Combine(projectDirectory, "src"),
            SolutionPackageZipFilePath = ResolvePath(projectDirectory, properties.TryGetValue("SolutionPackageZipFilePath", out var zipFilePath) ? zipFilePath : null) ?? string.Empty
        };
    }

    private static async Task<SelectedCdsProject?> ResolveSelectionAsync()
    {
        var activeItem = await VS.Solutions.GetActiveItemAsync();
        if (activeItem is null)
        {
            return null;
        }

        if (activeItem.Type == SolutionItemType.Project && activeItem is Community.VisualStudio.Toolkit.Project project && IsCdsProjectPath(project.FullPath))
        {
            return new SelectedCdsProject
            {
                ProjectFilePath = project.FullPath!,
                ProjectName = project.Name
            };
        }

        if (activeItem.FullPath is not null && IsCdsProjectPath(activeItem.FullPath))
        {
            return new SelectedCdsProject
            {
                ProjectFilePath = activeItem.FullPath,
                ProjectName = Path.GetFileNameWithoutExtension(activeItem.FullPath)
            };
        }

        if (activeItem.Type is SolutionItemType.PhysicalFile or SolutionItemType.PhysicalFolder)
        {
            var parentProject = activeItem.FindParent(SolutionItemType.Project) as Community.VisualStudio.Toolkit.Project;
            if (parentProject is not null && IsCdsProjectPath(parentProject.FullPath))
            {
                return new SelectedCdsProject
                {
                    ProjectFilePath = parentProject.FullPath!,
                    ProjectName = parentProject.Name
                };
            }
        }

        return null;
    }

    internal static IReadOnlyDictionary<string, string?> ParsePropertiesJson(string json)
        => MsBuildProjectPropertyEvaluator.ParsePropertiesJson(json);

    internal static string? ResolvePath(string projectDirectory, string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        return Path.IsPathRooted(path)
            ? Path.GetFullPath(path)
            : Path.GetFullPath(Path.Combine(projectDirectory, path));
    }

    private static bool IsCdsProjectPath(string? path)
        => !string.IsNullOrWhiteSpace(path)
            && string.Equals(Path.GetExtension(path), ".cdsproj", StringComparison.OrdinalIgnoreCase);

    private static async Task<string> GetActiveConfigurationNameAsync(CancellationToken cancellationToken)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
        return (Package.GetGlobalService(typeof(DTE)) as DTE2)?.Solution?.SolutionBuild?.ActiveConfiguration?.Name
            ?? "Debug";
    }
}
#nullable restore
