#nullable enable
namespace XrmTools.DataverseExplorer.ViewModels;

using CommunityToolkit.Mvvm.Input;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using XrmTools.DataverseExplorer.Models;
using XrmTools.DataverseExplorer.Services;
using XrmTools.Logging.Compatibility;
using XrmTools.UI;

internal class DataverseExplorerViewModel : ViewModelBase, IDisposable
{
    private readonly IReadOnlyList<IExplorerCategoryProvider> _categories;
    private readonly ILogger _logger;
    private CancellationTokenSource _session = new();
    private bool _disposed;
    private bool _matchCase;
    private bool _updatedLast3Hours;
    private bool _isLoading;
    private string _searchText = string.Empty;
    private ExplorerNodeBase? _selectedNode;

    public bool IsLoading { get => _isLoading; private set => SetProperty(ref _isLoading, value); }
    public string SearchText { get => _searchText; private set => SetProperty(ref _searchText, value); }
    public ObservableCollection<ExplorerNodeBase> RootNodes { get; } = [];
    public ICollectionView VisibleRoots { get; }
    public ExplorerNodeBase? SelectedNode
    {
        get => _selectedNode;
        set
        {
            if (SetProperty(ref _selectedNode, value)) SelectedNodeChanged?.Invoke(this, value);
        }
    }
    public event EventHandler<ExplorerNodeBase?>? SelectedNodeChanged;
    public ICommand RefreshCommand { get; }
    public ICommand CollapseAllCommand { get; }
    public ICommand NodeExpandedCommand { get; }

    public DataverseExplorerViewModel(IEnumerable<IExplorerCategoryProvider> categories, ILogger logger)
    {
        _categories = categories.OrderBy(category => category.Order).ToList();
        _logger = logger;
        VisibleRoots = new ListCollectionView(RootNodes) { Filter = item => ((ExplorerNodeBase)item).IsVisible };
        RefreshCommand = new AsyncRelayCommand(RefreshAsync);
        CollapseAllCommand = new RelayCommand(CollapseAll);
        NodeExpandedCommand = new AsyncRelayCommand<ExplorerNodeBase>(LoadNodeAsync, AsyncRelayCommandOptions.AllowConcurrentExecutions);
        DataverseEnvironmentProvider.EnvironmentChanged += OnEnvironmentChanged;
    }

    private void OnEnvironmentChanged(DataverseEnvironment environment)
    {
        // The environment provider can raise this event from a worker thread.
        ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            await RefreshAsync();
        }).FileAndForget("XrmTools/DataverseExplorer/EnvironmentChanged");
    }

    public Task InitializeAsync() => RefreshAsync();

    public async Task RefreshAsync()
    {
        if (_disposed) return;
        _session.Cancel();
        _session.Dispose();
        _session = new CancellationTokenSource();
        var token = _session.Token;
        IsLoading = true;
        SelectedNode = null;
        RootNodes.Clear();
        try
        {
            foreach (var provider in _categories)
            {
                var root = provider.CreateRoot();
                root.SessionToken = token;
                root.RefreshExplorerAsync = RefreshAsync;
                RootNodes.Add(root);
            }
            ApplyFilter();
            // Each category owns its loading and errors; a failed category does not hide the others.
            foreach (var root in RootNodes.ToList())
            {
                token.ThrowIfCancellationRequested();
                await LoadNodeAsync(root);
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing Dataverse Explorer");
        }
        finally
        {
            if (!token.IsCancellationRequested) IsLoading = false;
        }
    }

    internal async Task LoadNodeAsync(ExplorerNodeBase? node)
    {
        if (_disposed || node == null || !node.CanLoadChildren || node.IsLoading) return;
        // Ignore expansion events from containers belonging to a previous environment/tree.
        var root = node;
        while (root.Parent != null) root = root.Parent;
        if (!RootNodes.Contains(root)) return;
        var token = _session.Token;
        try
        {
            node.IsLoading = true;
            node.LoadError = null;
            await node.LoadChildrenAsync!(token);
            token.ThrowIfCancellationRequested();
            node.AreChildrenLoaded = true;
            ApplyFilter();
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            if (!token.IsCancellationRequested)
            {
                node.LoadError = "Unable to load children. Collapse and expand to retry.";
                _logger.LogError(ex, "Error expanding node {0}", node.DisplayName);
            }
        }
        finally { node.IsLoading = false; }
    }

    private void CollapseAll()
    {
        foreach (var root in RootNodes) Collapse(root);
    }

    private static void Collapse(ExplorerNodeBase node)
    {
        node.IsExpanded = false;
        foreach (var child in node.Children) Collapse(child);
    }

    public Task ClearSearchAsync() => ApplySearchAsync(string.Empty, false, false);

    public Task<uint> ApplySearchAsync(string searchText, bool matchCase, bool updatedLast3Hours)
    {
        SearchText = searchText;
        _matchCase = matchCase;
        _updatedLast3Hours = updatedLast3Hours;
        return Task.FromResult(ApplyFilter());
    }

    // Search intentionally covers loaded nodes. Views preserve node identity, parents and load state.
    private uint ApplyFilter()
    {
        var active = !string.IsNullOrWhiteSpace(SearchText) || _updatedLast3Hours;
        var threshold = DateTimeOffset.UtcNow.AddHours(-3);
        uint count = 0;
        foreach (var root in RootNodes) count += Filter(root, active, threshold);
        VisibleRoots.Refresh();
        if (SelectedNode is { IsVisible: false }) SelectedNode = null;
        return count;
    }

    private uint Filter(ExplorerNodeBase node, bool active, DateTimeOffset threshold)
    {
        uint descendants = 0;
        foreach (var child in node.Children) descendants += Filter(child, active, threshold);
        var comparison = _matchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
        var matchesText = string.IsNullOrWhiteSpace(SearchText) ||
            node.DisplayName.IndexOf(SearchText, comparison) >= 0 || node.Description.IndexOf(SearchText, comparison) >= 0;
        var matchesTime = !_updatedLast3Hours || node is ExplorerNodeBaseWithDates { ModifiedOn: { } modified } && modified >= threshold;
        node.IsVisible = !active || (matchesText && matchesTime) || descendants > 0;
        node.VisibleChildren.Refresh();
        if (active && descendants > 0) node.IsExpanded = true;
        return descendants + (node.IsVisible ? 1u : 0u);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        DataverseEnvironmentProvider.EnvironmentChanged -= OnEnvironmentChanged;
        _session.Cancel();
        _session.Dispose();
    }
}
