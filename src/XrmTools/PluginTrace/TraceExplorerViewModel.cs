#nullable enable
namespace XrmTools.PluginTrace;

using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.Environments;
using XrmTools.UI;

internal sealed class TraceExplorerViewModel : ViewModelBase, IDisposable
{
    private readonly ITraceExplorerService service;
    private readonly ITraceViewStore store;
    private readonly ITraceNavigator navigator;
    private readonly ITraceDispatcher dispatcher;
    private readonly Func<DateTimeOffset> now;
    private DataverseEnvironment? environment;
    private TraceFilter? applied, selectedView;
    private TraceQueryResult? latest;
    private IReadOnlyList<TraceRecord> records = Array.Empty<TraceRecord>();
    private IReadOnlyList<TraceFilter> savedViews = Array.Empty<TraceFilter>();
    private IReadOnlyList<string> suggestions = Array.Empty<string>();
    private TraceRecord? selectedRecord;
    private CancellationTokenSource? queryCancellation, navigationCancellation;
    private int revision, sessionRevision, intervalSeconds = 10;
    private bool active, disposed;
    private string status = "Ready", emptyMessage = "Apply a filter to load plugin traces.";
    private Investigation? back;

    public TraceExplorerViewModel(ITraceExplorerService service, ITraceViewStore store, ITraceNavigator navigator,
        ITraceDispatcher dispatcher, Func<DateTimeOffset>? now = null)
    {
        this.service = service; this.store = store; this.navigator = navigator; this.dispatcher = dispatcher;
        this.now = now ?? (() => DateTimeOffset.UtcNow);
        Filter = new TraceFilterViewModel(this.now); Detail = new TraceDetailViewModel(service); Logging = new TraceLoggingViewModel(service, this.now);
        Filter.Changed += FilterChanged; Logging.StatusChanged += SetStatus;
        RunCommand = new AsyncRelayCommand(RunAsync, AsyncRelayCommandOptions.AllowConcurrentExecutions);
        SelectViewCommand = new AsyncRelayCommand<TraceFilter>(SelectViewAsync, AsyncRelayCommandOptions.AllowConcurrentExecutions);
        SaveViewCommand = new AsyncRelayCommand(SaveViewAsync);
        DeleteViewCommand = new AsyncRelayCommand(DeleteViewAsync, () => SelectedView != null);
        RelatedCommand = new AsyncRelayCommand(ShowRelatedAsync, () => Detail.CanShowRelated && applied != null && !IsQuerying);
        BackCommand = new RelayCommand(GoBack, () => back != null);
        CloseDetailCommand = new RelayCommand(CloseDetail);
        UpdateDetailCommand = new RelayCommand(UpdateDetail, () => Detail.CanUpdate);
        NavigateCommand = new AsyncRelayCommand<TraceRecord>(NavigateAsync);
        ClearFiltersCommand = new AsyncRelayCommand(ClearFiltersAsync);
        Last24HoursCommand = new AsyncRelayCommand(Last24HoursAsync);
        Detail.PropertyChanged += DetailChanged;
    }

    public TraceFilterViewModel Filter { get; }
    public TraceDetailViewModel Detail { get; }
    public TraceLoggingViewModel Logging { get; }
    public IAsyncRelayCommand RunCommand { get; }
    public IAsyncRelayCommand<TraceFilter> SelectViewCommand { get; }
    public IAsyncRelayCommand SaveViewCommand { get; }
    public IAsyncRelayCommand DeleteViewCommand { get; }
    public IAsyncRelayCommand RelatedCommand { get; }
    public IRelayCommand BackCommand { get; }
    public IRelayCommand CloseDetailCommand { get; }
    public IRelayCommand UpdateDetailCommand { get; }
    public IAsyncRelayCommand<TraceRecord> NavigateCommand { get; }
    public IAsyncRelayCommand ClearFiltersCommand { get; }
    public IAsyncRelayCommand Last24HoursCommand { get; }
    public IReadOnlyList<TraceRecord> Records => records;
    public IReadOnlyList<TraceFilter> SavedViews => savedViews;
    public IReadOnlyList<string> TypeSuggestions => suggestions;
    public bool IsEmpty => records.Count == 0;
    public string EmptyMessage { get => emptyMessage; private set => SetProperty(ref emptyMessage, value); }
    public string EnvironmentLabel => environment == null ? "Plugin Trace Explorer" : (environment.Name ?? "Dataverse") + " · " + environment.Url;
    public string Status { get => status; private set => SetProperty(ref status, value); }
    public bool IsQuerying => queryCancellation != null;
    public bool HasUnappliedChanges => applied == null || Filter.Snapshot() != applied;
    public string QueryLabel => IsQuerying ? "Cancel" : HasUnappliedChanges ? "Apply" : "Refresh";
    public string QueryToolTip => IsQuerying ? "Cancel request" : HasUnappliedChanges ? "Apply filter (Ctrl+Enter)" : "Refresh the applied filter (Ctrl+Enter)";
    public bool CanGoBack => back != null;
    public string SavedViewLabel => selectedView?.Name ?? "Saved views";
    public string ModifiedLabel => selectedView != null && Filter.Snapshot() with { Name = selectedView.Name } != selectedView ? "Modified" : "";
    public int IntervalSeconds { get => intervalSeconds; set => SetProperty(ref intervalSeconds, value); }
    public TraceFilter? SelectedView
    {
        get => selectedView;
        set { if (SetProperty(ref selectedView, value)) { NotifySavedView(); if (value != null) SelectViewCommand.Execute(value); } }
    }
    public TraceRecord? SelectedRecord
    {
        get => selectedRecord;
        set
        {
            if (!SetProperty(ref selectedRecord, value)) return;
            if (value == null) Detail.Close();
            else _ = OpenDetailAsync(value);
        }
    }
    // The view uses these notifications only to preserve its viewport across a results replacement.
    public event Action<bool>? RecordsChanging;
    public event Action<bool>? RecordsChanged;
    public event Action? InvestigationRestored;

    public async Task ActivateAsync()
    {
        if (active || disposed) return;
        active = true; var session = ++sessionRevision;
        service.EnvironmentChanged += EnvironmentChanged;
        service.TraceLoggingConfigurationChanged += ConfigurationChanged;
        service.TraceLoggingLeaseChanged += LeaseChanged;
        try
        {
            await service.InitializeTraceLoggingLeasesAsync();
            var views = await store.LoadAsync();
            if (!IsSession(session)) return;
            savedViews = views.OrderBy(v => v.Name).ToArray(); OnPropertyChanged(nameof(SavedViews));
        }
        catch (Exception ex) { if (IsSession(session)) Status = "Could not initialize explorer: " + ex.Message; }
        if (!IsSession(session)) return;
        try
        {
            var environmentRevision = revision;
            var selected = await service.GetEnvironmentAsync();
            if (!IsSession(session) || revision != environmentRevision) return;
            if (selected?.IsValid == true)
            {
                if (environment?.ConnectionString != selected.ConnectionString) ResetEnvironment(selected);
                environmentRevision = revision;
                await Logging.LoadAsync(selected);
            }
            if (IsSession(session) && revision == environmentRevision && applied == null) await ApplyAsync();
            if (IsSession(session) && Detail.RequiresReload && selectedRecord != null) await OpenDetailAsync(selectedRecord);
        }
        catch (Exception ex) { if (IsSession(session)) Status = "Could not load explorer: " + ex.Message; }
    }

    public void Deactivate()
    {
        if (!active) return;
        active = false; sessionRevision++;
        service.EnvironmentChanged -= EnvironmentChanged;
        service.TraceLoggingConfigurationChanged -= ConfigurationChanged;
        service.TraceLoggingLeaseChanged -= LeaseChanged;
        CancelRequests();
    }
    private bool IsSession(int session) => active && !disposed && sessionRevision == session;
    private void EnvironmentChanged(DataverseEnvironment target) => PostForSession(() =>
    {
        ResetEnvironment(target);
        if (target.IsValid) _ = Logging.LoadAsync(target);
    });
    private void ConfigurationChanged(DataverseEnvironment target, TraceLoggingConfiguration configuration) =>
        PostForSession(() => Logging.Confirm(target, configuration));
    private void LeaseChanged(TraceLoggingLease? lease) => PostForSession(Logging.Tick);
    private void PostForSession(Action action)
    {
        var session = sessionRevision;
        dispatcher.Post(() => { if (IsSession(session)) action(); });
    }
    private void ResetEnvironment(DataverseEnvironment target)
    {
        CancelRequests(); environment = target with { }; applied = null; latest = null; back = null;
        Logging.SetEnvironment(environment); CloseDetail(); SetRecords(Array.Empty<TraceRecord>(), false);
        suggestions = Array.Empty<string>(); OnPropertyChanged(nameof(TypeSuggestions));
        EmptyMessage = "Environment changed. Apply a filter to load its traces.";
        Status = "Environment changed. Apply to load traces.";
        OnPropertyChanged(nameof(EnvironmentLabel)); NotifyQuery(); NotifyBack();
    }

    public async Task TickAsync(bool visible)
    {
        if (!active || disposed) return;
        Logging.Tick();
        if (visible && Logging.CanPoll && applied != null && !IsQuerying) await QueryAsync(applied, false, false);
    }
    public async Task RunAsync()
    {
        if (!active || disposed) return;
        if (IsQuerying) { CancelQuery(); Status = "Request cancelled. Existing results preserved."; return; }
        var session = sessionRevision; var generation = revision;
        try
        {
            await Logging.ExtendAsync();
            if (!IsSession(session) || generation != revision) return;
            if (HasUnappliedChanges) await ApplyAsync();
            else if (applied != null) await QueryAsync(applied, true, false);
        }
        catch (Exception ex) { if (IsSession(session) && generation == revision) Status = "Could not refresh traces: " + ex.Message; }
    }
    public async Task ApplyAsync()
    {
        try { var filter = Filter.Snapshot(); filter.Build(now()); await QueryAsync(filter, true, true); }
        catch (Exception ex) { if (active && !disposed) Status = ex.Message; }
    }
    private async Task QueryAsync(TraceFilter filter, bool interactive, bool replace)
    {
        if (!active || disposed) return;
        if (IsQuerying) { if (!replace) return; queryCancellation!.Cancel(); }
        var cancellation = new CancellationTokenSource(); queryCancellation = cancellation; var generation = ++revision;
        NotifyQuery(); if (interactive) Status = "Loading traces…";
        try
        {
            var selected = await service.GetEnvironmentAsync();
            cancellation.Token.ThrowIfCancellationRequested();
            if (selected?.IsValid != true) throw new InvalidOperationException("Select a Dataverse environment first.");
            var captured = selected with { };
            if (environment != null && captured.ConnectionString != environment.ConnectionString) { ResetEnvironment(captured); return; }
            var result = await service.QueryAsync(captured, filter, interactive, cancellation.Token);
            cancellation.Token.ThrowIfCancellationRequested();
            if (generation != revision || !active) return;
            var current = await service.GetEnvironmentAsync();
            cancellation.Token.ThrowIfCancellationRequested();
            if (generation != revision || !active) return;
            if (current?.ConnectionString != captured.ConnectionString) throw new InvalidOperationException("Environment changed. Apply the filter again.");
            environment = captured; latest = result;
            if (replace) { applied = filter with { }; back = null; CloseDetail(); NotifyBack(); }
            SetRecords(replace ? result.Records : TraceSnapshots.PreserveSelection(result.Records, selectedRecord), !replace);
            Detail.UpdateNotice(latest.Records);
            suggestions = records.Concat(result.Records).Select(r => r.TypeName).Distinct().OrderBy(n => n).ToArray();
            OnPropertyChanged(nameof(TypeSuggestions)); OnPropertyChanged(nameof(EnvironmentLabel));
            EmptyMessage = "No traces match the current filters.";
            Status = $"{records.Count} displayed · Last checked {now().ToLocalTime():HH:mm:ss}"
                + (result.Truncated ? $" · Showing the newest {TraceExplorerService.MaximumRecords} matches; narrow the filter to see older traces." : "");
            if (replace) await Logging.LoadAsync(captured);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { if (generation == revision && active) Status = "Could not load traces: " + ex.Message + " Existing results preserved."; }
        finally
        {
            if (ReferenceEquals(queryCancellation, cancellation)) { queryCancellation = null; NotifyQuery(); }
            cancellation.Dispose();
        }
    }
    private void SetRecords(IReadOnlyList<TraceRecord> values, bool preserve)
    {
        RecordsChanging?.Invoke(preserve); records = values;
        OnPropertyChanged(nameof(Records)); OnPropertyChanged(nameof(IsEmpty)); RecordsChanged?.Invoke(preserve);
    }
    private async Task OpenDetailAsync(TraceRecord record)
    {
        var pending = Detail.OpenAsync(environment, record);
        Detail.UpdateNotice(latest?.Records);
        await pending;
    }
    public void CloseDetail() { SelectedRecord = null; Detail.Close(); }
    private void UpdateDetail()
    {
        var updated = Detail.UpdatedRecord;
        if (updated == null || latest == null) return;
        CloseDetail(); SetRecords(latest.Records, true); SelectedRecord = updated;
    }
    private async Task SelectViewAsync(TraceFilter? filter)
    {
        if (filter == null) return;
        Filter.Set(filter); await ApplyAsync();
    }
    private async Task SaveViewAsync()
    {
        var session = sessionRevision;
        try
        {
            var filter = Filter.Snapshot(); filter.Build(now()); var previous = selectedView;
            if (string.IsNullOrWhiteSpace(filter.Name)) throw new FormatException("Enter a view name first.");
            if (savedViews.Any(v => !ReferenceEquals(v, previous) && string.Equals(v.Name, filter.Name, StringComparison.OrdinalIgnoreCase)))
                throw new FormatException("A saved view already has that name. Select it to overwrite it.");
            var values = savedViews.Where(v => !ReferenceEquals(v, previous)).Concat(new[] { filter }).OrderBy(v => v.Name).ToArray();
            await store.SaveAsync(values);
            if (!IsSession(session)) return;
            savedViews = values; selectedView = filter; Filter.Name = filter.Name;
            OnPropertyChanged(nameof(SavedViews)); OnPropertyChanged(nameof(SelectedView)); NotifySavedView();
            Status = "Saved view: " + filter.Name;
        }
        catch (Exception ex) { if (IsSession(session)) Status = "Could not save view: " + ex.Message; }
    }
    private async Task DeleteViewAsync()
    {
        if (selectedView == null) return;
        var previous = selectedView; var session = sessionRevision;
        try
        {
            var values = savedViews.Where(v => !ReferenceEquals(v, previous)).ToArray(); await store.SaveAsync(values);
            if (!IsSession(session)) return;
            savedViews = values; selectedView = null;
            OnPropertyChanged(nameof(SavedViews)); OnPropertyChanged(nameof(SelectedView)); NotifySavedView();
            Status = "Deleted saved view: " + previous.Name;
        }
        catch (Exception ex) { if (IsSession(session)) Status = "Could not delete view: " + ex.Message; }
    }
    private async Task ClearFiltersAsync()
    {
        Filter.Set(Filter.Snapshot() with { Name = "", TypeName = "", ErrorsOnly = false, FullControl = false, Expression = "" });
        SelectedView = null; await ApplyAsync();
    }
    private async Task Last24HoursAsync() { Filter.Minutes = 1440; Filter.FullControl = false; await ApplyAsync(); }
    private async Task ShowRelatedAsync()
    {
        if (selectedRecord?.CorrelationId == null || applied == null) return;
        var previous = back ?? new Investigation(applied, Filter.Snapshot(), records, selectedRecord, Detail.Raw, Detail.RequiresReload);
        var related = new TraceFilter { FullControl = true, Expression = $"correlationid eq {selectedRecord.CorrelationId:D}" };
        await QueryAsync(related, true, true);
        if (applied != related) return;
        back = previous; Filter.Set(related); NotifyBack();
    }
    private void GoBack()
    {
        if (back == null) return;
        var previous = back; CancelRequests(includeLogging: false); back = null; applied = previous.Filter; Filter.Set(previous.Draft);
        latest = new TraceQueryResult(previous.Records, false); CloseDetail(); SetRecords(previous.Records, false);
        // Restore the snapshot directly so returning does not start another detail request.
        selectedRecord = previous.Selected; OnPropertyChanged(nameof(SelectedRecord)); Detail.Restore(previous.Selected, previous.Raw); Detail.UpdateNotice(latest.Records);
        if (previous.DetailRequiresReload) _ = OpenDetailAsync(previous.Selected);
        NotifyBack(); NotifyQuery(); InvestigationRestored?.Invoke(); Status = "Returned to previous results.";
    }
    private async Task NavigateAsync(TraceRecord? record)
    {
        if (record == null || !active || disposed) return;
        navigationCancellation?.Cancel(); var cancellation = new CancellationTokenSource(); navigationCancellation = cancellation;
        Status = "Finding type definition in the current solution…";
        try { var result = await navigator.NavigateAsync(record.TypeName, cancellation.Token); if (!cancellation.IsCancellationRequested && active) Status = result; }
        catch (OperationCanceledException) { }
        catch (Exception ex) { if (!cancellation.IsCancellationRequested && active) Status = "Could not navigate to the type: " + ex.Message; }
        finally { if (ReferenceEquals(navigationCancellation, cancellation)) navigationCancellation = null; cancellation.Dispose(); }
    }
    private void CancelQuery() { revision++; queryCancellation?.Cancel(); queryCancellation = null; NotifyQuery(); }
    private void CancelRequests(bool includeLogging = true)
    {
        CancelQuery(); navigationCancellation?.Cancel(); navigationCancellation = null; Detail.Cancel();
        if (includeLogging) Logging.Cancel();
    }
    private void FilterChanged() { NotifyQuery(); OnPropertyChanged(nameof(ModifiedLabel)); }
    private void DetailChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs args)
    { RelatedCommand.NotifyCanExecuteChanged(); UpdateDetailCommand.NotifyCanExecuteChanged(); }
    private void NotifyQuery()
    {
        foreach (var property in new[] { nameof(IsQuerying), nameof(HasUnappliedChanges), nameof(QueryLabel), nameof(QueryToolTip) }) OnPropertyChanged(property);
        RelatedCommand.NotifyCanExecuteChanged();
    }
    private void NotifyBack() { OnPropertyChanged(nameof(CanGoBack)); BackCommand.NotifyCanExecuteChanged(); }
    private void NotifySavedView()
    { OnPropertyChanged(nameof(SavedViewLabel)); OnPropertyChanged(nameof(ModifiedLabel)); DeleteViewCommand.NotifyCanExecuteChanged(); }
    private void SetStatus(string value) { if (active && !disposed) Status = value; }
    public void Dispose()
    {
        if (disposed) return; Deactivate(); disposed = true;
        Filter.Changed -= FilterChanged; Logging.StatusChanged -= SetStatus; Detail.PropertyChanged -= DetailChanged;
        Detail.Dispose(); Logging.Dispose();
    }
    private sealed record Investigation(TraceFilter Filter, TraceFilter Draft, IReadOnlyList<TraceRecord> Records, TraceRecord Selected, string Raw, bool DetailRequiresReload);
}
