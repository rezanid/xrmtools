namespace XrmTools.Tests.PluginTrace;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using XrmTools.Environments;
using XrmTools.PluginTrace;
using Xunit;

public sealed class TraceLoggingLeaseManagerTests
{
    private readonly Mock<ITraceExplorerService> service = new();
    private readonly DataverseEnvironment environment = new() { ConnectionString = "Url=https://contoso.crm.dynamics.com;TenantId=test" };
    private readonly Guid organizationId = Guid.NewGuid();
    private readonly MemoryStore store = new();
    private readonly ManualTimer timer = new();
    private readonly List<(string Message, bool Warning)> diagnostics = new();
    private DateTimeOffset now = new(2026, 10, 2, 12, 0, 0, TimeSpan.Zero);

    public TraceLoggingLeaseManagerTests()
    {
        store.Value = new TraceLoggingLease(environment.Url, organizationId, TraceLoggingMode.Off, now.AddHours(1), now.AddDays(8), null);
        service.Setup(s => s.GetTraceLoggingAsync(It.IsAny<DataverseEnvironment>(), It.IsAny<CancellationToken>(), false, false))
            .ReturnsAsync(new TraceLoggingConfiguration(organizationId, TraceLoggingMode.All));
        service.Setup(s => s.SetTraceLoggingAsync(It.IsAny<DataverseEnvironment>(), It.IsAny<Guid>(), It.IsAny<TraceLoggingMode>(), It.IsAny<CancellationToken>(), false, false))
            .Returns(Task.CompletedTask);
        service.Setup(s => s.SetTraceLoggingAsync(It.IsAny<DataverseEnvironment>(), It.IsAny<Guid>(), It.IsAny<TraceLoggingMode>(), It.IsAny<CancellationToken>(), true, true))
            .Returns(Task.CompletedTask);
    }
    private TraceLoggingLeaseManager Create() => new(service.Object, store, timer,
        () => Task.FromResult<IReadOnlyList<DataverseEnvironment>>(new[] { environment }), () => now,
        (message, warning) => diagnostics.Add((message, warning)));

    [Fact]
    public async Task SuccessfulWakeRecoveryUpdatesTheExplorerAndStopsOffModePolling()
    {
        using var manager = Create();
        service.Setup(s => s.InitializeTraceLoggingLeasesAsync()).Returns(() => manager.InitializeAsync());
        service.SetupGet(s => s.CurrentTraceLoggingLease).Returns(() => manager.Current);
        service.Setup(s => s.GetEnvironmentAsync()).ReturnsAsync(environment);
        service.Setup(s => s.GetTraceLoggingAsync(It.IsAny<DataverseEnvironment>(), It.IsAny<CancellationToken>(), true, true))
            .ReturnsAsync(new TraceLoggingConfiguration(organizationId, TraceLoggingMode.All));
        service.Setup(s => s.QueryAsync(It.IsAny<DataverseEnvironment>(), It.IsAny<TraceFilter>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TraceQueryResult(Array.Empty<TraceRecord>(), false));
        service.Setup(s => s.SetTraceLoggingAsync(It.IsAny<DataverseEnvironment>(), organizationId, TraceLoggingMode.Off, It.IsAny<CancellationToken>(), false, false))
            .Callback<DataverseEnvironment, Guid, TraceLoggingMode, CancellationToken, bool, bool>((target, id, mode, _, _, _) =>
                service.Raise(s => s.TraceLoggingConfigurationChanged += null, target, new TraceLoggingConfiguration(id, mode)))
            .Returns(Task.CompletedTask);
        manager.Changed += value => service.Raise(s => s.TraceLoggingLeaseChanged += null, new object[] { value });
        var views = new Mock<ITraceViewStore>(); views.Setup(s => s.LoadAsync()).ReturnsAsync(Array.Empty<TraceFilter>());
        using var explorer = new TraceExplorerViewModel(service.Object, views.Object, Mock.Of<ITraceNavigator>(), new ImmediateDispatcher(), () => now);
        await explorer.ActivateAsync(); Assert.True(explorer.Logging.SelectedOption.Timed);
        now = now.AddHours(2); timer.Pulse(); await explorer.TickAsync(true);
        Assert.Equal(TraceLoggingMode.Off, explorer.Logging.SelectedOption.Mode);
        Assert.False(explorer.Logging.HasTimer); Assert.False(explorer.Logging.CanPoll); Assert.Contains("paused", explorer.Logging.AutoRefreshLabel);
        service.Verify(s => s.QueryAsync(It.IsAny<DataverseEnvironment>(), It.IsAny<TraceFilter>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PendingTooltipShowsTheLastFailureAndNextRetry()
    {
        service.Setup(s => s.GetTraceLoggingAsync(It.IsAny<DataverseEnvironment>(), It.IsAny<CancellationToken>(), false, false))
            .ThrowsAsync(new InvalidOperationException("Network is recovering"));
        using var manager = Create(); await manager.InitializeAsync(); now = now.AddHours(2); timer.Pulse();
        service.SetupGet(s => s.CurrentTraceLoggingLease).Returns(() => manager.Current);
        using var logging = new TraceLoggingViewModel(service.Object, () => now);
        logging.SetEnvironment(environment); logging.Confirm(environment, new TraceLoggingConfiguration(organizationId, TraceLoggingMode.All));
        Assert.Equal("Restore pending", logging.TimerText);
        Assert.Contains("Network is recovering", logging.TimerToolTip);
        Assert.Contains("Last attempt", logging.TimerToolTip); Assert.Contains("Next retry", logging.TimerToolTip);
    }

    [Fact]
    public async Task FirstPulseAfterSleepRestoresUsingUtcWithoutWaitingForAnotherHour()
    {
        using var manager = Create(); await manager.InitializeAsync();
        Assert.True(timer.Enabled); Assert.Equal(TimeSpan.FromSeconds(30), TraceLoggingLeaseTimer.CheckInterval);
        service.Verify(s => s.GetTraceLoggingAsync(It.IsAny<DataverseEnvironment>(), It.IsAny<CancellationToken>(), false, false), Times.Never);
        now = now.AddHours(2); // No timer pulses during laptop sleep.
        bool notified = false; manager.Changed += value => notified = value == null;
        timer.Pulse();
        Assert.Null(manager.Current); Assert.Null(store.Value); Assert.False(timer.Enabled); Assert.True(notified);
        service.Verify(s => s.SetTraceLoggingAsync(environment, organizationId, TraceLoggingMode.Off, It.Is<CancellationToken>(t => t.CanBeCanceled), false, false), Times.Once);
        Assert.Contains(diagnostics, d => d.Message.Contains("Restored timed trace logging") && !d.Warning);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(2, 0)]
    public async Task StartupImmediatelyRecoversAnExpiredPersistedLease(int previous, int expected)
    {
        store.Value = store.Value with { RestoreMode = (TraceLoggingMode)previous, ExpiresAtUtc = now.AddHours(-1), RetryAtUtc = now.AddMinutes(5) };
        using var manager = Create(); await manager.InitializeAsync();
        Assert.Null(manager.Current);
        service.Verify(s => s.SetTraceLoggingAsync(environment, organizationId, (TraceLoggingMode)expected, It.IsAny<CancellationToken>(), false, false), Times.Once);
    }

    [Fact]
    public async Task WakeNetworkFailureIsRecordedAndRetriedAfterThirtySeconds()
    {
        service.SetupSequence(s => s.GetTraceLoggingAsync(It.IsAny<DataverseEnvironment>(), It.IsAny<CancellationToken>(), false, false))
            .ThrowsAsync(new InvalidOperationException("Network is recovering"))
            .ReturnsAsync(new TraceLoggingConfiguration(organizationId, TraceLoggingMode.All));
        using var manager = Create(); await manager.InitializeAsync(); now = now.AddHours(2); timer.Pulse();
        Assert.Equal("Network is recovering", manager.Current.LastError);
        Assert.Equal(now, manager.Current.LastAttemptAtUtc); Assert.Equal(now.AddSeconds(30), manager.Current.RetryAtUtc);
        Assert.Equal(1, store.Value.FailedAttempts); Assert.True(timer.Enabled);
        timer.Pulse();
        service.Verify(s => s.GetTraceLoggingAsync(It.IsAny<DataverseEnvironment>(), It.IsAny<CancellationToken>(), false, false), Times.Once);
        now = now.AddSeconds(30); timer.Pulse();
        Assert.Null(manager.Current);
        Assert.Contains(diagnostics, d => d.Warning && d.Message.Contains("Network is recovering") && d.Message.Contains("Retry at"));
    }

    [Fact]
    public async Task RepeatedFailuresBackOffAndNeverExceedFiveMinutes()
    {
        service.Setup(s => s.GetTraceLoggingAsync(It.IsAny<DataverseEnvironment>(), It.IsAny<CancellationToken>(), false, false))
            .ThrowsAsync(new InvalidOperationException("Offline"));
        using var manager = Create(); await manager.InitializeAsync(); now = now.AddHours(2);
        foreach (var seconds in new[] { 30, 60, 120, 240, 300, 300 })
        {
            await manager.ReconcileAsync();
            Assert.Equal(TimeSpan.FromSeconds(seconds), manager.Current.RetryAtUtc.Value - now);
            now = manager.Current.RetryAtUtc.Value;
        }
        Assert.NotNull(store.Value); Assert.True(timer.Enabled);
    }

    [Fact]
    public async Task OverlappingPulsesDoNotQueueAdditionalRestorations()
    {
        var pending = new TaskCompletionSource<TraceLoggingConfiguration>();
        service.Setup(s => s.GetTraceLoggingAsync(It.IsAny<DataverseEnvironment>(), It.IsAny<CancellationToken>(), false, false)).Returns(pending.Task);
        using var manager = Create(); await manager.InitializeAsync(); now = now.AddHours(2);
        var first = manager.ReconcileAsync();
        await manager.ReconcileAsync(); await manager.ReconcileAsync();
        service.Verify(s => s.GetTraceLoggingAsync(It.IsAny<DataverseEnvironment>(), It.IsAny<CancellationToken>(), false, false), Times.Once);
        pending.SetResult(new TraceLoggingConfiguration(organizationId, TraceLoggingMode.All)); await first;
        Assert.Null(manager.Current);
    }

    [Fact]
    public async Task ManualExtensionKeepsTheOriginalRestoreModeAndClearsRetryDiagnostics()
    {
        using var manager = Create(); await manager.InitializeAsync();
        now = now.AddMinutes(20); await manager.ExtendAsync(environment, organizationId);
        Assert.Equal(now.AddHours(1), manager.Current.ExpiresAtUtc);
        Assert.Equal(TraceLoggingMode.Off, manager.Current.RestoreMode);
        now = now.AddMinutes(45); timer.Pulse(); Assert.NotNull(manager.Current);
        service.Verify(s => s.GetTraceLoggingAsync(It.IsAny<DataverseEnvironment>(), It.IsAny<CancellationToken>(), false, false), Times.Never);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public async Task RecoveryRespectsAnExternallyChangedMode(int mode)
    {
        service.Setup(s => s.GetTraceLoggingAsync(It.IsAny<DataverseEnvironment>(), It.IsAny<CancellationToken>(), false, false))
            .ReturnsAsync(new TraceLoggingConfiguration(organizationId, (TraceLoggingMode)mode));
        using var manager = Create(); await manager.InitializeAsync(); now = now.AddHours(2); timer.Pulse();
        Assert.Null(manager.Current);
        service.Verify(s => s.SetTraceLoggingAsync(It.IsAny<DataverseEnvironment>(), It.IsAny<Guid>(), It.IsAny<TraceLoggingMode>(), It.IsAny<CancellationToken>(), false, false), Times.Never);
    }

    [Fact]
    public async Task FailedDurableCleanupKeepsRecoveryStateAndThenRetriesSafely()
    {
        using var manager = Create(); await manager.InitializeAsync(); now = now.AddHours(2);
        store.FailClear = true; timer.Pulse(); Assert.NotNull(manager.Current); Assert.Contains("settings unavailable", manager.Current.LastError);
        store.FailClear = false;
        service.Setup(s => s.GetTraceLoggingAsync(It.IsAny<DataverseEnvironment>(), It.IsAny<CancellationToken>(), false, false))
            .ReturnsAsync(new TraceLoggingConfiguration(organizationId, TraceLoggingMode.Off));
        now = now.AddSeconds(30); timer.Pulse(); Assert.Null(manager.Current);
        service.Verify(s => s.SetTraceLoggingAsync(environment, organizationId, TraceLoggingMode.Off, It.IsAny<CancellationToken>(), false, false), Times.Once);
    }

    [Fact]
    public async Task DisposalStopsPulsesWithoutClearingThePersistedLease()
    {
        var manager = Create(); await manager.InitializeAsync(); manager.Dispose(); now = now.AddHours(2); timer.Pulse();
        Assert.NotNull(store.Value); Assert.True(timer.Disposed);
        service.Verify(s => s.GetTraceLoggingAsync(It.IsAny<DataverseEnvironment>(), It.IsAny<CancellationToken>(), false, false), Times.Never);
    }

    [Fact]
    public void OlderLeaseJsonLoadsWithoutRetryFields()
    {
        var json = JsonSerializer.Serialize(new { store.Value.EnvironmentUrl, store.Value.OrganizationId, store.Value.RestoreMode,
            store.Value.ExpiresAtUtc, store.Value.CleanupAtUtc, store.Value.LastAttemptAtUtc });
        var lease = JsonSerializer.Deserialize<TraceLoggingLease>(json);
        Assert.Equal(0, lease.FailedAttempts); Assert.Null(lease.RetryAtUtc); Assert.Null(lease.LastError);
    }

    private sealed class MemoryStore : ITraceLoggingLeaseStore
    {
        public TraceLoggingLease Value;
        public bool FailClear;
        public Task<TraceLoggingLease> LoadAsync() => Task.FromResult(Value);
        public Task SaveAsync(TraceLoggingLease lease)
        {
            if (FailClear && lease == null) throw new InvalidOperationException("settings unavailable");
            Value = lease; return Task.CompletedTask;
        }
    }
    private sealed class ImmediateDispatcher : ITraceDispatcher { public void Post(Action action) => action(); }
    private sealed class ManualTimer : ITraceLoggingLeaseTimer
    {
        public event Action Tick;
        public bool Enabled, Disposed;
        public void SetEnabled(bool enabled) => Enabled = enabled;
        public void Pulse() { if (Enabled && !Disposed) Tick?.Invoke(); }
        public void Dispose() { Disposed = true; Enabled = false; }
    }
}
