#nullable enable
namespace XrmTools;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TaskStatusCenter;
using NuGet.VisualStudio;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.UI.InfoBars;

public partial class XrmToolsPackage
{
    private async Task AuditNugetsAsync(ITaskHandler handler, CancellationToken ct)
    {
        handler.Progress.Report(new TaskProgressData
        {
            CanBeCanceled = false,
            PercentComplete = 10,
            ProgressText = "Scanning..."
        });
        try
        {
            // Delegate the audit logic to NuGetAuditor for readability and testability
            var auditor = new NuGetAuditor();
            await auditor.AuditAsync(ct);
        }
        catch (Exception ex)
        {
            // Keep failures silent for UX; if desired, log to Output window.
            var pane = await VS.Windows.CreateOutputWindowPaneAsync("XrmTools");
            await pane.WriteLineAsync($"[XrmTools] Package audit failed: {ex.Message}");
        }
        handler.Progress.Report(new TaskProgressData
        {
            PercentComplete = 100,
            ProgressText = "Scan completed"
        });
    }

    public static async Task ShowOutdatedPackagesInfoBarAsync(IReadOnlyList<(string ProjectName, string Version)> hits)
    {
        var infoBar = new NugetUpdateInfoBar
        {
            PromptMessage = $"“XrmTools.Meta.Attributes” is below 1.0.56 in {hits.Count} project(s): " +
            string.Join(", ", hits.Take(5).Select(h => $"{h.ProjectName} ({h.Version})")) +
            (hits.Count > 5 ? ", …" : "")
        };
        await infoBar.TryShowAsync();
    }
}

[Export]
internal sealed class NuGetInfra
{
    [ImportingConstructor]
    public NuGetInfra(
        IVsNuGetProjectUpdateEvents updateEvents,
        IVsSemanticVersionComparer semverComparer)
    {
        UpdateEvents = updateEvents;
        SemverComparer = semverComparer;
    }

    public IVsNuGetProjectUpdateEvents UpdateEvents { get; }
    public IVsSemanticVersionComparer SemverComparer { get; }
}

internal static class NuGetReadyGate
{
    public static Task WaitForSolutionReadyAsync(IVsNuGetProjectUpdateEvents eventsSvc, CancellationToken ct)
    {
        var tcs = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);

        void Complete()
        {
            eventsSvc.SolutionRestoreFinished -= OnCompleted;
            tcs.TrySetResult(null);
        }

        void OnCompleted(IReadOnlyList<string> projects) => Complete();

        // If SolutionRestoreCompleted may have already fired, still provide a small delay to let NuGet init.
        eventsSvc.SolutionRestoreFinished += OnCompleted;

        // Safety timeout: we don't want to block forever if there is no restore
        var timeout = Task.Delay(TimeSpan.FromSeconds(10), ct); // “slightly after”
        return Task.WhenAny(tcs.Task, timeout);
    }
}
#nullable restore