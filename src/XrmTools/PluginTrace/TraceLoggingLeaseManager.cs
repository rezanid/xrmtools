#nullable enable
namespace XrmTools.PluginTrace;

using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.Environments;

internal sealed record TraceLoggingLease(
    string EnvironmentUrl,
    Guid OrganizationId,
    TraceLoggingMode RestoreMode,
    DateTimeOffset ExpiresAtUtc,
    DateTimeOffset CleanupAtUtc,
    DateTimeOffset? LastAttemptAtUtc,
    int FailedAttempts = 0,
    DateTimeOffset? RetryAtUtc = null,
    string? LastError = null);

/// <summary>Owns persisted, best-effort UTC deadline recovery independently of the explorer.</summary>
internal sealed class TraceLoggingLeaseManager : IDisposable
{
    private readonly ITraceExplorerService service;
    private readonly ITraceLoggingLeaseStore store;
    private readonly ITraceLoggingLeaseTimer timer;
    private readonly Func<Task<IReadOnlyList<DataverseEnvironment>>> getEnvironments;
    private readonly Func<DateTimeOffset> now;
    private readonly Action<string, bool> report;
    private readonly SemaphoreSlim gate = new(1, 1);
    private volatile TraceLoggingLease? lease;
    private volatile bool initialized, disposed;

    public event Action<TraceLoggingLease?>? Changed;
    public TraceLoggingLease? Current => lease;

    public TraceLoggingLeaseManager(ITraceExplorerService service)
        : this(service, new TraceLoggingLeaseStore(), new TraceLoggingLeaseTimer(), TraceLoggingLeaseStore.GetEnvironmentsAsync,
            () => DateTimeOffset.UtcNow, ReportToActivityLog) { }

    internal TraceLoggingLeaseManager(ITraceExplorerService service, ITraceLoggingLeaseStore store,
        ITraceLoggingLeaseTimer timer, Func<Task<IReadOnlyList<DataverseEnvironment>>> getEnvironments,
        Func<DateTimeOffset> now, Action<string, bool> report)
    {
        this.service = service; this.store = store; this.timer = timer;
        this.getEnvironments = getEnvironments; this.now = now; this.report = report;
        timer.Tick += TimerTick;
    }

    public async Task InitializeAsync()
    {
        if (initialized || disposed) return;
        await gate.WaitAsync();
        try
        {
            if (initialized || disposed) return;
            lease = await store.LoadAsync();
            if (lease != null)
            {
                // Migrate older persisted leases without a cleanup deadline or safe restore mode.
                lease = lease with
                {
                    CleanupAtUtc = lease.CleanupAtUtc <= lease.ExpiresAtUtc ? lease.ExpiresAtUtc.AddDays(7) : lease.CleanupAtUtc,
                    RestoreMode = TraceLoggingPolicy.RestoreMode(lease.RestoreMode)
                };
            }
            initialized = true;
            Schedule();
        }
        finally { gate.Release(); }
        // Startup is an opportunity to recover immediately, even if a previous process scheduled a retry.
        await ReconcileAsync(ignoreRetryDelay: true);
    }

    public async Task StartAsync(DataverseEnvironment environment, Guid organizationId, TraceLoggingMode restoreMode, CancellationToken cancellation)
    {
        await InitializeAsync();
        await gate.WaitAsync(cancellation);
        try
        {
            var expiresAt = now().AddHours(1);
            restoreMode = TraceLoggingPolicy.RestoreModeForStart(lease, environment, organizationId, restoreMode);
            lease = new TraceLoggingLease(environment.Url ?? "", organizationId, restoreMode, expiresAt, expiresAt.AddDays(7), null);
            await store.SaveAsync(lease); // Save before enabling tracing so recovery survives a VS crash.
            await service.SetTraceLoggingAsync(environment, organizationId, TraceLoggingMode.All, cancellation, interactive: true, verifySelectedEnvironment: true);
            Schedule();
        }
        catch
        {
            lease = null;
            await store.SaveAsync(null);
            Schedule();
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
            if (lease == null || !TraceLoggingPolicy.Matches(lease, environment, organizationId)) return;
            var expiresAt = now().AddHours(1);
            lease = lease with { ExpiresAtUtc = expiresAt, CleanupAtUtc = expiresAt.AddDays(7), LastAttemptAtUtc = null,
                FailedAttempts = 0, RetryAtUtc = null, LastError = null };
            await store.SaveAsync(lease);
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
            if (lease == null || !TraceLoggingPolicy.Matches(lease, environment, organizationId)) return;
            await ClearAsync();
        }
        finally { gate.Release(); }
        Changed?.Invoke(null);
    }

    internal async Task ReconcileAsync(bool ignoreRetryDelay = false)
    {
        if (!initialized || disposed || lease == null || lease.ExpiresAtUtc > now()) return;
        // Periodic ticks must not queue behind an in-flight restoration or a manual logging change.
        if (!await gate.WaitAsync(0)) return;
        bool changed = false;
        try
        {
            if (disposed || lease == null || lease.ExpiresAtUtc > now()) return;
            if (lease.CleanupAtUtc <= now())
            {
                report($"Timed trace recovery for {lease.EnvironmentUrl} was abandoned after its cleanup deadline.", true);
                await ClearAsync(); changed = true;
            }
            else if (ignoreRetryDelay || lease.RetryAtUtc == null || lease.RetryAtUtc <= now())
            {
                var environment = (await getEnvironments()).FirstOrDefault(candidate => candidate.IsValid && TraceLoggingPolicy.SameEnvironment(lease, candidate));
                if (environment == null)
                {
                    report($"Timed trace recovery for {lease.EnvironmentUrl} was abandoned because the environment is no longer configured.", true);
                    await ClearAsync(); changed = true;
                }
                else
                {
                    lease = lease with { LastAttemptAtUtc = now(), RetryAtUtc = null };
                    await store.SaveAsync(lease);
                    report($"Attempting timed trace restoration to {lease.RestoreMode} for {lease.EnvironmentUrl}.", false);
                    using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(2));
                    var configuration = await service.GetTraceLoggingAsync(environment, timeout.Token, interactive: false, verifySelectedEnvironment: false);
                    if (configuration.OrganizationId == lease.OrganizationId && configuration.Mode == TraceLoggingMode.All)
                    {
                        await service.SetTraceLoggingAsync(environment, lease.OrganizationId, TraceLoggingPolicy.RestoreMode(lease.RestoreMode),
                            timeout.Token, interactive: false, verifySelectedEnvironment: false);
                        report($"Restored timed trace logging to {lease.RestoreMode} for {lease.EnvironmentUrl}.", false);
                    }
                    else
                        report($"Timed trace recovery for {lease.EnvironmentUrl} completed without changing settings: the organization or logging mode has changed.", false);
                    await ClearAsync(); changed = true;
                }
            }
        }
        catch (Exception ex)
        {
            if (lease != null)
            {
                var attempts = Math.Min(10, Math.Max(0, lease.FailedAttempts)) + 1;
                var delay = TimeSpan.FromSeconds(Math.Min(300, 30 * Math.Pow(2, Math.Min(attempts - 1, 4))));
                var error = ex is OperationCanceledException ? "Restoration timed out or was canceled." : ex.Message;
                lease = lease with { FailedAttempts = attempts, RetryAtUtc = now().Add(delay), LastError = error };
                report($"Timed trace restoration for {lease.EnvironmentUrl} failed: {error} Retry at {lease.RetryAtUtc:O}.", true);
                try { await store.SaveAsync(lease); }
                catch (Exception persistenceError) { report("Could not save trace recovery diagnostics: " + persistenceError.Message, true); }
                changed = true;
            }
        }
        finally { gate.Release(); }
        if (changed) Changed?.Invoke(lease);
    }

    private async Task ClearAsync()
    {
        // Keep the in-memory recovery record until clearing durable state succeeds.
        await store.SaveAsync(null);
        lease = null;
        Schedule();
    }
    private void Schedule() { if (!disposed) timer.SetEnabled(lease != null); }
    private void TimerTick() => _ = CheckFromTimerAsync();
    private async Task CheckFromTimerAsync()
    {
        try { await ReconcileAsync(); }
        catch (Exception ex) { report("Unexpected timed trace recovery error: " + ex.Message, true); }
    }
    private static void ReportToActivityLog(string message, bool warning)
    {
        if (warning) ActivityLog.TryLogWarning("XrmTools.PluginTrace", message);
        else ActivityLog.TryLogInformation("XrmTools.PluginTrace", message);
    }
    public void Dispose()
    {
        disposed = true; timer.Tick -= TimerTick; timer.Dispose();
        // An in-flight callback can still own the semaphore; do not dispose it underneath that callback.
    }
}
