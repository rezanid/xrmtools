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

public sealed class TraceExplorerViewModelTests
{
    private readonly Mock<ITraceExplorerService> service = new();
    private readonly Mock<ITraceViewStore> store = new();
    private readonly Mock<ITraceNavigator> navigator = new();
    private readonly DataverseEnvironment environment = new() { ConnectionString = "Url=https://contoso.crm.dynamics.com;TenantId=test", Name = "Dev" };
    private readonly Guid organizationId = Guid.NewGuid();
    private readonly DateTimeOffset now = new(2026, 10, 2, 12, 0, 0, TimeSpan.Zero);

    public TraceExplorerViewModelTests()
    {
        service.Setup(s => s.InitializeTraceLoggingLeasesAsync()).Returns(Task.CompletedTask);
        service.Setup(s => s.GetEnvironmentAsync()).ReturnsAsync(environment);
        service.Setup(s => s.GetTraceLoggingAsync(It.IsAny<DataverseEnvironment>(), It.IsAny<CancellationToken>(), true, true))
            .ReturnsAsync(new TraceLoggingConfiguration(organizationId, TraceLoggingMode.Off));
        service.Setup(s => s.QueryAsync(It.IsAny<DataverseEnvironment>(), It.IsAny<TraceFilter>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TraceQueryResult(Array.Empty<TraceRecord>(), false));
        service.Setup(s => s.DetailAsync(It.IsAny<DataverseEnvironment>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync("full record");
        service.Setup(s => s.ExtendTimedTraceLoggingAsync(It.IsAny<DataverseEnvironment>(), It.IsAny<Guid>())).Returns(Task.CompletedTask);
        service.Setup(s => s.CancelTimedTraceLoggingAsync(It.IsAny<DataverseEnvironment>(), It.IsAny<Guid>())).Returns(Task.CompletedTask);
        service.Setup(s => s.SetTraceLoggingAsync(It.IsAny<DataverseEnvironment>(), It.IsAny<Guid>(), It.IsAny<TraceLoggingMode>(), It.IsAny<CancellationToken>(), true, true)).Returns(Task.CompletedTask);
        store.Setup(s => s.LoadAsync()).ReturnsAsync(Array.Empty<TraceFilter>());
        store.Setup(s => s.SaveAsync(It.IsAny<IReadOnlyList<TraceFilter>>())).Returns(Task.CompletedTask);
    }
    private TraceExplorerViewModel Create(ITraceDispatcher dispatcher = null) => new(service.Object, store.Object, navigator.Object, dispatcher ?? new ImmediateDispatcher(), () => now);

    [Fact]
    public async Task BackRestartsADetailLoadThatWasInterruptedByRelatedExecution()
    {
        var original = Record("selected"); var pending = new TaskCompletionSource<string>();
        using var model = Create(); await model.ActivateAsync();
        service.SetupSequence(s => s.DetailAsync(environment, original.Id, It.IsAny<CancellationToken>()))
            .Returns(pending.Task).ReturnsAsync("completed after returning");
        model.SelectedRecord = original; await model.RelatedCommand.ExecuteAsync(null); model.BackCommand.Execute(null);
        Assert.Equal("completed after returning", model.Detail.Raw); Assert.False(model.Detail.RequiresReload);
        pending.SetResult("stale full record");
    }

    [Fact]
    public async Task RapidSavedViewChangesApplyTheLatestChoice()
    {
        using var model = Create(); await model.ActivateAsync();
        var pending = new TaskCompletionSource<TraceQueryResult>();
        service.SetupSequence(s => s.QueryAsync(environment, It.IsAny<TraceFilter>(), true, It.IsAny<CancellationToken>()))
            .Returns(pending.Task).ReturnsAsync(new TraceQueryResult(new[] { Record("latest") }, false));
        var first = new TraceFilter { Name = "First", TypeName = "First" };
        var second = new TraceFilter { Name = "Second", TypeName = "Second" };
        var firstSelection = model.SelectViewCommand.ExecuteAsync(first);
        await model.SelectViewCommand.ExecuteAsync(second);
        pending.SetResult(new TraceQueryResult(new[] { Record("late first") }, false)); await firstSelection;
        Assert.Equal("Second", model.Filter.TypeName); Assert.Equal("latest", Assert.Single(model.Records).Message);
    }

    [Fact]
    public async Task ReopeningRestartsAnInterruptedDetailLoad()
    {
        using var model = Create(); await model.ActivateAsync();
        var original = Record("selected"); var pending = new TaskCompletionSource<string>();
        service.SetupSequence(s => s.DetailAsync(environment, original.Id, It.IsAny<CancellationToken>()))
            .Returns(pending.Task).ReturnsAsync("reloaded full record");
        model.SelectedRecord = original; model.Deactivate(); Assert.True(model.Detail.RequiresReload);
        await model.ActivateAsync(); pending.SetResult("stale full record");
        Assert.Equal("reloaded full record", model.Detail.Raw); Assert.False(model.Detail.RequiresReload);
    }

    [Fact]
    public async Task QueuedNotificationsFromAnOldActivationAreIgnoredAfterReopening()
    {
        var dispatcher = new QueuedDispatcher(); using var model = Create(dispatcher); await model.ActivateAsync();
        service.Raise(s => s.TraceLoggingConfigurationChanged += null, environment, new TraceLoggingConfiguration(organizationId, TraceLoggingMode.All));
        model.Deactivate(); await model.ActivateAsync(); dispatcher.Flush();
        Assert.Equal(TraceLoggingMode.Off, model.Logging.SelectedOption.Mode);
    }

    [Fact]
    public async Task LateActivationCannotUndoAnEnvironmentChange()
    {
        var pending = new TaskCompletionSource<DataverseEnvironment>();
        service.Setup(s => s.GetEnvironmentAsync()).Returns(pending.Task);
        using var model = Create(); var activation = model.ActivateAsync();
        var other = environment with { ConnectionString = "Url=https://other.crm.dynamics.com;TenantId=test", Name = "Other" };
        service.Raise(s => s.EnvironmentChanged += null, other);
        pending.SetResult(environment); await activation;
        Assert.Contains("Other", model.EnvironmentLabel); Assert.Empty(model.Records);
        service.Verify(s => s.QueryAsync(It.IsAny<DataverseEnvironment>(), It.IsAny<TraceFilter>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task LateDetailResponseCannotReplaceANewerSelection()
    {
        var first = Record("first"); var second = Record("second");
        var pending = new TaskCompletionSource<string>();
        service.Setup(s => s.DetailAsync(environment, first.Id, It.IsAny<CancellationToken>())).Returns(pending.Task);
        using var detail = new TraceDetailViewModel(service.Object);
        var firstLoad = detail.OpenAsync(environment, first); await detail.OpenAsync(environment, second);
        pending.SetResult("stale first record"); await firstLoad;
        Assert.Same(second, detail.Record); Assert.Equal("full record", detail.Raw);
    }

    [Fact]
    public async Task OffPausesPollingButManualRefreshStillWorks()
    {
        using var model = Create(); await model.ActivateAsync();
        Assert.Equal("Refresh", model.QueryLabel);
        await model.TickAsync(true);
        service.Verify(s => s.QueryAsync(It.IsAny<DataverseEnvironment>(), It.IsAny<TraceFilter>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
        await model.RunAsync();
        service.Verify(s => s.QueryAsync(It.IsAny<DataverseEnvironment>(), It.IsAny<TraceFilter>(), true, It.IsAny<CancellationToken>()), Times.Exactly(2));
        Assert.True(model.Logging.AutoRefresh);
    }

    [Fact]
    public async Task PollingIsNonInteractiveAndNeverExtendsLogging()
    {
        using var model = Create(); await model.ActivateAsync();
        service.Raise(s => s.TraceLoggingConfigurationChanged += null, environment, new TraceLoggingConfiguration(organizationId, TraceLoggingMode.Exceptions));
        await model.TickAsync(false); await model.TickAsync(true);
        service.Verify(s => s.QueryAsync(It.IsAny<DataverseEnvironment>(), It.IsAny<TraceFilter>(), false, It.IsAny<CancellationToken>()), Times.Once);
        service.Verify(s => s.ExtendTimedTraceLoggingAsync(It.IsAny<DataverseEnvironment>(), It.IsAny<Guid>()), Times.Never);
        await model.RunAsync();
        service.Verify(s => s.ExtendTimedTraceLoggingAsync(environment, organizationId), Times.Once);
    }

    [Fact]
    public async Task EnvironmentSwitchDiscardsLateResultsEvenWhenTransportIgnoresCancellation()
    {
        using var model = Create(); await model.ActivateAsync();
        var pending = new TaskCompletionSource<TraceQueryResult>();
        service.Setup(s => s.QueryAsync(It.IsAny<DataverseEnvironment>(), It.IsAny<TraceFilter>(), true, It.IsAny<CancellationToken>())).Returns(pending.Task);
        model.Filter.TypeName = "draft";
        var query = model.ApplyAsync(); Assert.True(model.IsQuerying);
        var other = environment with { ConnectionString = "Url=https://other.crm.dynamics.com;TenantId=test", Name = "Other" };
        service.Raise(s => s.EnvironmentChanged += null, other);
        pending.SetResult(new TraceQueryResult(new[] { Record("old environment") }, false));
        await query;
        Assert.Empty(model.Records); Assert.False(model.IsQuerying); Assert.Contains("Other", model.EnvironmentLabel);
        Assert.Equal("draft", model.Filter.TypeName); Assert.Equal("Apply", model.QueryLabel);
    }

    [Fact]
    public async Task CancelKeepsPreviousResultsAndLateResponseCannotReplaceThem()
    {
        var original = Record("original");
        service.Setup(s => s.QueryAsync(It.IsAny<DataverseEnvironment>(), It.IsAny<TraceFilter>(), true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TraceQueryResult(new[] { original }, false));
        using var model = Create(); await model.ActivateAsync();
        var pending = new TaskCompletionSource<TraceQueryResult>();
        service.Setup(s => s.QueryAsync(It.IsAny<DataverseEnvironment>(), It.IsAny<TraceFilter>(), true, It.IsAny<CancellationToken>())).Returns(pending.Task);
        var query = model.RunAsync(); Assert.Equal("Cancel", model.QueryLabel);
        await model.RunAsync();
        pending.SetResult(new TraceQueryResult(new[] { Record("late") }, false)); await query;
        Assert.Same(original, Assert.Single(model.Records)); Assert.Equal("Refresh", model.QueryLabel);
    }

    [Fact]
    public async Task RefreshPreservesDetailSnapshotUntilTheUserLoadsItsUpdate()
    {
        var original = Record("original"); var updated = Record("updated", original.Id);
        service.Setup(s => s.QueryAsync(It.IsAny<DataverseEnvironment>(), It.IsAny<TraceFilter>(), true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TraceQueryResult(new[] { original }, false));
        using var model = Create(); await model.ActivateAsync(); model.SelectedRecord = original;
        service.Setup(s => s.QueryAsync(It.IsAny<DataverseEnvironment>(), It.IsAny<TraceFilter>(), true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TraceQueryResult(new[] { updated }, false));
        await model.RunAsync();
        Assert.Same(original, model.SelectedRecord); Assert.Equal("original", model.Detail.Message); Assert.True(model.Detail.CanUpdate);
        model.UpdateDetailCommand.Execute(null);
        Assert.Same(updated, model.SelectedRecord); Assert.Equal("updated", model.Detail.Message);
    }

    [Fact]
    public async Task RelatedExecutionAndBackRestoreTheDraftSelectionAndFullRecordWithoutReloadingDetails()
    {
        var original = Record("original");
        service.Setup(s => s.QueryAsync(It.IsAny<DataverseEnvironment>(), It.IsAny<TraceFilter>(), true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TraceQueryResult(new[] { original }, false));
        using var model = Create(); await model.ActivateAsync(); model.SelectedRecord = original; model.Filter.TypeName = "unsaved draft";
        await model.RelatedCommand.ExecuteAsync(null); Assert.True(model.CanGoBack); Assert.True(model.Filter.FullControl);
        model.BackCommand.Execute(null);
        Assert.Same(original, model.SelectedRecord); Assert.Equal("unsaved draft", model.Filter.TypeName); Assert.Equal("full record", model.Detail.Raw);
        Assert.True(model.Logging.IsAvailable); Assert.False(model.CanGoBack);
        service.Verify(s => s.DetailAsync(It.IsAny<DataverseEnvironment>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeactivationCancelsRequestsAndUnsubscribesFromEnvironmentEvents()
    {
        using var model = Create(); await model.ActivateAsync();
        var label = model.EnvironmentLabel; model.Deactivate();
        service.Raise(s => s.EnvironmentChanged += null, environment with { Name = "Other" });
        await model.TickAsync(true); Assert.Equal(label, model.EnvironmentLabel);
        service.Verify(s => s.QueryAsync(It.IsAny<DataverseEnvironment>(), It.IsAny<TraceFilter>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
        service.VerifyRemove(s => s.EnvironmentChanged -= It.IsAny<Action<DataverseEnvironment>>(), Times.Once);
    }

    [Fact]
    public async Task FailedSavePreservesTheExistingSavedViews()
    {
        var existing = new TraceFilter { Name = "Existing" }; store.Setup(s => s.LoadAsync()).ReturnsAsync(new[] { existing });
        using var model = Create(); await model.ActivateAsync(); model.Filter.Name = "New";
        store.Setup(s => s.SaveAsync(It.IsAny<IReadOnlyList<TraceFilter>>())).ThrowsAsync(new InvalidOperationException("disk unavailable"));
        await model.SaveViewCommand.ExecuteAsync(null);
        Assert.Same(existing, Assert.Single(model.SavedViews)); Assert.Contains("disk unavailable", model.Status);
    }

    [Fact]
    public async Task SuccessfulSaveAndDeleteUpdateTheBoundState()
    {
        using var model = Create(); await model.ActivateAsync(); model.Filter.Name = "Mine";
        await model.SaveViewCommand.ExecuteAsync(null);
        Assert.Equal("Mine", Assert.Single(model.SavedViews).Name); Assert.Equal("Mine", model.SavedViewLabel); Assert.Equal("", model.ModifiedLabel);
        model.Filter.TypeName = "Changed"; Assert.Equal("Modified", model.ModifiedLabel);
        await model.DeleteViewCommand.ExecuteAsync(null); Assert.Empty(model.SavedViews); Assert.Equal("Saved views", model.SavedViewLabel);
    }

    [Fact]
    public async Task ConfigurationNotificationsAreMarshaledAndExpiryUpdatesTheChoice()
    {
        var dispatcher = new QueuedDispatcher(); using var model = Create(dispatcher); await model.ActivateAsync();
        var lease = new TraceLoggingLease(environment.Url, organizationId, TraceLoggingMode.Off, now.AddHours(1), now.AddDays(7), null);
        service.SetupGet(s => s.CurrentTraceLoggingLease).Returns(lease);
        service.Raise(s => s.TraceLoggingConfigurationChanged += null, environment, new TraceLoggingConfiguration(organizationId, TraceLoggingMode.All));
        Assert.Equal(TraceLoggingMode.Off, model.Logging.SelectedOption.Mode); dispatcher.Flush(); Assert.True(model.Logging.SelectedOption.Timed);
        service.SetupGet(s => s.CurrentTraceLoggingLease).Returns((TraceLoggingLease)null);
        service.Raise(s => s.TraceLoggingConfigurationChanged += null, environment, new TraceLoggingConfiguration(organizationId, TraceLoggingMode.Off));
        service.Raise(s => s.TraceLoggingLeaseChanged += null, new object[] { null }); dispatcher.Flush();
        Assert.Equal(TraceLoggingMode.Off, model.Logging.SelectedOption.Mode); Assert.False(model.Logging.HasTimer); Assert.False(model.Logging.CanPoll);
    }

    [Fact]
    public async Task TimedSelectionPassesTheConfirmedPreviousModeAndFailureKeepsTheConfirmedChoice()
    {
        using var model = Create(); await model.ActivateAsync();
        service.Setup(s => s.StartTimedTraceLoggingAsync(environment, organizationId, TraceLoggingMode.Off, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("no permission"));
        await model.Logging.ChangeCommand.ExecuteAsync(TraceLoggingViewModel.Options[3]);
        Assert.Equal(TraceLoggingMode.Off, model.Logging.SelectedOption.Mode); Assert.True(model.Logging.IsAvailable); Assert.Contains("no permission", model.Status);
        service.Verify(s => s.StartTimedTraceLoggingAsync(environment, organizationId, TraceLoggingMode.Off, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void FilterValidationAndPreviewAreIndependentOfControls()
    {
        var filter = new TraceFilterViewModel(() => now) { Minutes = 0, FromText = "invalid", ToText = "invalid" };
        Assert.Contains("start and end time", filter.Preview);
        filter.FullControl = true; filter.Expression = "depth gt 2";
        Assert.Equal("depth gt 2", filter.Preview); Assert.False(filter.QuickFiltersEnabled); Assert.False(filter.ShowCustomRange);
    }

    private static TraceRecord Record(string message, Guid? id = null)
    {
        using var json = JsonDocument.Parse(JsonSerializer.Serialize(new
        {
            plugintracelogid = id ?? Guid.NewGuid(), createdon = "2026-10-02T12:00:00Z", messageblock = message,
            typename = "Example.Plugin", correlationid = Guid.Parse("00000001-0000-0000-0000-000000000000")
        }));
        return new TraceRecord(json.RootElement);
    }
    private sealed class ImmediateDispatcher : ITraceDispatcher { public void Post(Action action) => action(); }
    private sealed class QueuedDispatcher : ITraceDispatcher
    {
        private readonly Queue<Action> actions = new();
        public void Post(Action action) => actions.Enqueue(action);
        public void Flush() { while (actions.Count > 0) actions.Dequeue()(); }
    }
}
