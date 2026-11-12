#nullable enable
namespace XrmTools.DataverseSolutions;

using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.Properties;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class PcfSolutionDestination(string projectFilePath, string solutionUniqueName)
{
    public string ProjectFilePath { get; } = projectFilePath;
    public string SolutionUniqueName { get; } = solutionUniqueName;
}

internal interface IPcfPushDestinationResolver
{
    Task<IReadOnlyList<PcfSolutionDestination>> FindAsync(string pcfProjectPath, CancellationToken token);
}

[Export(typeof(IPcfPushDestinationResolver))]
internal sealed class PcfPushDestinationResolver : IPcfPushDestinationResolver
{
    public async Task<IReadOnlyList<PcfSolutionDestination>> FindAsync(string pcfProjectPath, CancellationToken token)
    {
        var projects = await VS.Solutions.GetAllProjectsAsync();
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(token);
        var loadedProjects = new List<UnconfiguredProject>();
        foreach (var project in projects.Where(project => project.IsLoaded
            && string.Equals(Path.GetExtension(project.FullPath), ".cdsproj", StringComparison.OrdinalIgnoreCase)))
        {
            project.GetItemInfo(out var hierarchy, out _, out _);
            ErrorHandler.ThrowOnFailure(hierarchy.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ExtObject, out var automation));
            var context = automation as IVsBrowseObjectContext ?? (automation as EnvDTE.Project)?.Object as IVsBrowseObjectContext;
            if (context is null)
                throw new InvalidOperationException($"Could not inspect project references for '{project.FullPath}'. Choose the push destination explicitly.");
            loadedProjects.Add(context.UnconfiguredProject);
        }

        await TaskScheduler.Default;
        var destinations = new List<PcfSolutionDestination>();
        foreach (var project in loadedProjects)
        {
            token.ThrowIfCancellationRequested();
            try
            {
                var snapshot = await project.Services.ProjectLockService.ReadLockAsync(async access =>
                {
                    token.ThrowIfCancellationRequested();
                    var configured = await project.GetSuggestedConfiguredProjectAsync()
                        ?? throw new InvalidOperationException("The active project configuration is unavailable.");
                    var evaluation = await access.GetProjectAsync(configured);
                    return (References: evaluation.GetItems("ProjectReference").Select(item => item.GetMetadataValue("FullPath")).ToArray(),
                        Root: evaluation.GetPropertyValue("SolutionRootPath"));
                });
                token.ThrowIfCancellationRequested();
                var destination = ReadDestination(project.FullPath, pcfProjectPath, snapshot.References, snapshot.Root);
                if (destination is not null) destinations.Add(destination);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                throw new InvalidOperationException($"Could not determine the push destination from '{project.FullPath}': {ex.Message}", ex);
            }
        }
        token.ThrowIfCancellationRequested();
        return destinations.OrderBy(item => item.ProjectFilePath, StringComparer.OrdinalIgnoreCase).ToArray();
    }

    internal static PcfSolutionDestination? ReadDestination(string projectPath, string pcfProjectPath, IEnumerable<string> evaluatedReferencePaths, string? solutionRootPath)
    {
        var controlPath = Path.GetFullPath(pcfProjectPath);
        if (!evaluatedReferencePaths.Any(path => string.Equals(Path.GetFullPath(path), controlPath, StringComparison.OrdinalIgnoreCase)))
            return null;
        var directory = Path.GetDirectoryName(projectPath) ?? throw new InvalidOperationException($"Could not determine the directory of '{projectPath}'.");
        var root = CdsProjectResolver.ResolvePath(directory, solutionRootPath) ?? Path.Combine(directory, "src");
        return new PcfSolutionDestination(projectPath, SolutionManifestReader.ReadUniqueName(root));
    }
}
#nullable restore
