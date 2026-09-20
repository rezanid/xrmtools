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
using XrmTools.Services;
using XrmTools.UI;

internal class DataverseExplorerViewModel : ViewModelBase, IDisposable
{
    private readonly IReadOnlyList<IExplorerCategoryProvider> _categories;
    private readonly ILogger _logger;
    private CancellationTokenSource _session = new();
    private SemaphoreSlim _operations = new(1, 1);
    private readonly Dictionary<ExplorerNodeBase, CancellationTokenSource> _lifetimes = [];
    private readonly IPluginRegistrationService? _registration;
    private readonly Func<Task<Uri?>>? _getEnvironmentUrl;
    private Uri? _environmentUrl;
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

    public DataverseExplorerViewModel(IEnumerable<IExplorerCategoryProvider> categories, ILogger logger,
        IPluginRegistrationService? registration = null, Func<Task<Uri?>>? getEnvironmentUrl = null)
    {
        _categories = categories.OrderBy(category => category.Order).ToList();
        _logger = logger;
        _registration = registration;
        _getEnvironmentUrl = getEnvironmentUrl;
        VisibleRoots = new ListCollectionView(RootNodes) { Filter = item => ((ExplorerNodeBase)item).IsVisible };
        RefreshCommand = new AsyncRelayCommand(RefreshAsync);
        CollapseAllCommand = new RelayCommand(CollapseAll);
        NodeExpandedCommand = new AsyncRelayCommand<ExplorerNodeBase>(LoadNodeAsync, AsyncRelayCommandOptions.AllowConcurrentExecutions);
        DataverseEnvironmentProvider.EnvironmentChanged += OnEnvironmentChanged;
        if (_registration != null) _registration.RegistrationChanged += OnRegistrationChanged;
    }

    private void OnRegistrationChanged(object? sender, PluginRegistrationChangedEventArgs change)
    {
        ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            await RefreshRegistrationAsync(change);
        }).FileAndForget("XrmTools/DataverseExplorer/RegistrationChanged");
    }

    internal async Task RefreshRegistrationAsync(PluginRegistrationChangedEventArgs change)
    {
        if (_disposed) return;
        var token = _session.Token;
        var operations = _operations;
        try
        {
            await operations.WaitAsync(token);
            try
            {
                if (_disposed || !SameEnvironment(_environmentUrl, change.EnvironmentUrl)) return;
                var category = change.PackageId.HasValue ? "Packages" : "Assemblies";
                var root = RootNodes.FirstOrDefault(node => node.ArtifactCategory == category);
                if (root != null) await RefreshNodeCoreAsync(root, token, (change.PackageId ?? change.AssemblyId)?.ToString());
                // Registering an existing standalone assembly into a package also changes standalone membership.
                if (change.PackageId.HasValue && change.AssemblyId.HasValue)
                {
                    var assemblies = RootNodes.FirstOrDefault(node => node.ArtifactCategory == "Assemblies");
                    if (assemblies?.Children.OfType<AssemblyNode>().Any(node => node.AssemblyId == change.AssemblyId) == true)
                        await RefreshNodeCoreAsync(assemblies, token, change.AssemblyId.ToString());
                }
            }
            finally { operations.Release(); }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { _logger.LogError(ex, "Error refreshing changed registration."); }
    }

    private static bool SameEnvironment(Uri? first, Uri second) => first != null &&
        string.Equals(first.AbsoluteUri.TrimEnd('/'), second.AbsoluteUri.TrimEnd('/'), StringComparison.OrdinalIgnoreCase);

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
        _operations = new SemaphoreSlim(1, 1);
        foreach (var lifetime in _lifetimes.Values) lifetime.Dispose();
        _lifetimes.Clear();
        _environmentUrl = null;
        var token = _session.Token;
        IsLoading = true;
        SelectedNode = null;
        RootNodes.Clear();
        try
        {
            var environmentUrl = _getEnvironmentUrl == null ? null : await _getEnvironmentUrl();
            token.ThrowIfCancellationRequested();
            _environmentUrl = environmentUrl;
            foreach (var provider in _categories)
            {
                var root = provider.CreateRoot();
                root.ReloadAsync = _ => Task.FromResult<ExplorerNodeBase?>(CreateRoot(provider));
                RootNodes.Add(root);
                BindTree(root);
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
        if (_disposed) return;
        var token = _session.Token;
        var operations = _operations;
        try
        {
            await operations.WaitAsync(token);
            try { await LoadNodeCoreAsync(node, token); }
            finally { operations.Release(); }
        }
        catch (OperationCanceledException) { }
    }

    private CategoryNode CreateRoot(IExplorerCategoryProvider provider)
    {
        var root = provider.CreateRoot();
        root.ReloadAsync = _ => Task.FromResult<ExplorerNodeBase?>(CreateRoot(provider));
        return root;
    }

    private async Task LoadNodeCoreAsync(ExplorerNodeBase? node, CancellationToken token)
    {
        if (_disposed || node == null || !node.CanLoadChildren || node.IsLoading) return;
        // Ignore expansion events from containers belonging to a previous environment/tree.
        if (!IsAttached(node)) return;
        try
        {
            node.IsLoading = true;
            node.LoadError = null;
            await node.LoadChildrenAsync!(token);
            token.ThrowIfCancellationRequested();
            node.AreChildrenLoaded = true;
            BindTree(node);
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

    internal async Task RefreshNodeAsync(ExplorerNodeBase node)
    {
        if (_disposed) return;
        var token = _session.Token;
        var operations = _operations;
        try
        {
            await operations.WaitAsync(token);
            try { await RefreshNodeCoreAsync(node, token); }
            finally { operations.Release(); }
        }
        catch (OperationCanceledException) { }
    }

    private async Task RefreshNodeCoreAsync(ExplorerNodeBase node, CancellationToken token, string? changedId = null)
    {
        if (_disposed || !IsAttached(node) || node.ReloadAsync == null) return;
        try
        {
            node.IsLoading = true;
            node.LoadError = null;
            var replacement = await node.ReloadAsync(token);
            token.ThrowIfCancellationRequested();
            if (replacement != null)
            {
                replacement.IsExpanded = node.IsExpanded;
                if (replacement.LoadChildrenAsync != null)
                {
                    await replacement.LoadChildrenAsync(token);
                    replacement.AreChildrenLoaded = true;
                }
                if (changedId != null)
                {
                    // Category membership is re-read for additions/deletions; unrelated branches keep their objects and loaded state.
                    for (var i = 0; i < replacement.Children.Count; i++)
                    {
                        var child = replacement.Children[i];
                        var previous = node.Children.FirstOrDefault(old => SameNode(old, child));
                        if (previous != null && child.Id != changedId) replacement.Children[i] = previous;
                    }
                }
                await RestoreChildrenAsync(node, replacement, token);
            }
            token.ThrowIfCancellationRequested();
            if (!IsAttached(node)) return;
            var oldNodes = Descendants(node).ToList();
            var newNodes = replacement == null ? [] : Descendants(replacement).ToList();
            var selected = SelectedNode;
            var selectionWasInside = selected != null && oldNodes.Contains(selected);
            var restoredSelection = selectionWasInside && replacement != null ? FindReplacement(node, replacement, selected!) : null;
            foreach (var retired in oldNodes.Except(newNodes))
            {
                if (_lifetimes.TryGetValue(retired, out var lifetime))
                {
                    lifetime.Cancel();
                    lifetime.Dispose();
                    _lifetimes.Remove(retired);
                }
            }
            var siblings = node.Parent?.Children ?? RootNodes;
            var index = siblings.IndexOf(node);
            if (replacement == null) siblings.RemoveAt(index);
            else
            {
                replacement.Parent = node.Parent;
                BindTree(replacement);
                siblings[index] = replacement;
            }
            if (selectionWasInside) SelectedNode = restoredSelection;
            ApplyFilter();
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            if (!token.IsCancellationRequested)
            {
                node.LoadError = "Unable to refresh. Use Refresh to retry.";
                _logger.LogError(ex, "Error refreshing node {0}", node.DisplayName);
            }
        }
        finally { node.IsLoading = false; }
    }

    private static async Task RestoreChildrenAsync(ExplorerNodeBase old, ExplorerNodeBase current, CancellationToken token)
    {
        foreach (var child in current.Children)
        {
            token.ThrowIfCancellationRequested();
            var previous = old.Children.FirstOrDefault(candidate => SameNode(candidate, child));
            if (previous == null || ReferenceEquals(previous, child)) continue;
            child.IsExpanded = previous.IsExpanded;
            if ((previous.AreChildrenLoaded || previous.IsExpanded) && child.CanLoadChildren)
            {
                await child.LoadChildrenAsync!(token);
                child.AreChildrenLoaded = true;
            }
            await RestoreChildrenAsync(previous, child, token);
        }
    }

    private static bool SameNode(ExplorerNodeBase first, ExplorerNodeBase second) => first.GetType() == second.GetType() && first.Id == second.Id;

    private static ExplorerNodeBase? FindReplacement(ExplorerNodeBase old, ExplorerNodeBase current, ExplorerNodeBase selected)
    {
        if (ReferenceEquals(old, selected)) return current;
        foreach (var child in old.Children)
        {
            var replacement = current.Children.FirstOrDefault(candidate => SameNode(child, candidate));
            if (replacement != null && FindReplacement(child, replacement, selected) is { } found) return found;
        }
        return null;
    }

    private static IEnumerable<ExplorerNodeBase> Descendants(ExplorerNodeBase node)
    {
        yield return node;
        foreach (var child in node.Children)
            foreach (var descendant in Descendants(child)) yield return descendant;
    }

    private bool IsAttached(ExplorerNodeBase node)
    {
        while (node.Parent != null)
        {
            if (!node.Parent.Children.Contains(node)) return false;
            node = node.Parent;
        }
        return RootNodes.Contains(node);
    }

    private void BindTree(ExplorerNodeBase node)
    {
        if (!_lifetimes.TryGetValue(node, out var lifetime))
        {
            lifetime = CancellationTokenSource.CreateLinkedTokenSource(_session.Token);
            _lifetimes[node] = lifetime;
            node.SessionToken = lifetime.Token;
            node.RefreshAsync = () => RefreshNodeAsync(node);
        }
        foreach (var child in node.Children)
        {
            child.Parent = node;
            BindTree(child);
        }
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
        if (_registration != null) _registration.RegistrationChanged -= OnRegistrationChanged;
        _session.Cancel();
        _session.Dispose();
        foreach (var lifetime in _lifetimes.Values) lifetime.Dispose();
        _lifetimes.Clear();
    }
}
