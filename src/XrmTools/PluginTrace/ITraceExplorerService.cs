#nullable enable
namespace XrmTools.PluginTrace;

using System;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.Environments;

internal interface ITraceExplorerService
{
    event Action<DataverseEnvironment> EnvironmentChanged;
    event Action<DataverseEnvironment, TraceLoggingConfiguration> TraceLoggingConfigurationChanged;
    event Action<TraceLoggingLease?> TraceLoggingLeaseChanged;
    Task<DataverseEnvironment?> GetEnvironmentAsync();
    Task<TraceQueryResult> QueryAsync(DataverseEnvironment environment, TraceFilter filter, bool interactive, CancellationToken cancellation);
    Task<string> DetailAsync(DataverseEnvironment environment, Guid id, CancellationToken cancellation);
    Task<TraceLoggingConfiguration> GetTraceLoggingAsync(DataverseEnvironment environment, CancellationToken cancellation, bool interactive = true, bool verifySelectedEnvironment = true);
    Task SetTraceLoggingAsync(DataverseEnvironment environment, Guid organizationId, TraceLoggingMode mode, CancellationToken cancellation, bool interactive = true, bool verifySelectedEnvironment = true);
    Task InitializeTraceLoggingLeasesAsync();
    TraceLoggingLease? CurrentTraceLoggingLease { get; }
    Task StartTimedTraceLoggingAsync(DataverseEnvironment environment, Guid organizationId, TraceLoggingMode restoreMode, CancellationToken cancellation);
    Task ExtendTimedTraceLoggingAsync(DataverseEnvironment environment, Guid organizationId);
    Task CancelTimedTraceLoggingAsync(DataverseEnvironment environment, Guid organizationId);
}
