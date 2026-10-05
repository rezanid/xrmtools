#nullable enable
namespace XrmTools.PluginTrace;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

/// <summary>View-only behavior: viewport, focus, editor selection and visual lifetime.</summary>
public partial class TraceExplorerControl : UserControl, IDisposable
{
    private readonly TraceExplorerViewModel viewModel;
    private readonly DispatcherTimer timer = new();
    private readonly List<SortDescription> sortDescriptions = [new(nameof(TraceRecord.CreatedOn), ListSortDirection.Descending)];
    private bool changingSelection, disposed;
    private GridLength detailWidth = new(1, GridUnitType.Star);
    private Viewport viewport = new(0, 0, null), investigationViewport = new(0, 0, null);

    internal TraceExplorerControl(TraceExplorerService service)
        : this(new TraceExplorerViewModel(service, new TraceViewStore(), new TraceNavigator(), new TraceDispatcher(Dispatcher.CurrentDispatcher))) { }

    internal TraceExplorerControl(TraceExplorerViewModel viewModel)
    {
        this.viewModel = viewModel;
        InitializeComponent();
        DataContext = viewModel;
        // Popup content is hosted outside the view's visual tree.
        SavedViewsPopup.DataContext = viewModel;
        timer.Tick += TimerTick;
        viewModel.PropertyChanged += ViewModelChanged;
        viewModel.Detail.PropertyChanged += DetailChanged;
        viewModel.RecordsChanging += RecordsChanging;
        viewModel.RecordsChanged += RecordsChanged;
        viewModel.InvestigationRestored += RestoreInvestigationViewport;
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        KeyDown += OnKeyDown;
        SetTimerInterval();
        UpdateTypeSuggestions();
        UpdateSortIndicators();
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await viewModel.ActivateAsync();
        if (!disposed && IsLoaded) timer.Start();
    }
    private void OnUnloaded(object sender, RoutedEventArgs e) { timer.Stop(); viewModel.Deactivate(); }
    private async void TimerTick(object? sender, EventArgs e) => await viewModel.TickAsync(IsVisible);
    private void SetTimerInterval() => timer.Interval = TimeSpan.FromSeconds(viewModel.IntervalSeconds);
    private void ViewModelChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TraceExplorerViewModel.IntervalSeconds)) SetTimerInterval();
        if (e.PropertyName == nameof(TraceExplorerViewModel.SelectedRecord)) SyncSelection();
        if (e.PropertyName == nameof(TraceExplorerViewModel.SelectedView)) SavedViewsPopup.IsOpen = false;
        if (e.PropertyName == nameof(TraceExplorerViewModel.TypeSuggestions)) UpdateTypeSuggestions();
    }
    private void DetailChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TraceDetailViewModel.IsOpen))
        {
            if (!viewModel.Detail.IsOpen && DetailColumn.Width.Value > 0) detailWidth = DetailColumn.Width;
            DetailColumn.Width = viewModel.Detail.IsOpen ? detailWidth : new GridLength(0);
            SplitterColumn.Width = new GridLength(viewModel.Detail.IsOpen ? 5 : 0);
        }
        if (e.PropertyName == nameof(TraceDetailViewModel.Record)) FindStatus.Text = "";
    }
    private void LogSelected(object sender, SelectionChangedEventArgs e)
    {
        if (!changingSelection) viewModel.SelectedRecord = Logs.SelectedItem as TraceRecord;
    }
    private void SyncSelection()
    {
        var previous = changingSelection; changingSelection = true;
        Logs.SelectedItem = viewModel.SelectedRecord;
        changingSelection = previous;
    }
    private Viewport CaptureViewport()
    {
        var scroll = FindScroll(Logs); var offset = scroll?.VerticalOffset ?? 0;
        var anchor = offset < Logs.Items.Count ? (Logs.Items[(int)offset] as TraceRecord)?.Id : null;
        return new Viewport(offset, scroll?.HorizontalOffset ?? 0, anchor);
    }
    private void RecordsChanging(bool preserve)
    {
        if (preserve) viewport = CaptureViewport();
        changingSelection = true;
    }
    private void RecordsChanged(bool preserve)
    {
        var view = CollectionViewSource.GetDefaultView(Logs.ItemsSource);
        if (view?.CanSort == true)
        {
            using (view.DeferRefresh())
            {
                view.SortDescriptions.Clear();
                foreach (var sort in sortDescriptions) view.SortDescriptions.Add(sort);
            }
        }
        SyncSelection(); UpdateSortIndicators();
        changingSelection = false;
        RestoreViewport(preserve ? viewport : new Viewport(0, 0, null));
    }
    private void RestoreViewport(Viewport state)
    {
        var offset = state.Offset;
        if (state.Anchor != null)
        {
            int index = Logs.Items.Cast<TraceRecord>().ToList().FindIndex(r => r.Id == state.Anchor);
            if (index >= 0) offset = index;
        }
        Logs.UpdateLayout(); var scroll = FindScroll(Logs);
        scroll?.ScrollToVerticalOffset(offset); scroll?.ScrollToHorizontalOffset(state.HorizontalOffset);
    }
    private void LogsSorting(object sender, DataGridSortingEventArgs e)
    {
        e.Handled = true;
        var direction = e.Column.SortDirection == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
        sortDescriptions.Clear(); sortDescriptions.Add(new SortDescription(e.Column.SortMemberPath, direction));
        RecordsChanging(true); RecordsChanged(true);
    }
    private void UpdateSortIndicators()
    {
        foreach (var column in Logs.Columns)
        {
            var sort = sortDescriptions.FirstOrDefault(s => s.PropertyName == column.SortMemberPath);
            column.SortDirection = string.IsNullOrEmpty(sort.PropertyName) ? null : sort.Direction;
        }
    }
    private void RelatedClick(object sender, RoutedEventArgs e)
    {
        if (!viewModel.CanGoBack) investigationViewport = CaptureViewport();
        if (viewModel.RelatedCommand.CanExecute(null)) viewModel.RelatedCommand.Execute(null);
    }
    private void RestoreInvestigationViewport() => RestoreViewport(investigationViewport);
    private void SavedViewChanged(object sender, SelectionChangedEventArgs e)
    {
        // Replacing ItemsSource temporarily clears selection. Do not write that visual reset into the model.
        if (SavedViews.SelectedItem is TraceFilter filter)
        {
            viewModel.SelectedView = filter;
            SavedViewsPopup.IsOpen = false;
        }
    }
    private void SavedViewsSourceUpdated(object sender, DataTransferEventArgs e)
    {
        if (e.Property == ItemsControl.ItemsSourceProperty)
            SavedViews.SetCurrentValue(Selector.SelectedItemProperty, viewModel.SelectedView);
    }
    private void SavedViewActionClick(object sender, RoutedEventArgs e) => SavedViewsPopup.IsOpen = false;
    private void TypeSuggestionsFocusChanged(object sender, KeyboardFocusChangedEventArgs e) => UpdateTypeSuggestions();
    private void TypeSuggestionsDropDownClosed(object sender, EventArgs e) => UpdateTypeSuggestions();
    private void UpdateTypeSuggestions()
    {
        // Updating an editable ComboBox's list while typing/open can reset its text and caret.
        if (TypeName.IsKeyboardFocusWithin || TypeName.IsDropDownOpen) return;
        if (TypeName.ItemsSource is IReadOnlyList<string> previous && previous.SequenceEqual(viewModel.TypeSuggestions)) return;
        var text = viewModel.Filter.TypeName;
        TypeName.ItemsSource = viewModel.TypeSuggestions;
        TypeName.SetCurrentValue(ComboBox.TextProperty, text);
    }
    private void WrapClick(object sender, RoutedEventArgs e)
    {
        var wrap = WrapText.IsChecked == true ? TextWrapping.Wrap : TextWrapping.NoWrap;
        ExceptionText.TextWrapping = MessageText.TextWrapping = RawText.TextWrapping = wrap;
    }
    private void FindNextClick(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(FindText.Text)) return;
        if (DetailTabs.SelectedIndex == 2) { FindStatus.Text = RawText.FindNext(FindText.Text) ? "" : "No match"; return; }
        var text = DetailTabs.SelectedIndex == 0 ? ExceptionText : MessageText;
        int start = Math.Min(text.Text.Length, text.SelectionStart + text.SelectionLength);
        int index = text.Text.IndexOf(FindText.Text, start, StringComparison.OrdinalIgnoreCase);
        if (index < 0) index = text.Text.IndexOf(FindText.Text, StringComparison.OrdinalIgnoreCase);
        FindStatus.Text = index < 0 ? "No match" : "";
        if (index < 0) return;
        text.Focus(); text.Select(index, FindText.Text.Length); text.ScrollToLine(text.GetLineIndexFromCharacterIndex(index));
    }
    private void GoToDefinitionClick(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: TraceRecord record }) viewModel.NavigateCommand.Execute(record);
    }
    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.F12 && Keyboard.Modifiers == ModifierKeys.None && Logs.IsKeyboardFocusWithin && viewModel.SelectedRecord is { } record)
        { e.Handled = true; viewModel.NavigateCommand.Execute(record); }
        else if (e.Key == Key.Escape && viewModel.Detail.IsOpen) { viewModel.CloseDetailCommand.Execute(null); e.Handled = true; }
        else if (e.Key == Key.Enter && FindText.IsKeyboardFocusWithin) { FindNextClick(sender, e); e.Handled = true; }
        else if (e.Key == Key.Enter && (Keyboard.Modifiers == ModifierKeys.Control || QuickFilters.IsKeyboardFocusWithin || CustomRange.IsKeyboardFocusWithin))
        { e.Handled = true; viewModel.RunCommand.Execute(null); }
    }
    private static ScrollViewer? FindScroll(DependencyObject parent)
    {
        if (parent is ScrollViewer viewer) return viewer;
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var found = FindScroll(VisualTreeHelper.GetChild(parent, i)); if (found != null) return found;
        }
        return null;
    }
    public void Dispose()
    {
        if (disposed) return; disposed = true; timer.Stop(); timer.Tick -= TimerTick;
        viewModel.PropertyChanged -= ViewModelChanged; viewModel.Detail.PropertyChanged -= DetailChanged;
        viewModel.RecordsChanging -= RecordsChanging; viewModel.RecordsChanged -= RecordsChanged;
        viewModel.InvestigationRestored -= RestoreInvestigationViewport;
        viewModel.Dispose(); RawText.Dispose();
    }
    private sealed record Viewport(double Offset, double HorizontalOffset, Guid? Anchor);
}
