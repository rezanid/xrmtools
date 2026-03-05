#nullable enable
namespace XrmTools.DataverseExplorer.ViewModels;

using CommunityToolkit.Mvvm.Input;
using Microsoft.VisualStudio.Imaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    private List<ExplorerNodeBase>? _searchSourceRoots;

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
        set => SetProperty(ref _searchText, value);
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
        set
        {
            if (SetProperty(ref _selectedNode, value))
            {
                SelectedNodeChanged?.Invoke(this, value);
            }
        }
    }

    public event EventHandler<ExplorerNodeBase?>? SelectedNodeChanged;

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

    public async Task RefreshAsync()
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
                ImageMoniker = KnownMonikers.Assembly,
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
            _searchSourceRoots = null;
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

    public Task ClearSearchAsync()
    {
        if (_searchSourceRoots != null)
        {
            RootNodes = new ObservableCollection<ExplorerNodeBase>(_searchSourceRoots);
            _searchSourceRoots = null;
        }
        SearchText = string.Empty;
        return Task.CompletedTask;
    }

    public Task<uint> ApplySearchAsync(string searchText, bool matchCase, bool updatedLast3Hours)
    {
        SearchText = searchText;

        if (string.IsNullOrWhiteSpace(searchText) && !updatedLast3Hours)
        {
            return RestoreUnfilteredAsync();
        }

        _searchSourceRoots ??= [.. RootNodes];

        var nowUtc = DateTime.UtcNow;
        var thresholdUtc = nowUtc.AddHours(-3);

        var filteredRoots = new List<ExplorerNodeBase>();
        foreach (var root in _searchSourceRoots)
        {
            var filtered = FilterNode(root, searchText, matchCase, updatedLast3Hours, thresholdUtc);
            if (filtered != null)
            {
                filteredRoots.Add(filtered);
            }
        }

        RootNodes = new ObservableCollection<ExplorerNodeBase>(filteredRoots);
        return Task.FromResult((uint)CountNodes(filteredRoots));
    }

    private Task<uint> RestoreUnfilteredAsync()
    {
        if (_searchSourceRoots != null)
        {
            RootNodes = new ObservableCollection<ExplorerNodeBase>(_searchSourceRoots);
            var count = (uint)CountNodes(_searchSourceRoots);
            _searchSourceRoots = null;
            return Task.FromResult(count);
        }

        return Task.FromResult((uint)CountNodes(RootNodes));
    }

    private static ExplorerNodeBase? FilterNode(
        ExplorerNodeBase node,
        string searchText,
        bool matchCase,
        bool updatedLast3Hours,
        DateTime thresholdUtc)
    {
        var matchesText = string.IsNullOrWhiteSpace(searchText) || MatchesSearch(node, searchText, matchCase);
        var matchesTime = !updatedLast3Hours || (node.ModifiedOn.HasValue && node.ModifiedOn.Value.ToUniversalTime() >= thresholdUtc);

        var filteredChildren = new List<ExplorerNodeBase>();
        foreach (var child in node.Children)
        {
            var filteredChild = FilterNode(child, searchText, matchCase, updatedLast3Hours, thresholdUtc);
            if (filteredChild != null)
            {
                filteredChildren.Add(filteredChild);
            }
        }

        var includeNode = (matchesText && matchesTime) || filteredChildren.Count > 0;
        if (!includeNode)
        {
            return null;
        }

        var clone = CloneNode(node);
        foreach (var filteredChild in filteredChildren)
        {
            filteredChild.Parent = clone;
            clone.Children.Add(filteredChild);
        }

        clone.IsExpanded = filteredChildren.Count > 0;
        return clone;
    }

    private static bool MatchesSearch(ExplorerNodeBase node, string searchText, bool matchCase)
    {
        var comparison = matchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
        return node.DisplayName.IndexOf(searchText, comparison) >= 0 ||
               (!string.IsNullOrWhiteSpace(node.Description) && node.Description.IndexOf(searchText, comparison) >= 0);
    }

    private static int CountNodes(IEnumerable<ExplorerNodeBase> nodes)
    {
        var count = 0;
        foreach (var node in nodes)
        {
            count++;
            count += CountNodes(node.Children);
        }

        return count;
    }

    private static ExplorerNodeBase CloneNode(ExplorerNodeBase node)
    {
        return node switch
        {
            CategoryNode category => new CategoryNode
            {
                Id = category.Id,
                DisplayName = category.DisplayName,
                Description = category.Description,
                AreChildrenLoaded = category.AreChildrenLoaded,
                ImageMoniker = category.ImageMoniker,
                IsExpanded = category.IsExpanded,
                IsLoading = category.IsLoading,
                ModifiedOn = category.ModifiedOn,
            }.WithCategory(category.ArtifactCategory),

            AssemblyNode assembly => new AssemblyNode
            {
                Id = assembly.Id,
                DisplayName = assembly.DisplayName,
                Description = assembly.Description,
                AssemblyId = assembly.AssemblyId,
                PublicKeyToken = assembly.PublicKeyToken,
                Version = assembly.Version,
                IsolationMode = assembly.IsolationMode,
                SourceType = assembly.SourceType,
                AreChildrenLoaded = assembly.AreChildrenLoaded,
                ImageMoniker = assembly.ImageMoniker,
                IsExpanded = assembly.IsExpanded,
                IsLoading = assembly.IsLoading,
                ModifiedOn = assembly.ModifiedOn,
            },

            PluginTypeNode pluginType => new PluginTypeNode
            {
                Id = pluginType.Id,
                DisplayName = pluginType.DisplayName,
                Description = pluginType.Description,
                PluginTypeId = pluginType.PluginTypeId,
                TypeName = pluginType.TypeName,
                FriendlyName = pluginType.FriendlyName,
                WorkflowActivityGroupName = pluginType.WorkflowActivityGroupName,
                AreChildrenLoaded = pluginType.AreChildrenLoaded,
                ImageMoniker = pluginType.ImageMoniker,
                IsExpanded = pluginType.IsExpanded,
                IsLoading = pluginType.IsLoading,
                ModifiedOn = pluginType.ModifiedOn,
            },

            PluginStepNode step => new PluginStepNode
            {
                Id = step.Id,
                DisplayName = step.DisplayName,
                Description = step.Description,
                StepId = step.StepId,
                Stage = step.Stage,
                Mode = step.Mode,
                Rank = step.Rank,
                SdkMessageId = step.SdkMessageId,
                StateCode = step.StateCode,
                AsyncAutoDelete = step.AsyncAutoDelete,
                FilteringAttributes = step.FilteringAttributes,
                InvocationSource = step.InvocationSource,
                SupportedDeployment = step.SupportedDeployment,
                AreChildrenLoaded = step.AreChildrenLoaded,
                ImageMoniker = step.ImageMoniker,
                IsExpanded = step.IsExpanded,
                IsLoading = step.IsLoading,
                ModifiedOn = step.ModifiedOn,
            },

            PluginImageNode image => new PluginImageNode
            {
                Id = image.Id,
                DisplayName = image.DisplayName,
                Description = image.Description,
                ImageId = image.ImageId,
                ImageType = image.ImageType,
                MessagePropertyName = image.MessagePropertyName,
                Attributes = image.Attributes,
                EntityAlias = image.EntityAlias,
                ImageMoniker = image.ImageMoniker,
                IsExpanded = image.IsExpanded,
                IsLoading = image.IsLoading,
                ModifiedOn = image.ModifiedOn,
            },

            CustomApiNode api => new CustomApiNode
            {
                Id = api.Id,
                DisplayName = api.DisplayName,
                Description = api.Description,
                CustomApiId = api.CustomApiId,
                Name = api.Name,
                TypeName = api.TypeName,
                AreChildrenLoaded = api.AreChildrenLoaded,
                ImageMoniker = api.ImageMoniker,
                IsExpanded = api.IsExpanded,
                IsLoading = api.IsLoading,
                ModifiedOn = api.ModifiedOn,
            },

            CustomApiParameterNode parameter => new CustomApiParameterNode
            {
                Id = parameter.Id,
                DisplayName = parameter.DisplayName,
                Description = parameter.Description,
                ParameterId = parameter.ParameterId,
                Name = parameter.Name,
                ParameterType = parameter.ParameterType,
                IsOptional = parameter.IsOptional,
                AreChildrenLoaded = parameter.AreChildrenLoaded,
                ImageMoniker = parameter.ImageMoniker,
                IsExpanded = parameter.IsExpanded,
                IsLoading = parameter.IsLoading,
                ModifiedOn = parameter.ModifiedOn,
            },

            CustomApiResponseNode response => new CustomApiResponseNode
            {
                Id = response.Id,
                DisplayName = response.DisplayName,
                Description = response.Description,
                ResponseId = response.ResponseId,
                Name = response.Name,
                PropertyType = response.PropertyType,
                AreChildrenLoaded = response.AreChildrenLoaded,
                ImageMoniker = response.ImageMoniker,
                IsExpanded = response.IsExpanded,
                IsLoading = response.IsLoading,
                ModifiedOn = response.ModifiedOn,
            },

            _ => throw new InvalidOperationException($"Unsupported node type: {node.GetType().Name}"),
        };
    }
}

internal static class CategoryNodeExtensions
{
    public static CategoryNode WithCategory(this CategoryNode node, string category)
    {
        node.SetArtifactCategory(category);
        return node;
    }
}

#nullable restore
