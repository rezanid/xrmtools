#nullable enable
namespace XrmTools.PluginTrace;

using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.Environments;
using XrmTools.Options;

internal sealed record TraceLoggingLease(
    string EnvironmentUrl,
    Guid OrganizationId,
    TraceLoggingMode RestoreMode,
    DateTimeOffset ExpiresAtUtc,
    DateTimeOffset CleanupAtUtc,
    DateTimeOffset? LastAttemptAtUtc);

/// <summary>Owns a timed trace-logging lease independently of the explorer tool window.</summary>
internal sealed class TraceLoggingLeaseManager : IDisposable
{
    private readonly TraceExplorerService service;
    private readonly SemaphoreSlim gate = new(1, 1);
    private readonly Timer timer;
    private TraceLoggingLease? lease;
    private bool initialized;

    public event Action<TraceLoggingLease?>? Changed;
    public TraceLoggingLease? Current => lease;

    public TraceLoggingLeaseManager(TraceExplorerService service)
    {
        this.service = service;
        timer = new Timer(_ => _ = ReconcileAsync(), null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
    }

    public async Task InitializeAsync()
    {
        if (initialized) return;
        await gate.WaitAsync();
        try
        {
            if (initialized) return;
            var options = await TraceExplorerOptions.GetLiveInstanceAsync();
            try
            {
                lease = string.IsNullOrWhiteSpace(options.TimedTraceLoggingLeaseJson)
                    ? null
                    : JsonSerializer.Deserialize<TraceLoggingLease>(options.TimedTraceLoggingLeaseJson);
                // Leases written by earlier builds did not have a cleanup deadline.
                if (lease != null && lease.CleanupAtUtc <= lease.ExpiresAtUtc)
                    lease = lease with { CleanupAtUtc = lease.ExpiresAtUtc.AddDays(7) };
            }
            catch (JsonException) { lease = null; }
            initialized = true;
            Schedule();
        }
        finally { gate.Release(); }
        await ReconcileAsync();
    }

    public async Task StartAsync(DataverseEnvironment environment, Guid organizationId, TraceLoggingMode restoreMode, CancellationToken cancellation)
    {
        await InitializeAsync();
        var expiresAt = DateTimeOffset.UtcNow.AddHours(1);
        var next = new TraceLoggingLease(environment.Url ?? "", organizationId, restoreMode, expiresAt, expiresAt.AddDays(7), null);
        await gate.WaitAsync(cancellation);
        try
        {
            lease = next;
            await PersistAsync(); // Persist before enabling tracing so recovery survives a VS crash.
            await service.SetTraceLoggingAsync(environment, organizationId, TraceLoggingMode.All, cancellation, interactive: true, verifySelectedEnvironment: true);
            Schedule();
        }
        catch
        {
            lease = null;
            await PersistAsync();
            throw;
        }
        finally { gate.Release(); }
        Changed?.Invoke(lease);
    }

    public async Task ExtendAsync(DataverseEnvironment environment, Guid organizationId)
    {
        await InitializeAsync();
        await gate.WaitAsync();
        try
        {
            if (lease == null || lease.OrganizationId != organizationId || !SameEnvironment(lease, environment)) return;
            var expiresAt = DateTimeOffset.UtcNow.AddHours(1);
            lease = lease with { ExpiresAtUtc = expiresAt, CleanupAtUtc = expiresAt.AddDays(7), LastAttemptAtUtc = null };
            await PersistAsync();
            Schedule();
        }
        finally { gate.Release(); }
        Changed?.Invoke(lease);
    }

    public async Task CancelAsync(DataverseEnvironment environment, Guid organizationId)
    {
        await InitializeAsync();
        await gate.WaitAsync();
        try
        {
            if (lease == null || lease.OrganizationId != organizationId || !SameEnvironment(lease, environment)) return;
            lease = null;
            await PersistAsync();
            Schedule();
        }
        finally { gate.Release(); }
        Changed?.Invoke(null);
    }

    private async Task ReconcileAsync()
    {
        if (!initialized || lease == null || lease.ExpiresAtUtc > DateTimeOffset.UtcNow) return;
        await gate.WaitAsync();
        try
        {
            if (lease == null || lease.ExpiresAtUtc > DateTimeOffset.UtcNow) return;
            if (lease.CleanupAtUtc <= DateTimeOffset.UtcNow)
            {
                lease = null;
                await PersistAsync();
                Schedule();
                return;
            }

            var environments = (await GeneralOptions.GetLiveInstanceAsync()).Environments;
            var environment = environments.FirstOrDefault(candidate => candidate.IsValid && SameEnvironment(lease, candidate));
            // A deleted environment cannot be restored. Do not leave a permanent orphan in user settings.
            if (environment == null)
            {
                lease = null;
                await PersistAsync();
                Schedule();
                return;
            }

            lease = lease with { LastAttemptAtUtc = DateTimeOffset.UtcNow };
            await PersistAsync();
            var configuration = await service.GetTraceLoggingAsync(environment, CancellationToken.None, interactive: false, verifySelectedEnvironment: false);
            if (configuration.OrganizationId == lease.OrganizationId && configuration.Mode == TraceLoggingMode.All)
                await service.SetTraceLoggingAsync(environment, lease.OrganizationId, lease.RestoreMode, CancellationToken.None, interactive: false, verifySelectedEnvironment: false);
            lease = null;
            await PersistAsync();
            Schedule();
        }
        catch { Schedule(); }
        finally { gate.Release(); }
        Changed?.Invoke(lease);
    }

    private async Task PersistAsync()
    {
        var options = await TraceExplorerOptions.GetLiveInstanceAsync();
        options.TimedTraceLoggingLeaseJson = lease == null ? "" : JsonSerializer.Serialize(lease);
        await options.SaveAsync();
    }

    private void Schedule()
    {
        var due = lease == null ? Timeout.InfiniteTimeSpan : lease.ExpiresAtUtc - DateTimeOffset.UtcNow;
        if (lease != null && lease.ExpiresAtUtc <= DateTimeOffset.UtcNow)
            due = lease.CleanupAtUtc <= DateTimeOffset.UtcNow ? TimeSpan.Zero : TimeSpan.FromMinutes(15);
        timer.Change(due, Timeout.InfiniteTimeSpan);
    }

    private static bool SameEnvironment(TraceLoggingLease value, DataverseEnvironment environment) =>
        string.Equals(value.EnvironmentUrl.TrimEnd('/'), environment.Url?.TrimEnd('/'), StringComparison.OrdinalIgnoreCase);

    public void Dispose() { timer.Dispose(); gate.Dispose(); }
}
