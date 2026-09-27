#nullable enable
namespace XrmTools.PluginTrace;

using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

public partial class TraceExplorerControl : UserControl, IDisposable
{
    private readonly TraceExplorerService service;
    private readonly DispatcherTimer timer = new();
    private readonly List<TraceFilter> saved = new();
    private readonly List<SortDescription> sortDescriptions = [new(nameof(TraceRecord.CreatedOn), ListSortDirection.Descending)];
    private IReadOnlyList<TraceRecord> displayed = Array.Empty<TraceRecord>();
    private TraceQueryResult? latest;
    private TraceFilter? applied;
    private DataverseEnvironment? environment;
    private CancellationTokenSource? queryCancellation;
    private CancellationTokenSource? detailCancellation;
    private CancellationTokenSource? navigationCancellation;
    private CancellationTokenSource? traceLoggingLoadCancellation;
    private CancellationTokenSource? loggingCancellation;
    private Guid organizationId;
    private TraceLoggingMode traceLoggingMode;
    private bool initializing = true, changingSelection, changingTraceLogging, loaded, disposed;
    private int revision;
    private GridLength detailWidth = new(1, GridUnitType.Star);
    private Investigation? back;

    internal TraceExplorerControl(TraceExplorerService service)
    {
        this.service = service;
        InitializeComponent();
        FromTime.Text = DateTime.Now.AddHours(-1).ToString("yyyy-MM-dd HH:mm:ss");
        ToTime.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        foreach (var text in new[] { Expression, FromTime, ToTime }) text.TextChanged += FilterChanged;
        TypeName.AddHandler(TextBox.TextChangedEvent, new TextChangedEventHandler(FilterChanged));
        Interval.SelectionChanged += (_, _) => SetTimerInterval();
        timer.Tick += TimerTick;
        SetTimerInterval();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        KeyDown += OnKeyDown;
        UpdateSortIndicators();
        initializing = false;
        UpdateFilterPreview();
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (loaded || disposed) return;
        loaded = true;
        DataverseEnvironmentProvider.EnvironmentChanged += EnvironmentChanged;
        try
        {
            var options = await TraceExplorerOptions.GetLiveInstanceAsync();
            saved.Clear();
            saved.AddRange(JsonSerializer.Deserialize<List<TraceFilter>>(options.SavedViewsJson) ?? new());
            RefreshSavedViews();
        }
        catch (Exception ex) { Status.Text = "Could not load saved views: " + ex.Message; }
        if (!loaded || disposed) return;
        var selectedEnvironment = await service.GetEnvironmentAsync();
        if (selectedEnvironment?.IsValid == true)
        {
            environment = selectedEnvironment with { };
            EnvironmentLabel.Text = (selectedEnvironment.Name ?? "Dataverse") + " · " + selectedEnvironment.Url;
            await LoadTraceLoggingAsync(selectedEnvironment);
        }
        timer.Start();
        if (applied == null) await ApplyAsync();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        loaded = false;
        timer.Stop();
        DataverseEnvironmentProvider.EnvironmentChanged -= EnvironmentChanged;
        CancelRequests();
    }

    private void SetTimerInterval() => timer.Interval = TimeSpan.FromSeconds(int.Parse((string)((ComboBoxItem)Interval.SelectedItem).Tag, CultureInfo.InvariantCulture));

    private async void TimerTick(object? sender, EventArgs e)
    {
        if (AutoRefresh.IsChecked == true && applied != null && queryCancellation == null && IsVisible)
            await QueryAsync(applied, false, false);
    }

    private void EnvironmentChanged(DataverseEnvironment value)
    {
        // The provider may raise this event off the UI thread.
        _ = Dispatcher.BeginInvoke(new Action(() =>
        {
            if (!loaded || disposed) return;
            CancelRequests();
            applied = null;
            environment = value with { };
            organizationId = Guid.Empty;
            TraceLogging.IsEnabled = false;
            SetTraceLogging(TraceLoggingMode.Off);
            latest = null;
            back = null;
            displayed = Array.Empty<TraceRecord>();
            Logs.ItemsSource = displayed;
            CloseDetails();
            UpdatesButton.Visibility = BackButton.Visibility = Visibility.Collapsed;
            EmptyState.Visibility = Visibility.Visible;
            EmptyState.Text = "Environment changed. Apply a filter to load its traces.";
            EnvironmentLabel.Text = value.Name + " · " + value.Url;
            Status.Text = "Environment changed. Apply to load traces.";
            if (value.IsValid) _ = LoadTraceLoggingAsync(value);
        }));
    }

    private TraceFilter ReadFilter() => new()
    {
        Name = ViewName.Text.Trim(),
        Minutes = int.Parse((string)((ComboBoxItem)Duration.SelectedItem).Tag, CultureInfo.InvariantCulture),
        From = DateTimeOffset.TryParse(FromTime.Text, CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out var from) ? from : null,
        To = DateTimeOffset.TryParse(ToTime.Text, CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out var to) ? to : null,
        TypeName = TypeName.Text,
        ErrorsOnly = ErrorsOnly.IsChecked == true,
        FullControl = FullControl.IsChecked == true,
        Expression = Expression.Text
    };

    private void DurationChanged(object sender, SelectionChangedEventArgs e) => FilterChanged(sender, e);

    private void FilterChanged(object sender, RoutedEventArgs e)
    {
        if (!initializing) UpdateFilterPreview();
    }

    private void UpdateFilterPreview()
    {
        var filter = ReadFilter();
        QuickFilters.IsEnabled = !filter.FullControl;
        CustomRange.Visibility = filter.Minutes == 0 && !filter.FullControl ? Visibility.Visible : Visibility.Collapsed;
        ModifiedLabel.Text = SavedViews.SelectedItem is TraceFilter selected && filter with { Name = selected.Name } != selected ? "Modified" : "";
        try { Preview.Text = filter.Build(DateTimeOffset.UtcNow); }
        catch (FormatException ex) { Preview.Text = ex.Message; }
    }

    private async void ApplyClick(object sender, RoutedEventArgs e) => await ApplyAsync();
    private async Task ApplyAsync()
    {
        try
        {
            var filter = ReadFilter();
            filter.Build(DateTimeOffset.UtcNow);
            await QueryAsync(filter, true, true);
        }
        catch (Exception ex) { Status.Text = ex.Message; }
    }

    private async void RefreshOrCancelClick(object sender, RoutedEventArgs e)
    {
        if (queryCancellation != null)
        {
            CancelActiveRefresh();
            return;
        }
        if (applied == null) await ApplyAsync();
        else await QueryAsync(applied, true, false);
    }

    private void CancelActiveRefresh()
    {
        revision++;
        queryCancellation?.Cancel();
        queryCancellation = null;
        SetRefreshAction(false);
        Status.Text = "Request cancelled. Existing results preserved.";
    }

    private void CancelRequests()
    {
        navigationCancellation?.Cancel();
        revision++;
        queryCancellation?.Cancel();
        queryCancellation = null;
        detailCancellation?.Cancel();
        detailCancellation = null;
        traceLoggingLoadCancellation?.Cancel();
        traceLoggingLoadCancellation = null;
        loggingCancellation?.Cancel();
        loggingCancellation = null;
        SetRefreshAction(false);
    }

    private void SetRefreshAction(bool refreshing)
    {
        RefreshButton.Content = refreshing ? "Cancel" : "Refresh";
        RefreshButton.ToolTip = refreshing ? "Cancel refresh" : "Refresh traces";
    }

    private async Task LoadTraceLoggingAsync(DataverseEnvironment selectedEnvironment)
    {
        traceLoggingLoadCancellation?.Cancel();
        var cancellation = new CancellationTokenSource();
        traceLoggingLoadCancellation = cancellation;
        TraceLogging.IsEnabled = false;
        try
        {
            var configuration = await service.GetTraceLoggingAsync(selectedEnvironment, cancellation.Token);
            if (cancellation.IsCancellationRequested || disposed || !loaded || environment?.ConnectionString != selectedEnvironment.ConnectionString) return;
            organizationId = configuration.OrganizationId;
            SetTraceLogging(configuration.Mode);
            TraceLogging.IsEnabled = true;
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            if (!cancellation.IsCancellationRequested) Status.Text = "Could not read trace logging: " + ex.Message;
        }
        finally
        {
            if (ReferenceEquals(traceLoggingLoadCancellation, cancellation)) traceLoggingLoadCancellation = null;
            cancellation.Dispose();
        }
    }

    private void SetTraceLogging(TraceLoggingMode mode)
    {
        changingTraceLogging = true;
        traceLoggingMode = mode;
        TraceLogging.SelectedItem = TraceLogging.Items.Cast<ComboBoxItem>().First(i => (string)i.Tag == ((int)mode).ToString(CultureInfo.InvariantCulture));
        changingTraceLogging = false;
    }

    private async void TraceLoggingChanged(object sender, SelectionChangedEventArgs e)
    {
        if (initializing || changingTraceLogging || !loaded || environment == null || organizationId == Guid.Empty || TraceLogging.SelectedItem is not ComboBoxItem item) return;
        var requested = (TraceLoggingMode)int.Parse((string)item.Tag, CultureInfo.InvariantCulture);
        if (requested == traceLoggingMode) return;
        loggingCancellation?.Cancel();
        var cancellation = new CancellationTokenSource();
        loggingCancellation = cancellation;
        TraceLogging.IsEnabled = false;
        try
        {
            await service.SetTraceLoggingAsync(environment, organizationId, requested, cancellation.Token);
            if (!cancellation.IsCancellationRequested) { traceLoggingMode = requested; Status.Text = "Trace logging set to " + item.Content + "."; }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            if (!cancellation.IsCancellationRequested)
            {
                SetTraceLogging(traceLoggingMode);
                Status.Text = "Could not change trace logging: " + ex.Message;
            }
        }
        finally
        {
            if (ReferenceEquals(loggingCancellation, cancellation))
            {
                loggingCancellation = null;
                if (!disposed && loaded) TraceLogging.IsEnabled = true;
            }
            cancellation.Dispose();
        }
    }

    private async Task QueryAsync(TraceFilter filter, bool interactive, bool replace)
    {
        if (!loaded || disposed) return;
        if (queryCancellation != null)
        {
            if (!replace) return;
            queryCancellation.Cancel();
        }
        var cancellation = new CancellationTokenSource();
        queryCancellation = cancellation;
        int generation = ++revision;
        SetRefreshAction(true);
        if (interactive) Status.Text = "Loading traces…";
        try
        {
            var selectedEnvironment = await service.GetEnvironmentAsync();
            if (selectedEnvironment?.IsValid != true) throw new InvalidOperationException("Select a Dataverse environment first.");
            var captured = selectedEnvironment with { };
            // Never keep displaying one environment's rows under another environment's label.
            if (environment != null && captured.ConnectionString != environment.ConnectionString)
            {
                EnvironmentChanged(captured);
                return;
            }
            var result = await service.QueryAsync(captured, filter, interactive, cancellation.Token);
            cancellation.Token.ThrowIfCancellationRequested();
            if (generation != revision || !loaded) return;
            if ((await service.GetEnvironmentAsync())?.ConnectionString != captured.ConnectionString)
                throw new InvalidOperationException("Environment changed. Apply the filter again.");
            cancellation.Token.ThrowIfCancellationRequested();
            if (generation != revision || !loaded) return;
            environment = captured;
            EnvironmentLabel.Text = (captured.Name ?? "Dataverse") + " · " + captured.Url;
            latest = result;
            if (replace)
            {
                applied = filter with { };
                back = null;
                BackButton.Visibility = Visibility.Collapsed;
                CloseDetails();
                Display(result.Records, false);
                UpdatesButton.Visibility = Visibility.Collapsed;
            }
            else if (TraceSnapshots.Changed(displayed, result.Records))
            {
                var existingIds = new HashSet<Guid>(displayed.Select(r => r.Id));
                int added = result.Records.Count(r => !existingIds.Contains(r.Id));
                UpdatesButton.Content = added > 0 ? $"{added} new traces · Show updates" : "Results changed · Show updates";
                UpdatesButton.Visibility = Visibility.Visible;
                UpdateDetailNotice();
            }
            else UpdatesButton.Visibility = Visibility.Collapsed;
            if (!TypeName.IsKeyboardFocusWithin && !TypeName.IsDropDownOpen)
            {
                var suggestions = displayed.Concat(result.Records).Select(r => r.TypeName).Distinct().OrderBy(n => n).ToArray();
                if (!(TypeName.ItemsSource is string[] existing) || !existing.SequenceEqual(suggestions))
                {
                    var typeText = TypeName.Text;
                    TypeName.ItemsSource = suggestions;
                    TypeName.Text = typeText;
                }
            }
            Status.Text = $"{displayed.Count} displayed · Last checked {DateTime.Now:HH:mm:ss}"
                + (result.Truncated ? $" · Showing the newest {TraceExplorerService.MaximumRecords} matches; narrow the filter to see older traces." : "");
            if (replace) await LoadTraceLoggingAsync(captured);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            if (generation == revision) Status.Text = "Could not load traces: " + ex.Message + " Existing results preserved.";
        }
        finally
        {
            if (ReferenceEquals(queryCancellation, cancellation))
            {
                queryCancellation = null;
                SetRefreshAction(false);
            }
            cancellation.Dispose();
        }
    }

    private void Display(IReadOnlyList<TraceRecord> rows, bool preserve)
    {
        var selected = preserve ? Logs.SelectedItem as TraceRecord : null;
        var scroll = FindScroll(Logs);
        var offset = preserve ? scroll?.VerticalOffset ?? 0 : 0;
        var horizontal = preserve ? scroll?.HorizontalOffset ?? 0 : 0;
        // Take the anchor from the currently sorted view, then restore it in the new sorted view.
        // This keeps the viewport steady even when the user has sorted by a column other than Created.
        var anchor = preserve && offset < Logs.Items.Count ? (Logs.Items[(int)offset] as TraceRecord)?.Id : null;
        displayed = TraceSnapshots.PreserveSelection(rows, selected);
        var view = new ListCollectionView(displayed.ToList());
        foreach (var sort in sortDescriptions) view.SortDescriptions.Add(sort);
        UpdateSortIndicators();
        changingSelection = true;
        Logs.ItemsSource = view;
        Logs.SelectedItem = selected;
        changingSelection = false;
        EmptyState.Visibility = displayed.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        EmptyState.Text = "No matching traces. Try a longer time window or a broader filter. Tracing must be enabled in Dataverse.";
        if (anchor != null)
        {
            int index = Logs.Items.Cast<TraceRecord>().ToList().FindIndex(r => r.Id == anchor);
            if (index >= 0) offset = index;
        }
        Logs.UpdateLayout();
        scroll = FindScroll(Logs);
        scroll?.ScrollToVerticalOffset(offset);
        scroll?.ScrollToHorizontalOffset(horizontal);
    }

    private void LogsSorting(object sender, DataGridSortingEventArgs e)
    {
        e.Handled = true;
        var direction = e.Column.SortDirection == ListSortDirection.Ascending
            ? ListSortDirection.Descending
            : ListSortDirection.Ascending;
        sortDescriptions.Clear();
        sortDescriptions.Add(new SortDescription(e.Column.SortMemberPath, direction));
        // Apply the new view without changing the selected trace or its detail snapshot.
        Display(displayed, true);
    }

    private void UpdateSortIndicators()
    {
        foreach (var column in Logs.Columns)
        {
            var sort = sortDescriptions.FirstOrDefault(s => string.Equals(s.PropertyName, column.SortMemberPath, StringComparison.Ordinal));
            column.SortDirection = string.IsNullOrEmpty(sort.PropertyName) ? null : sort.Direction;
        }
    }

    private void ShowUpdatesClick(object sender, RoutedEventArgs e)
    {
        if (latest == null) return;
        Display(latest.Records, true);
        UpdatesButton.Visibility = Visibility.Collapsed;
        UpdateDetailNotice();
        Status.Text = $"{displayed.Count} displayed · Updates shown {DateTime.Now:HH:mm:ss}"
            + (latest.Truncated ? $" · Results capped at {TraceExplorerService.MaximumRecords}; narrow the filter to see older traces." : "");
    }

    private async void LogSelected(object sender, SelectionChangedEventArgs e)
    {
        if (changingSelection || Logs.SelectedItem is not TraceRecord record) return;
        Details.Visibility = DetailSplitter.Visibility = Visibility.Visible;
        SplitterColumn.Width = new GridLength(5);
        DetailColumn.Width = detailWidth;
        DetailTitle.Text = record.ShortTypeName;
        DetailTitle.ToolTip = record.TypeName;
        FindStatus.Text = "";
        DetailSummary.Text = $"{record.Time} · {record.MessageName} · {record.Entity}\n{record.Duration} ms · Depth {record.Depth} · {record.Status}";
        ExceptionText.Text = string.IsNullOrWhiteSpace(record.Exception) ? "No exception recorded." : record.Exception;
        MessageText.Text = string.IsNullOrEmpty(record.Message) ? "No trace message recorded." : record.Message;
        ExceptionTab.Visibility = string.IsNullOrWhiteSpace(record.Exception) ? Visibility.Collapsed : Visibility.Visible;
        DetailTabs.SelectedIndex = string.IsNullOrWhiteSpace(record.Exception) ? 1 : 0;
        RelatedButton.IsEnabled = record.CorrelationId.HasValue && record.CorrelationId != Guid.Empty;
        UpdateDetailNotice();
        detailCancellation?.Cancel();
        var cancellation = new CancellationTokenSource();
        detailCancellation = cancellation;
        RawText.Text = "Loading full record…";
        try
        {
            if (environment == null) return;
            var raw = await service.DetailAsync(environment with { }, record.Id, cancellation.Token);
            if (!cancellation.IsCancellationRequested && ReferenceEquals(Logs.SelectedItem, record) && loaded)
                RawText.Text = raw;
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            if (!cancellation.IsCancellationRequested) RawText.Text = "Could not load the full record: " + ex.Message + "\n\nLoaded fields:\n" + record.Raw;
        }
        finally
        {
            if (ReferenceEquals(detailCancellation, cancellation)) detailCancellation = null;
            cancellation.Dispose();
        }
    }

    private void UpdateDetailNotice()
    {
        var record = Logs.SelectedItem as TraceRecord;
        var updated = latest?.Records.FirstOrDefault(r => r.Id == record?.Id);
        DetailNotice.Text = record == null ? "" : updated == null ? "Outside the current results. This trace remains open for inspection." : updated.Raw != record.Raw ? "An updated version is available. Your current view is preserved." : "";
        DetailNotice.Visibility = string.IsNullOrEmpty(DetailNotice.Text) ? Visibility.Collapsed : Visibility.Visible;
        UpdateDetailButton.Visibility = updated != null && updated.Raw != record?.Raw ? Visibility.Visible : Visibility.Collapsed;
    }

    private void UpdateDetailClick(object sender, RoutedEventArgs e)
    {
        var id = (Logs.SelectedItem as TraceRecord)?.Id;
        if (latest == null || id == null) return;
        var replacement = latest.Records.FirstOrDefault(r => r.Id == id);
        if (replacement == null) return;
        CloseDetails();
        Display(latest.Records, true);
        Logs.SelectedItem = replacement;
    }

    private void CloseDetailsClick(object sender, RoutedEventArgs e) => CloseDetails();
    private void CloseDetails()
    {
        detailCancellation?.Cancel();
        if (Details.Visibility == Visibility.Visible) detailWidth = DetailColumn.Width;
        Details.Visibility = DetailSplitter.Visibility = Visibility.Collapsed;
        DetailColumn.Width = SplitterColumn.Width = new GridLength(0);
        changingSelection = true;
        Logs.SelectedItem = null;
        changingSelection = false;
        // Clearing selection allows the same row to be opened again with one click.
    }

    private void WrapClick(object sender, RoutedEventArgs e)
    {
        var wrap = WrapText.IsChecked == true ? TextWrapping.Wrap : TextWrapping.NoWrap;
        ExceptionText.TextWrapping = MessageText.TextWrapping = RawText.TextWrapping = wrap;
    }

    private void FindNextClick(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(FindText.Text)) return;
        if (DetailTabs.SelectedIndex == 2)
        {
            FindStatus.Text = RawText.FindNext(FindText.Text) ? "" : "No match";
            return;
        }
        var text = DetailTabs.SelectedIndex == 0 ? ExceptionText : MessageText;
        int start = Math.Min(text.Text.Length, text.SelectionStart + text.SelectionLength);
        int index = text.Text.IndexOf(FindText.Text, start, StringComparison.OrdinalIgnoreCase);
        if (index < 0) index = text.Text.IndexOf(FindText.Text, StringComparison.OrdinalIgnoreCase);
        FindStatus.Text = index < 0 ? "No match" : "";
        if (index < 0) return;
        text.Focus();
        text.Select(index, FindText.Text.Length);
        text.ScrollToLine(text.GetLineIndexFromCharacterIndex(index));
    }

    private async void SavedViewChanged(object sender, SelectionChangedEventArgs e)
    {
        if (initializing || SavedViews.SelectedItem is not TraceFilter filter) return;
        SetFilter(filter);
        SavedViewsButton.Content = filter.Name;
        SavedViewsPopup.IsOpen = false;
        await ApplyAsync();
    }

    private void SetFilter(TraceFilter filter)
    {
        initializing = true;
        ViewName.Text = filter.Name;
        Duration.SelectedItem = Duration.Items.Cast<ComboBoxItem>().FirstOrDefault(i => (string)i.Tag == filter.Minutes.ToString(CultureInfo.InvariantCulture)) ?? Duration.Items[2];
        TypeName.Text = filter.TypeName;
        ErrorsOnly.IsChecked = filter.ErrorsOnly;
        FullControl.IsChecked = filter.FullControl;
        Expression.Text = filter.Expression;
        FromTime.Text = filter.From?.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss") ?? "";
        ToTime.Text = filter.To?.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss") ?? "";
        initializing = false;
        UpdateFilterPreview();
    }

    private void RefreshSavedViews(TraceFilter? selected = null)
    {
        initializing = true;
        SavedViews.ItemsSource = null;
        SavedViews.ItemsSource = saved.OrderBy(s => s.Name).ToList();
        SavedViews.SelectedItem = selected;
        SavedViewsButton.Content = selected?.Name ?? "Saved views";
        initializing = false;
    }

    private async void SaveClick(object sender, RoutedEventArgs e) => await SaveAsync();
    private async Task SaveAsync()
    {
        try
        {
            var filter = ReadFilter();
            filter.Build(DateTimeOffset.UtcNow);
            var previous = SavedViews.SelectedItem as TraceFilter;
            if (string.IsNullOrWhiteSpace(filter.Name)) throw new FormatException("Enter a view name first.");
            if (saved.Any(s => !ReferenceEquals(s, previous) && string.Equals(s.Name, filter.Name, StringComparison.OrdinalIgnoreCase)))
                throw new FormatException("A saved view already has that name. Select it to overwrite it.");
            var updated = saved.Where(s => !ReferenceEquals(s, previous)).Concat(new[] { filter }).ToList();
            await PersistViewsAsync(updated);
            saved.Clear();
            saved.AddRange(updated);
            RefreshSavedViews(filter);
            ViewName.Text = filter.Name;
            SavedViewsPopup.IsOpen = false;
            UpdateFilterPreview();
            Status.Text = "Saved view: " + filter.Name;
        }
        catch (Exception ex) { Status.Text = "Could not save view: " + ex.Message; }
    }

    private async void DeleteViewClick(object sender, RoutedEventArgs e)
    {
        if (SavedViews.SelectedItem is not TraceFilter selected) return;
        try
        {
            var updated = saved.Where(s => !ReferenceEquals(s, selected)).ToList();
            await PersistViewsAsync(updated);
            saved.Clear();
            saved.AddRange(updated);
            RefreshSavedViews();
            SavedViewsPopup.IsOpen = false;
            ModifiedLabel.Text = "";
            Status.Text = "Deleted saved view: " + selected.Name;
        }
        catch (Exception ex) { Status.Text = "Could not delete view: " + ex.Message; }
    }

    private static async Task PersistViewsAsync(List<TraceFilter> views)
    {
        var options = await TraceExplorerOptions.GetLiveInstanceAsync();
        options.SavedViewsJson = JsonSerializer.Serialize(views);
        await options.SaveAsync();
    }

    private async void RelatedClick(object sender, RoutedEventArgs e)
    {
        if (Logs.SelectedItem is not TraceRecord record || record.CorrelationId == null || applied == null) return;
        var previous = back ?? new Investigation(applied, ReadFilter(), displayed, record, FindScroll(Logs)?.VerticalOffset ?? 0, RawText.Text);
        var related = new TraceFilter { FullControl = true, Expression = $"correlationid eq {record.CorrelationId:D}" };
        await QueryAsync(related, true, true);
        if (applied != related) return;
        back = previous;
        SetFilter(related);
        BackButton.Visibility = Visibility.Visible;
    }

    private void BackClick(object sender, RoutedEventArgs e)
    {
        if (back == null) return;
        var previous = back;
        CancelRequests();
        back = null;
        applied = previous.Filter;
        SetFilter(previous.Draft);
        latest = new TraceQueryResult(previous.Records, false);
        CloseDetails();
        Display(previous.Records, false);
        Logs.SelectedItem = previous.Selected;
        detailCancellation?.Cancel();
        RawText.Text = previous.Raw;
        FindScroll(Logs)?.ScrollToVerticalOffset(previous.Offset);
        BackButton.Visibility = UpdatesButton.Visibility = Visibility.Collapsed;
        Status.Text = "Returned to previous results.";
    }

    private async void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.F12 && Keyboard.Modifiers == ModifierKeys.None && Logs.IsKeyboardFocusWithin && Logs.SelectedItem is TraceRecord record)
        { e.Handled = true; await GoToDefinitionAsync(record); }
        else if (e.Key == Key.Escape && Details.Visibility == Visibility.Visible) { CloseDetails(); e.Handled = true; }
        else if (e.Key == Key.Enter && FindText.IsKeyboardFocusWithin) { FindNextClick(sender, e); e.Handled = true; }
        else if (e.Key == Key.Enter && (Keyboard.Modifiers == ModifierKeys.Control || QuickFilters.IsKeyboardFocusWithin || CustomRange.IsKeyboardFocusWithin))
        { e.Handled = true; await ApplyAsync(); }
    }

    private async void GoToDefinitionClick(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: TraceRecord record }) await GoToDefinitionAsync(record);
    }

    private async Task GoToDefinitionAsync(TraceRecord record)
    {
        navigationCancellation?.Cancel();
        using var cancellation = new CancellationTokenSource();
        navigationCancellation = cancellation;
        Status.Text = "Finding type definition in the current solution…";
        try
        {
            var result = await TraceDefinitionNavigator.NavigateAsync(record.TypeName, cancellation.Token);
            if (!disposed && !cancellation.IsCancellationRequested) Status.Text = result;
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            if (!disposed && !cancellation.IsCancellationRequested) Status.Text = "Could not navigate to the type: " + ex.Message;
        }
        finally
        {
            if (ReferenceEquals(navigationCancellation, cancellation)) navigationCancellation = null;
        }
    }

    private static ScrollViewer? FindScroll(DependencyObject parent)
    {
        if (parent is ScrollViewer viewer) return viewer;
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var found = FindScroll(VisualTreeHelper.GetChild(parent, i));
            if (found != null) return found;
        }
        return null;
    }

    public void Dispose()
    {
        disposed = true;
        RawText.Dispose();
        timer.Stop();
        DataverseEnvironmentProvider.EnvironmentChanged -= EnvironmentChanged;
        CancelRequests();
    }

    private sealed record Investigation(TraceFilter Filter, TraceFilter Draft, IReadOnlyList<TraceRecord> Records, TraceRecord Selected, double Offset, string Raw);
}
