#nullable enable
namespace XrmTools;

using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.ServiceBroker;
using NuGet.VisualStudio.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class NuGetAuditor
{
    public async Task AuditAsync(CancellationToken ct)
    {
        // Acquire NuGet MEF infra, or bail if NuGet is not ready
        var nuget = await VS.GetMefServiceAsync<NuGetInfra>();
        if (nuget is null)
            return;

        // Wait for NuGet restore/ready signal so we don't query too early
        await NuGetReadyGate.WaitForSolutionReadyAsync(nuget.UpdateEvents, ct);

        // Acquire NuGet project service via Service Broker (no client libs)
        var brokered = await VS.GetServiceAsync<SVsBrokeredServiceContainer, IBrokeredServiceContainer>();
        var sb = brokered?.GetFullAccessServiceBroker();
        if (sb is null)
            return;

        var proxy = await sb.GetProxyAsync<INuGetProjectService>(NuGetServices.NuGetProjectServiceV1, cancellationToken: ct).ConfigureAwait(false);
        using (proxy as IDisposable)
        {
            if (proxy is null)
                return;

            var projects = await VS.Solutions.GetAllProjectsAsync();
            if (projects is null) return;

            var solution = await VS.GetServiceAsync<SVsSolution, IVsSolution>();
            if (solution is null) return;

            var comparer = nuget.SemverComparer;
            var outdated = new List<(string ProjectName, string Version)>();

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(ct);

            foreach (Project project in projects)
            {
                if (project is null || project.IsCapabilityMatch("SharedAssetsProject"))
                    continue;

                project.GetItemInfo(out var hier, out _, out var _);

                if (ErrorHandler.Failed(solution.GetGuidOfProject(hier, out var projectGuid)))
                    continue;

                var installed = await proxy.GetInstalledPackagesAsync(projectGuid, ct);
                var match = installed?.Packages?
                    .FirstOrDefault(p => string.Equals(p.Id, "XrmTools.Meta.Attributes", StringComparison.OrdinalIgnoreCase));

                if (match is null) continue;

                if (comparer.Compare(match.Version, "1.1.0") < 0)
                {
                    outdated.Add((project.Name, match.Version));
                }
            }

            if (outdated.Count > 0)
            {
                await XrmToolsPackage.ShowOutdatedPackagesInfoBarAsync(outdated);
            }
        }
    }
}
#nullable restore
