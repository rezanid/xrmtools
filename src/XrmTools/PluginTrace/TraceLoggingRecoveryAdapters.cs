#nullable enable
namespace XrmTools.PluginTrace;

using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.Environments;
using XrmTools.Options;

internal interface ITraceLoggingLeaseStore
{
    Task<TraceLoggingLease?> LoadAsync();
    Task SaveAsync(TraceLoggingLease? lease);
}

internal sealed class TraceLoggingLeaseStore : ITraceLoggingLeaseStore
{
    public async Task<TraceLoggingLease?> LoadAsync()
    {
        var json = (await TraceExplorerOptions.GetLiveInstanceAsync()).TimedTraceLoggingLeaseJson;
        try { return string.IsNullOrWhiteSpace(json) ? null : JsonSerializer.Deserialize<TraceLoggingLease>(json); }
        catch (JsonException ex)
        {
            ActivityLog.TryLogWarning("XrmTools.PluginTrace", "Could not read the timed logging lease: " + ex.Message);
            return null;
        }
    }
    public async Task SaveAsync(TraceLoggingLease? lease)
    {
        var options = await TraceExplorerOptions.GetLiveInstanceAsync();
        options.TimedTraceLoggingLeaseJson = lease == null ? "" : JsonSerializer.Serialize(lease);
        await options.SaveAsync();
    }
    public static async Task<IReadOnlyList<DataverseEnvironment>> GetEnvironmentsAsync() =>
        (await GeneralOptions.GetLiveInstanceAsync()).Environments;
}

internal interface ITraceLoggingLeaseTimer : IDisposable
{
    event Action Tick;
    void SetEnabled(bool enabled);
}

/// <summary>Short wake-up pulses; UTC deadlines, not elapsed timer time, determine expiry.</summary>
internal sealed class TraceLoggingLeaseTimer : ITraceLoggingLeaseTimer
{
    internal static readonly TimeSpan CheckInterval = TimeSpan.FromSeconds(30);
    private readonly Timer timer;
    private readonly object lifetime = new();
    private bool disposed;
    public event Action? Tick;
    public TraceLoggingLeaseTimer() => timer = new Timer(_ => Tick?.Invoke(), null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
    public void SetEnabled(bool enabled)
    {
        lock (lifetime)
        {
            if (!disposed) timer.Change(enabled ? CheckInterval : Timeout.InfiniteTimeSpan,
                enabled ? CheckInterval : Timeout.InfiniteTimeSpan);
        }
    }
    public void Dispose()
    {
        lock (lifetime) { disposed = true; timer.Dispose(); }
    }
}
