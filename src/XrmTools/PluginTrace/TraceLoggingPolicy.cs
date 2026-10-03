#nullable enable
namespace XrmTools.PluginTrace;

using System;
using XrmTools.Environments;

/// <summary>Rules shared by timed logging recovery and the explorer presentation.</summary>
internal static class TraceLoggingPolicy
{
    public static TraceLoggingMode RestoreMode(TraceLoggingMode previous) =>
        previous == TraceLoggingMode.Exceptions ? TraceLoggingMode.Exceptions : TraceLoggingMode.Off;

    public static bool Matches(TraceLoggingLease lease, DataverseEnvironment environment, Guid organizationId) =>
        lease.OrganizationId == organizationId && SameEnvironment(lease, environment);

    public static bool SameEnvironment(TraceLoggingLease lease, DataverseEnvironment environment) =>
        string.Equals(lease.EnvironmentUrl.TrimEnd('/'), environment.Url?.TrimEnd('/'), StringComparison.OrdinalIgnoreCase);

    public static TraceLoggingMode RestoreModeForStart(TraceLoggingLease? existing, DataverseEnvironment environment,
        Guid organizationId, TraceLoggingMode previous) =>
        RestoreMode(existing != null && Matches(existing, environment, organizationId) ? existing.RestoreMode : previous);
}
