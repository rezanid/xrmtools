#nullable enable
namespace XrmTools.Pcf.Setup;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal enum PcfEnvironmentStatus { Ready, NodeMissing, NodeIncompatible, NpmMissing, SelectionUnavailable, Ambiguous }

internal sealed class PcfEnvironmentResolution
{
    public PcfEnvironmentStatus Status { get; init; }
    /// <summary>
    /// The Node.js to use for this project. Processes Xrm Tools starts itself can always use it; a Visual Studio build
    /// can only when <see cref="AvailableToBuilds"/>, because a build runs with the environment of its Visual Studio instance.
    /// </summary>
    public NodeInstallation? Installation { get; init; }
    public PcfNodeRequirement Requirement { get; init; } = null!;
    /// <summary>What is wrong, as a full sentence; empty when ready.</summary>
    public string Problem { get; init; } = string.Empty;
    /// <summary>What the developer should do next, as a full sentence.</summary>
    public string Remedy { get; init; } = string.Empty;
    /// <summary>The Node.js on the PATH of this Visual Studio instance; <c>null</c> when there is none.</summary>
    public NodeInstallation? InstanceNode { get; init; }
    /// <summary>The developer selected an installation for this project.</summary>
    public bool FromSelection { get; init; }
    public bool IsReady => Status == PcfEnvironmentStatus.Ready;
    public bool AvailableToBuilds => Installation is not null && Provides(Installation);

    /// <summary>Whether builds in this Visual Studio instance would run with the same Node.js version as <paramref name="node"/>.</summary>
    public bool Provides(NodeInstallation node)
        => node.Source == NodeEnvironmentSource.BuildEnvironment || (InstanceNode is { NpmLauncher: not null } instance && instance.Version.Equals(node.Version));

    public string DescribeInstance() => InstanceNode is null ? "no Node.js on its PATH"
        : InstanceNode.NpmLauncher is null ? $"Node.js {InstanceNode.Version} without npm on its PATH"
        : $"Node.js {InstanceNode.Version} on its PATH";
}

/// <summary>
/// Chooses the Node.js installation for a PCF project: an explicit selection for the project first, then a compatible
/// Node.js and npm on the PATH of this Visual Studio instance, then one on the registered Windows PATH, then a
/// compatible version already installed with fnm. It never installs or switches anything; when no choice is clearly
/// right it reports why instead of guessing.
/// </summary>
internal sealed class PcfEnvironmentResolver(INodeEnvironmentProbe probe, IPcfSetupStore store)
{
    private const string InstanceLabel = "Visual Studio's PATH";

    public async Task<PcfEnvironmentResolution> ResolveAsync(string? projectFile, string projectDirectory, CancellationToken cancellationToken)
    {
        var requirement = PcfNodeRequirement.Read(projectDirectory);
        var selection = projectFile is null ? null : store.GetSelection(projectFile);
        var instance = probe.FindNodeDirectory(probe.BuildPath) is { } instanceDirectory
            ? await probe.InspectAsync(instanceDirectory, probe.BuildPath, NodeEnvironmentSource.BuildEnvironment, InstanceLabel, cancellationToken).ConfigureAwait(false)
            : null;

        PcfEnvironmentResolution Ready(NodeInstallation installation) => new()
        {
            Status = PcfEnvironmentStatus.Ready, Installation = installation, Requirement = requirement, InstanceNode = instance, FromSelection = selection is not null
        };
        PcfEnvironmentResolution Problem(PcfEnvironmentStatus status, string problem, string remedy) => new()
        {
            Status = status, Requirement = requirement, Problem = problem, Remedy = remedy, InstanceNode = instance, FromSelection = selection is not null
        };
        PcfEnvironmentResolution FromFnm(NodeInstallation installation) => installation.NpmLauncher is null
            ? Problem(PcfEnvironmentStatus.NpmMissing, $"Node.js {installation.Version} from fnm has no npm beside it ({installation.NodeDirectory}).",
                "Reinstall that version with fnm, or choose an installation that includes npm.")
            : Ready(installation);

        if (selection is not null)
        {
            var selected = await probe.InspectAsync(selection.NodeDirectory, selection.NodeDirectory, NodeEnvironmentSource.Selected, "selected for this project", cancellationToken).ConfigureAwait(false);
            if (selected is null)
                return Problem(PcfEnvironmentStatus.SelectionUnavailable,
                    $"The Node.js installation selected for this project is no longer available ({selection.NodeDirectory}).", "Choose another installation.");
            if (selected.NpmLauncher is null)
                return Problem(PcfEnvironmentStatus.NpmMissing,
                    $"The selected Node.js {selected.Version} has no npm beside it ({selected.NodeDirectory}).", "Repair that installation or choose one that includes npm.");
            // When the declarations themselves disagree, the explicit selection is the developer's answer.
            if (requirement.Conflict is null && requirement.Check(selected.Version, out var mismatch) == NodeCompatibility.Incompatible)
                return Problem(PcfEnvironmentStatus.NodeIncompatible,
                    $"The selected Node.js {selected.Version} no longer fits this project: {mismatch}.", "Choose a matching installation.");
            // A build runs with what this Visual Studio instance has. When that is the selected version, even from
            // another folder, the selection is already met and the instance's own Node.js is what gets used.
            return Ready(instance is { NpmLauncher: not null } && instance.Version.Equals(selected.Version) ? instance : selected);
        }
        if (requirement.Conflict is not null)
            return Problem(PcfEnvironmentStatus.Ambiguous, $"The project's Node.js declarations disagree: {requirement.Conflict}.",
                "Correct the declarations, or choose the Node.js installation to use for this project.");

        string? rejected = null;
        NodeInstallation? withoutNpm = null;
        var onPath = new List<NodeInstallation>();
        if (instance is not null) onPath.Add(instance);
        if (probe.FindNodeDirectory(probe.WindowsPath) is { } windowsDirectory && (instance is null || !NodeInstallation.SameDirectory(instance.NodeDirectory, windowsDirectory))
            && await probe.InspectAsync(windowsDirectory, probe.WindowsPath, NodeEnvironmentSource.WindowsPath, "Windows PATH, newer than this Visual Studio session", cancellationToken).ConfigureAwait(false) is { } windows)
            onPath.Add(windows);
        foreach (var found in onPath)
        {
            var compatibility = requirement.Check(found.Version, out var reason);
            if (compatibility == NodeCompatibility.Unverified && requirement.VersionFilePath is not null)
            {
                // Aliases such as lts/* only mean something to a version manager; ask fnm when it is there.
                var alias = await probe.ResolveFnmAsync(requirement.VersionFilePath, cancellationToken).ConfigureAwait(false);
                if (alias.Installation is not null && !alias.Installation.Version.Equals(found.Version))
                {
                    compatibility = NodeCompatibility.Incompatible;
                    reason = $"{Path.GetFileName(requirement.VersionFilePath)} asks for {requirement.VersionSpec}, which fnm resolves to Node.js {alias.Installation.Version}";
                }
            }
            if (compatibility == NodeCompatibility.Incompatible) { rejected ??= $"Node.js {found.Version} on {found.Label} does not fit this project: {reason}"; continue; }
            if (found.NpmLauncher is null) { withoutNpm ??= found; continue; }
            return Ready(found);
        }

        var versions = await probe.ListFnmVersionsAsync(cancellationToken).ConfigureAwait(false);
        if (versions is { Count: > 0 })
        {
            if (requirement.VersionFilePath is not null)
            {
                var file = Path.GetFileName(requirement.VersionFilePath);
                var resolved = await probe.ResolveFnmAsync(requirement.VersionFilePath, cancellationToken).ConfigureAwait(false);
                if (resolved.Installation is { } installation)
                {
                    if (requirement.Check(installation.Version, out var reason) == NodeCompatibility.Incompatible)
                        return Problem(PcfEnvironmentStatus.NodeIncompatible, $"fnm resolved {file} to Node.js {installation.Version}, but {reason}.",
                            "Install a matching version with fnm, or choose an installation.");
                    return FromFnm(installation);
                }
                if (resolved.NotInstalled)
                    return Problem(PcfEnvironmentStatus.NodeIncompatible, $"{file} asks for Node.js {requirement.VersionSpec}, which is not installed in fnm.",
                        $"Run 'fnm install' in '{Path.GetDirectoryName(requirement.VersionFilePath)}', or choose another installation.");
                rejected ??= $"fnm could not resolve {file} ({requirement.VersionSpec})";
            }
            else
            {
                var fitting = versions.Where(version => !NodeVersion.TryParse(version.Name, out var parsed)
                    || NodeVersionRange.Satisfies(parsed, requirement.EnginesRange) != false).ToArray();
                var names = string.Join(", ", (fitting.Length > 0 ? fitting : versions.ToArray()).Select(version => version.Name));
                // The developer's fnm default wins; otherwise only an unambiguous match is used without asking.
                var choice = fitting.FirstOrDefault(version => version.IsDefault) ?? (fitting.Length == 1 ? fitting[0] : null);
                if (choice is not null)
                {
                    var resolved = await probe.ResolveFnmAsync(choice.IsDefault ? "default" : choice.Name, cancellationToken).ConfigureAwait(false);
                    if (resolved.Installation is not null) return FromFnm(resolved.Installation);
                    rejected ??= $"fnm could not start Node.js {choice.Name}";
                }
                else if (fitting.Length > 1)
                    return Problem(PcfEnvironmentStatus.Ambiguous,
                        requirement.EnginesRange is null
                            ? $"fnm has several Node.js versions installed ({names}) and no default."
                            : $"Several Node.js versions installed with fnm fit engines.node {requirement.EnginesRange} ({names}), and the fnm default is not one of them.",
                        "Choose the version to use for this project.");
                else rejected ??= $"None of the Node.js versions installed with fnm ({names}) fit engines.node {requirement.EnginesRange}";
            }
        }

        if (rejected is not null)
            return Problem(PcfEnvironmentStatus.NodeIncompatible, rejected + ".", "Install a Node.js version that fits, or choose an installation.");
        if (withoutNpm is not null)
            return Problem(PcfEnvironmentStatus.NpmMissing, $"Node.js {withoutNpm.Version} was found in '{withoutNpm.NodeDirectory}', but npm was not found.",
                "Repair that installation or choose one that includes npm.");
        return Problem(PcfEnvironmentStatus.NodeMissing, "Node.js was not found on Visual Studio's PATH, on the Windows PATH, or in fnm.",
            "Install Node.js (LTS), or choose an existing installation.");
    }

    /// <summary>Every installation the developer could choose, for the setup experience.</summary>
    public async Task<IReadOnlyList<NodeInstallation>> GetCandidatesAsync(string? projectFile, CancellationToken cancellationToken)
    {
        var candidates = new List<NodeInstallation>();
        void Add(NodeInstallation? installation)
        {
            if (installation is not null && !candidates.Any(existing => existing.IsSameEnvironment(installation))) candidates.Add(installation);
        }
        // This Visual Studio instance's own Node.js comes first: it is the only one that needs no restart to build.
        if (probe.FindNodeDirectory(probe.BuildPath) is { } build)
            Add(await probe.InspectAsync(build, probe.BuildPath, NodeEnvironmentSource.BuildEnvironment, InstanceLabel, cancellationToken).ConfigureAwait(false));
        var selection = projectFile is null ? null : store.GetSelection(projectFile);
        if (selection is not null)
            Add(await probe.InspectAsync(selection.NodeDirectory, selection.NodeDirectory, NodeEnvironmentSource.Selected, "selected for this project", cancellationToken).ConfigureAwait(false));
        if (probe.FindNodeDirectory(probe.WindowsPath) is { } windows)
            Add(await probe.InspectAsync(windows, probe.WindowsPath, NodeEnvironmentSource.WindowsPath, "Windows PATH", cancellationToken).ConfigureAwait(false));
        foreach (var version in await probe.ListFnmVersionsAsync(cancellationToken).ConfigureAwait(false) ?? [])
        {
            // The default is listed through its alias, the same location automatic selection uses, so it appears once.
            var resolved = await probe.ResolveFnmAsync(version.IsDefault ? "default" : version.Name, cancellationToken).ConfigureAwait(false);
            Add(resolved.Installation?.WithSource(NodeEnvironmentSource.Fnm, version.IsDefault ? "fnm default" : "fnm"));
        }
        return candidates;
    }
}
#nullable restore
