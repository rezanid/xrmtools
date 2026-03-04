#nullable enable
namespace XrmTools.Helpers;

using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using VsToolkit = Community.VisualStudio.Toolkit.VS;
using XrmTools.Options;

public static class SolutionNavigator
{
    public static async Task SelectProjectInSolutionExplorerAsync(string assemblyName, string? typeName, bool openDocument = false)
    {
        if (string.IsNullOrWhiteSpace(assemblyName))
        {
            return;
        }

        var target = await FindRoslynTargetAsync(assemblyName, typeName).ConfigureAwait(false);
        if (target.project is null)
        {
            return;
        }

        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        if (await AsyncServiceProvider.GlobalProvider.GetServiceAsync(typeof(SVsSolution)) is not IVsSolution solution)
        {
            return;
        }

        var hierarchy = FindHierarchyByProjectFilePath(solution, target.project.FilePath);
        if (hierarchy is null)
        {
            return;
        }

        var itemId = VSConstants.VSITEMID_ROOT;
        if (!string.IsNullOrWhiteSpace(target.documentFilePath)
            && hierarchy.ParseCanonicalName(target.documentFilePath, out var typeItemId) == VSConstants.S_OK)
        {
            itemId = typeItemId;
        }

        await hierarchy.SelectInSolutionExplorerAsync(itemId);

        if (openDocument && !string.IsNullOrWhiteSpace(target.documentFilePath))
        {
            var options = await GeneralOptions.GetLiveInstanceAsync();
            if (!options.DataverseExplorerOpenInPreviewTab) return;
            await VsToolkit.Documents.OpenInPreviewTabAsync(target.documentFilePath!);
        }
    }

    private static async Task<(Project? project, string? documentFilePath)> FindRoslynTargetAsync(string assemblyName, string? typeName)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        if (Package.GetGlobalService(typeof(SComponentModel)) is not IComponentModel componentModel)
        {
            return (null, null);
        }

        var workspace = componentModel.GetService<VisualStudioWorkspace>();
        if (workspace is null)
        {
            return (null, null);
        }

        var candidates = workspace.CurrentSolution.Projects
            .Where(p => string.Equals(p.AssemblyName, assemblyName, StringComparison.OrdinalIgnoreCase)
                || string.Equals(p.Name, assemblyName, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (candidates.Count == 0)
        {
            return (null, null);
        }

        if (string.IsNullOrWhiteSpace(typeName))
        {
            return (candidates[0], null);
        }

        foreach (var candidate in candidates)
        {
            var sourceFilePath = await FindTypeSourceFilePathAsync(candidate, typeName).ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(sourceFilePath))
            {
                return (candidate, sourceFilePath);
            }
        }

        return (candidates[0], null);
    }

    private static IVsHierarchy? FindHierarchyByProjectFilePath(IVsSolution solution, string? projectFilePath)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        if (string.IsNullOrWhiteSpace(projectFilePath))
        {
            return null;
        }

        foreach (var hierarchy in EnumerateLoadedProjectHierarchies(solution))
        {
            var msBuildProjectPath = hierarchy.GetProjectProperty("MSBuildProjectFullPath");
            if (PathEquals(msBuildProjectPath, projectFilePath))
            {
                return hierarchy;
            }

            if (hierarchy.GetCanonicalName(VSConstants.VSITEMID_ROOT, out var canonicalName) == VSConstants.S_OK)
            {
                if (PathEquals(canonicalName, projectFilePath))
                {
                    return hierarchy;
                }

                if (!string.IsNullOrWhiteSpace(canonicalName)
                    && PathEquals(Path.Combine(canonicalName, Path.GetFileName(projectFilePath)), projectFilePath))
                {
                    return hierarchy;
                }
            }
        }

        return null;
    }

    private static IEnumerable<IVsHierarchy> EnumerateLoadedProjectHierarchies(IVsSolution solution)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        Guid guid = Guid.Empty;
        if (solution.GetProjectEnum((uint)__VSENUMPROJFLAGS.EPF_LOADEDINSOLUTION, ref guid, out var enumHierarchies) != VSConstants.S_OK || enumHierarchies is null)
        {
            yield break;
        }

        var fetched = new IVsHierarchy[1];
        while (enumHierarchies.Next(1, fetched, out var celtFetched) == VSConstants.S_OK && celtFetched == 1)
        {
            if (fetched[0] is not null)
            {
                yield return fetched[0];
            }
        }
    }

    private static async Task<string?> FindTypeSourceFilePathAsync(Project project, string typeName)
    {
        var compilation = await project.GetCompilationAsync().ConfigureAwait(false);
        if (compilation is null)
        {
            return null;
        }

        var normalizedTypeName = NormalizeTypeName(typeName);
        var candidates = new List<INamedTypeSymbol?>
        {
            compilation.GetTypeByMetadataName(normalizedTypeName),
            compilation.GetTypeByMetadataName(normalizedTypeName.Replace('.', '+')),
            compilation.GetTypeByMetadataName(normalizedTypeName.Replace('+', '.'))
        };

        var symbol = candidates.FirstOrDefault(s => s is not null);
        symbol ??= FindTypeBySimpleName(compilation.Assembly.GlobalNamespace, GetSimpleTypeName(typeName));

        return symbol?.Locations
            .FirstOrDefault(l => l.IsInSource && !string.IsNullOrWhiteSpace(l.SourceTree?.FilePath))?
            .SourceTree?
            .FilePath;
    }

    private static INamedTypeSymbol? FindTypeBySimpleName(INamespaceSymbol ns, string simpleTypeName)
    {
        foreach (var type in ns.GetTypeMembers())
        {
            var match = FindTypeBySimpleName(type, simpleTypeName);
            if (match is not null)
            {
                return match;
            }
        }

        foreach (var childNs in ns.GetNamespaceMembers())
        {
            var match = FindTypeBySimpleName(childNs, simpleTypeName);
            if (match is not null)
            {
                return match;
            }
        }

        return null;
    }

    private static INamedTypeSymbol? FindTypeBySimpleName(INamedTypeSymbol type, string simpleTypeName)
    {
        if (string.Equals(type.Name, simpleTypeName, StringComparison.Ordinal))
        {
            return type;
        }

        foreach (var nested in type.GetTypeMembers())
        {
            var match = FindTypeBySimpleName(nested, simpleTypeName);
            if (match is not null)
            {
                return match;
            }
        }

        return null;
    }

    private static bool PathEquals(string? path1, string? path2)
    {
        if (string.IsNullOrWhiteSpace(path1) || string.IsNullOrWhiteSpace(path2))
        {
            return false;
        }

        try
        {
            return string.Equals(Path.GetFullPath(path1), Path.GetFullPath(path2), StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private static string GetSimpleTypeName(string typeName)
    {
        var type = NormalizeTypeName(typeName);
        if (string.IsNullOrEmpty(type))
        {
            return string.Empty;
        }

        var genericSeparator = type.IndexOf('`');
        if (genericSeparator >= 0)
        {
            type = type[..genericSeparator];
        }

        var lastNested = type.LastIndexOf('+');
        var lastNamespace = type.LastIndexOf('.');
        var separator = Math.Max(lastNested, lastNamespace);
        return separator >= 0 && separator < type.Length - 1 ? type[(separator + 1)..] : type;
    }

    private static string NormalizeTypeName(string typeName)
    {
        var type = typeName?.Trim();
        if (string.IsNullOrEmpty(type))
        {
            return string.Empty;
        }

        var assemblySeparator = type.IndexOf(',');
        if (assemblySeparator >= 0)
        {
            type = type[..assemblySeparator];
        }

        var genericArgumentsSeparator = type.IndexOf('[');
        if (genericArgumentsSeparator >= 0)
        {
            type = type[..genericArgumentsSeparator];
        }

        return type;
    }
}
#nullable restore