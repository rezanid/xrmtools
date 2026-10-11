#nullable enable
namespace XrmTools.DataverseExplorer.ViewModels;

using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using XrmTools.DataverseExplorer.Models;
using XrmTools.DataverseExplorer.Services;
using XrmTools.Logging.Compatibility;
using XrmTools.UI;

/// <summary>An isolated, disposable selection workspace. It never modifies the browsing tree.</summary>
internal sealed class ExplorerCollectionSession : ViewModelBase, IDisposable
{
    private readonly IExplorerCollectionOperation _operation;
    private readonly ExplorerNodeBase _scope;
    private readonly Uri _environment;
    private readonly ILogger _logger;
    private readonly Action _close;
    private readonly CancellationTokenSource _lifetime = new();
    private CancellationTokenSource? _load;
    private readonly HashSet<string> _selected = new(StringComparer.Ordinal);
    private bool _busy;
    private bool _ready;
    private bool _disposed;
    private bool _executing;
    private string _search = string.Empty;
    private string _status = string.Empty;

    public string Title => _operation.Title;
    public string ActionLabel => _operation.ActionLabel;
    public string Context => _scope.DisplayName + " · " + _environment.Host;
    public ObservableCollection<ExplorerCollectionRow> Groups { get; } = [];
    public ICollectionView VisibleGroups { get; }
    public bool IsBusy { get => _busy; private set { if (SetProperty(ref _busy, value)) UpdateCommands(); } }
    public string Status { get => _status; private set => SetProperty(ref _status, value); }
    public string SelectionSummary => _selected.Count + " selected · " + Rows.Count(r => r.IsVisible) + " matching";
    public string SearchText { get => _search; set { if (SetProperty(ref _search, value)) ApplyFilter(); } }
    public AsyncRelayCommand ExecuteCommand { get; }
    public RelayCommand SelectMatchingCommand { get; }
    public RelayCommand ClearSelectionCommand { get; }
    public RelayCommand CancelCommand { get; }
    private IEnumerable<ExplorerCollectionRow> Rows => Groups.SelectMany(g => g.Children);

    public ExplorerCollectionSession(IExplorerCollectionOperation operation, ExplorerNodeBase scope, Uri environment,
        ILogger logger, Action close)
    {
        _operation = operation; _scope = scope; _environment = environment; _logger = logger; _close = close;
        VisibleGroups = new ListCollectionView(Groups) { Filter = item => ((ExplorerCollectionRow)item).IsVisible };
        ExecuteCommand = new AsyncRelayCommand(ExecuteAsync, () => !_disposed && _ready && !IsBusy && _selected.Count > 0);
        SelectMatchingCommand = new RelayCommand(SelectMatching, () => !_disposed && _ready && !IsBusy);
        ClearSelectionCommand = new RelayCommand(() => { if (_disposed || IsBusy) return; _selected.Clear(); SelectionChanged(); }, () => !_disposed && !IsBusy && _selected.Count > 0);
        CancelCommand = new RelayCommand(close, () => !_disposed);
    }

    public async Task LoadAsync()
    {
        if (_disposed || _executing) return;
        _load?.Cancel();
        _load?.Dispose();
        _load = CancellationTokenSource.CreateLinkedTokenSource(_lifetime.Token);
        var token = _load.Token;
        IsBusy = true; _ready = false; Status = "Loading available items…";
        try
        {
            var candidates = await _operation.LoadAsync(_scope, _environment, token);
            token.ThrowIfCancellationRequested();
            var ids = new HashSet<string>(candidates.Select(c => c.Id), StringComparer.Ordinal);
            if (ids.Count != candidates.Count) throw new InvalidOperationException("Candidate identities must be unique.");
            var removed = _selected.RemoveWhere(id => !ids.Contains(id));
            var expansion = Groups.ToDictionary(g => g.GroupId, g => g.IsExpanded, StringComparer.Ordinal);
            Groups.Clear();
            foreach (var group in candidates.GroupBy(c => c.GroupId).OrderBy(g => g.First().GroupName, StringComparer.Ordinal).ThenBy(g => g.Key, StringComparer.Ordinal))
            {
                var row = new ExplorerCollectionRow(this, group.First().GroupName, groupId: group.Key);
                if (expansion.TryGetValue(group.Key, out var expanded)) row.IsExpanded = expanded;
                foreach (var candidate in group.OrderBy(c => c.DisplayName, StringComparer.Ordinal).ThenBy(c => c.UniqueName, StringComparer.Ordinal))
                    row.Children.Add(new ExplorerCollectionRow(this, candidate.DisplayName, candidate));
                Groups.Add(row);
            }
            _ready = true;
            Status = candidates.Count == 0 ? "No eligible items in this scope." : removed > 0 ? removed + " previously selected items are no longer available." : string.Empty;
            ApplyFilter();
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested) { }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Collection discovery failed.");
            if (!_disposed && !token.IsCancellationRequested) Status = "Unable to load items. " + ex.Message + " Use Refresh to retry.";
        }
        finally { if (!_disposed && !token.IsCancellationRequested) { IsBusy = false; SelectionChanged(); } }
    }

    private async Task ExecuteAsync()
    {
        if (_disposed || !_ready || IsBusy || _selected.Count == 0) return;
        var token = _lifetime.Token;
        var selection = Rows.Where(r => r.Candidate != null && _selected.Contains(r.Candidate.Id)).Select(r => r.Candidate!).ToArray();
        _executing = true; IsBusy = true; Status = "Working…";
        try
        {
            await _operation.ExecuteAsync(selection, _environment, token);
            token.ThrowIfCancellationRequested();
            _close();
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested) { }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Collection operation failed.");
            if (!_disposed && !token.IsCancellationRequested) Status = "Unable to complete the operation. " + ex.Message;
        }
        finally { _executing = false; if (!_disposed) { IsBusy = false; SelectionChanged(); } }
    }

    internal bool IsChecked(string id) => _selected.Contains(id);
    internal void SetChecked(ExplorerCollectionRow row, bool value)
    {
        if (_disposed || IsBusy || !_ready) return;
        foreach (var candidate in row.Candidate == null ? row.Children.Where(c => c.IsVisible).Select(c => c.Candidate!) : new[] { row.Candidate })
            if (value) _selected.Add(candidate.Id); else _selected.Remove(candidate.Id);
        SelectionChanged();
    }
    private void SelectMatching()
    {
        if (_disposed || IsBusy || !_ready) return;
        foreach (var row in Rows.Where(r => r.IsVisible)) _selected.Add(row.Candidate!.Id);
        SelectionChanged();
    }
    private void ApplyFilter()
    {
        foreach (var group in Groups)
        {
            var groupMatches = group.DisplayName.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0;
            foreach (var row in group.Children)
                row.IsVisible = groupMatches || row.DisplayName.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0 || row.UniqueName.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0;
            group.IsVisible = group.Children.Any(r => r.IsVisible);
            if (group.IsVisible && !string.IsNullOrWhiteSpace(SearchText)) group.IsExpanded = true;
            group.VisibleChildren.Refresh();
        }
        VisibleGroups.Refresh();
        SelectionChanged();
    }
    private void SelectionChanged()
    {
        foreach (var group in Groups) { group.NotifySelection(); foreach (var row in group.Children) row.NotifySelection(); }
        OnPropertyChanged(nameof(SelectionSummary)); UpdateCommands();
    }
    private void UpdateCommands()
    {
        ExecuteCommand.NotifyCanExecuteChanged();
        SelectMatchingCommand.NotifyCanExecuteChanged(); ClearSelectionCommand.NotifyCanExecuteChanged();
    }
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true; _lifetime.Cancel(); _load?.Cancel(); _load?.Dispose(); _lifetime.Dispose();
        UpdateCommands(); CancelCommand.NotifyCanExecuteChanged();
    }
}

internal sealed class ExplorerCollectionRow : ViewModelBase
{
    private readonly ExplorerCollectionSession _session;
    private bool _isExpanded = true;
    public ExplorerCollectionCandidate? Candidate { get; }
    public string DisplayName { get; }
    public string GroupId { get; }
    public string UniqueName => Candidate?.UniqueName ?? string.Empty;
    public string SelectionHint => Candidate == null ? "Select matching items in " + DisplayName : "Select " + DisplayName + " (" + UniqueName + ")";
    public ObservableCollection<ExplorerCollectionRow> Children { get; } = [];
    public ICollectionView VisibleChildren { get; }
    public bool IsVisible { get; set; } = true;
    public bool IsExpanded { get => _isExpanded; set => SetProperty(ref _isExpanded, value); }
    public bool? IsChecked
    {
        get
        {
            if (Candidate != null) return _session.IsChecked(Candidate.Id);
            var rows = Children.Where(c => c.IsVisible).ToArray();
            var count = rows.Count(c => c.IsChecked == true);
            return count == 0 ? false : count == rows.Length ? true : null;
        }
        set => _session.SetChecked(this, value ?? true);
    }
    public ExplorerCollectionRow(ExplorerCollectionSession session, string name, ExplorerCollectionCandidate? candidate = null, string groupId = "")
    {
        _session = session; DisplayName = name; Candidate = candidate; GroupId = groupId;
        VisibleChildren = new ListCollectionView(Children) { Filter = item => ((ExplorerCollectionRow)item).IsVisible };
    }
    internal void NotifySelection() => OnPropertyChanged(nameof(IsChecked));
}
