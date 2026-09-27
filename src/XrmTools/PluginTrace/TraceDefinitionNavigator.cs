#nullable enable
namespace XrmTools.PluginTrace;

using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal static class TraceDefinitionNavigator
{
    public static async Task<string> NavigateAsync(string typeName, CancellationToken cancellation)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellation);
        var components = await AsyncServiceProvider.GlobalProvider.GetServiceAsync(typeof(SComponentModel)) as IComponentModel;
        var workspace = components?.GetService<VisualStudioWorkspace>();
        if (workspace == null) return "The current solution is not available for navigation.";
        var solution = workspace.CurrentSolution;
        var targets = await FindAsync(solution, typeName, cancellation);
        if (targets.Count == 0) return $"No source definition for '{typeName}' was found in the current solution.";
        if (targets.Count > 1) return "This type exists in multiple solution projects. Its assembly name is needed to select the correct definition.";
        var target = targets[0];
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellation);
        if (workspace.CurrentSolution.Id != solution.Id) return "The solution changed during navigation. Try Go To Definition again.";
        VsShellUtilities.OpenDocument(ServiceProvider.GlobalProvider, target.FilePath, VSConstants.LOGVIEWID.Code_guid,
            out _, out _, out var frame, out var view);
        ErrorHandler.ThrowOnFailure(frame.Show());
        if (view != null)
        {
            ErrorHandler.ThrowOnFailure(view.SetCaretPos(target.Line, target.Column));
            ErrorHandler.ThrowOnFailure(view.CenterLines(target.Line, 1));
        }
        return "Opened " + target.FilePath;
    }

    // Resolve only source symbols owned by solution projects, never an unrelated same-named type or metadata reference.
    internal static async Task<IReadOnlyList<TraceDefinition>> FindAsync(Solution solution, string typeName, CancellationToken cancellation)
    {
        var parts = typeName.Split(',');
        var name = parts[0].Trim();
        var assembly = parts.Length > 1 ? parts[1].Trim() : null;
        var targets = new List<TraceDefinition>();
        if (name.Length == 0) return targets;
        foreach (var project in solution.Projects.Where(p => assembly == null || string.Equals(p.AssemblyName, assembly, StringComparison.OrdinalIgnoreCase)))
        {
            cancellation.ThrowIfCancellationRequested();
            var compilation = await project.GetCompilationAsync(cancellation).ConfigureAwait(false);
            var symbol = compilation?.Assembly.GetTypeByMetadataName(name);
            if (symbol == null) continue;
            // Partial declarations belong to one definition; choose the first source location deterministically.
            var location = symbol.Locations.Where(l => l.IsInSource && !string.IsNullOrEmpty(l.SourceTree?.FilePath))
                .OrderBy(l => l.SourceTree!.FilePath, StringComparer.OrdinalIgnoreCase).ThenBy(l => l.SourceSpan.Start).FirstOrDefault();
            if (location == null) continue;
            var position = location.GetLineSpan().StartLinePosition;
            var target = new TraceDefinition(location.SourceTree!.FilePath, position.Line, position.Character);
            if (!targets.Any(t => string.Equals(t.FilePath, target.FilePath, StringComparison.OrdinalIgnoreCase) && t.Line == target.Line && t.Column == target.Column))
                targets.Add(target);
        }
        return targets;
    }
}

internal sealed record TraceDefinition(string FilePath, int Line, int Column);
