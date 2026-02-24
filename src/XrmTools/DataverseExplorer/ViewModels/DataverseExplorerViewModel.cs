#nullable enable
namespace XrmTools.DataverseExplorer.ViewModels;

using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using XrmTools.DataverseExplorer.Models;
using XrmTools.DataverseExplorer.Services;
using XrmTools.Logging.Compatibility;
using XrmTools.UI;

/// <summary>
/// ViewModel for the Dataverse Explorer tool window.
/// Manages the tree hierarchy and handles user interactions.
/// </summary>
internal class DataverseExplorerViewModel : ViewModelBase
{
    private readonly IExplorerDataService _dataService;
    private readonly ILogger _logger;
    private CancellationTokenSource? _cancellationTokenSource;

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                ApplySearchFilter();
            }
        }
    }

    private ObservableCollection<ExplorerNodeBase> _rootNodes = [];
    public ObservableCollection<ExplorerNodeBase> RootNodes
    {
        get => _rootNodes;
        set => SetProperty(ref _rootNodes, value);
    }

    private ExplorerNodeBase? _selectedNode;
    public ExplorerNodeBase? SelectedNode
    {
        get => _selectedNode;
        set => SetProperty(ref _selectedNode, value);
    }

    public ICommand RefreshCommand { get; }
    public ICommand CollapseAllCommand { get; }
    public ICommand NodeExpandedCommand { get; }

    public DataverseExplorerViewModel(IExplorerDataService dataService, ILogger logger)
    {
        _dataService = dataService;
        _logger = logger;

        RefreshCommand = new AsyncRelayCommand(RefreshAsync);
        CollapseAllCommand = new RelayCommand(CollapseAll);
        NodeExpandedCommand = new AsyncRelayCommand<ExplorerNodeBase>(OnNodeExpandedAsync);
    }

    public async Task InitializeAsync()
    {
        await RefreshAsync();
    }

    private async Task RefreshAsync()
    {
        try
        {
            IsLoading = true;
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();

            _dataService.ClearCache();
            
            // Load assemblies category
            var categoryNode = new CategoryNode
            {
                Id = "Assemblies",
                DisplayName = "Assemblies",
                Description = "Plugin Assemblies",
                AreChildrenLoaded = false
            };
            categoryNode.SetArtifactCategory("Assemblies");

            var assemblies = await _dataService.LoadAssembliesAsync(_cancellationTokenSource.Token);
            foreach (var assembly in assemblies)
            {
                categoryNode.Children.Add(assembly);
                assembly.Parent = categoryNode;
            }
            categoryNode.AreChildrenLoaded = true;

            RootNodes.Clear();
            RootNodes.Add(categoryNode);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Refresh cancelled by user");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing Dataverse Explorer");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task OnNodeExpandedAsync(ExplorerNodeBase? node)
    {
        if (node == null || !node.CanLoadChildren || node.IsLoading)
        {
            return;
        }

        try
        {
            node.IsLoading = true;

            switch (node)
            {
                case AssemblyNode assembly:
                    await _dataService.LoadAssemblyChildrenAsync(assembly, _cancellationTokenSource?.Token ?? CancellationToken.None);
                    break;

                case PluginTypeNode pluginType:
                    await _dataService.LoadPluginTypeChildrenAsync(pluginType, _cancellationTokenSource?.Token ?? CancellationToken.None);
                    break;

                case PluginStepNode step:
                    await _dataService.LoadPluginStepChildrenAsync(step, _cancellationTokenSource?.Token ?? CancellationToken.None);
                    break;

                case CategoryNode category:
                    // Category children are already loaded during refresh
                    break;
            }

            node.IsExpanded = true;
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Node expansion cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error expanding node {0}", node.DisplayName);
        }
        finally
        {
            node.IsLoading = false;
        }
    }

    private void CollapseAll()
    {
        foreach (var node in RootNodes)
        {
            CollapseNode(node);
        }
    }

    private static void CollapseNode(ExplorerNodeBase node)
    {
        node.IsExpanded = false;
        foreach (var child in node.Children)
        {
            CollapseNode(child);
        }
    }

    private void ApplySearchFilter()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            // Reset to show all
            RootNodes.Clear();
            RefreshAsync().GetAwaiter().GetResult();
            return;
        }

        // For now, just log the search. Full implementation would filter the tree.
        _logger.LogInformation("Search: {0}", SearchText);
    }
}

#nullable restore
