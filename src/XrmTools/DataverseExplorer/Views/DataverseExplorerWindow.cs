#nullable enable
namespace XrmTools.DataverseExplorer.Views;

using System.ComponentModel.Design;
using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using XrmTools.DataverseExplorer.Models;
using XrmTools.DataverseExplorer.Services;
using XrmTools.DataverseExplorer.ViewModels;
using XrmTools.Helpers;
using XrmTools.Logging.Compatibility;
using XrmTools.Options;

internal class DataverseExplorerSource
{
    public required ILogger Logger { get; init; }
    public required IExplorerDataService DataService { get; init; }

}

[Guid(WindowGuidString)]
internal class DataverseExplorerWindow : ToolWindowPane // BaseToolWindow<DataverseExplorerWindow>
{
    public const string WindowGuidString = "a0dab530-9b3a-4b72-b273-89139ad98537";
    public const string WindowCaption = "Dataverse Explorer";

    private readonly DataverseExplorerViewModel? _viewModel;
    private ITrackSelection? _trackSelection;
    private readonly SelectionContainer _selectionContainer = new();
    private readonly VisualStudioWorkspace _workspace;
    private IVsEnumWindowSearchOptions? _searchOptions;
    private IVsEnumWindowSearchFilters? _searchFilters;
    private WindowSearchBooleanOption? _matchCaseOption;
    
    public ILogger Logger { get; set; }

    internal IExplorerDataService DataService { get; set; }

    public DataverseExplorerWindow(DataverseExplorerSource source) : base()
    {
        var componentModel = (IComponentModel)Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(SComponentModel));
        _workspace = componentModel.GetService<VisualStudioWorkspace>();

        BitmapImageMoniker = KnownMonikers.Search;
        Caption = WindowCaption;
        ToolBar = new CommandID(PackageGuids.XrmToolsCmdSetId, PackageIds.DataverseExplorerToolbar);
        ToolBarLocation = (int)VSTWT_LOCATION.VSTWT_TOP;

        Logger = source.Logger;
        DataService = source.DataService;

        var control = new DataverseExplorerWindowControl();
        _viewModel = new DataverseExplorerViewModel(DataService, Logger);
        _viewModel.SelectedNodeChanged += OnSelectedNodeChanged;
        control.DataContext = _viewModel;
        Content = control;
    }

    public Task RefreshAsync()
    {
        return _viewModel?.RefreshAsync() ?? Task.CompletedTask;
    }

    public override bool SearchEnabled => true;

    public WindowSearchBooleanOption MatchCaseOption
    {
        get
        {
            return _matchCaseOption ??= new WindowSearchBooleanOption("Match case", "Match case", false);
        }
    }

    public override IVsEnumWindowSearchOptions SearchOptionsEnum
    {
        get
        {
            if (_searchOptions == null)
            {
                var list = new List<IVsWindowSearchOption> { MatchCaseOption };
                _searchOptions = new WindowSearchOptionEnumerator(list);
            }

            return _searchOptions;
        }
    }

    public override IVsEnumWindowSearchFilters SearchFiltersEnum
    {
        get
        {
            if (_searchFilters == null)
            {
                var list = new List<IVsWindowSearchFilter>
                {
                    new WindowSearchSimpleFilter("Updated in last 3 hours", "Updated in last 3 hours", "updated", "3h")
                };
                _searchFilters = new WindowSearchFilterEnumerator(list);
            }

            return _searchFilters;
        }
    }

    public override void ProvideSearchSettings(IVsUIDataSource pSearchSettings)
    {
        Utilities.SetValue(
            pSearchSettings,
            SearchSettingsDataSource.SearchStartTypeProperty.Name,
            (uint)VSSEARCHSTARTTYPE.SST_INSTANT);
        Utilities.SetValue(
            pSearchSettings,
            SearchSettingsDataSource.ControlMaxWidthProperty.Name,
            (uint)10000);
    }

    public override IVsSearchTask? CreateSearch(uint dwCookie, IVsSearchQuery pSearchQuery, IVsSearchCallback pSearchCallback)
    {
        if (_viewModel == null || pSearchQuery == null || pSearchCallback == null)
        {
            return null;
        }

        return new DataverseExplorerSearchTask(dwCookie, pSearchQuery, pSearchCallback, this);
    }

    public override void ClearSearch()
    {
        if (_viewModel == null)
        {
            return;
        }

        ThreadHelper.JoinableTaskFactory.Run(async () =>
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            await _viewModel.ClearSearchAsync();
        });
    }

    protected override void OnCreate()
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        base.OnCreate();
        _trackSelection = GetService(typeof(STrackSelection)) as ITrackSelection;
        UpdatePropertiesWindowSelection(_viewModel?.SelectedNode);
        _ = InitializeViewModelAsync();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && _viewModel != null)
        {
            _viewModel.SelectedNodeChanged -= OnSelectedNodeChanged;
            if (ThreadHelper.CheckAccess())
            {
                UpdatePropertiesWindowSelection(null);
            }
        }

        base.Dispose(disposing);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD102:Implement internal logic asynchronously", Justification = "Event Handler")]
    private void OnSelectedNodeChanged(object? sender, ExplorerNodeBase? node)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        UpdatePropertiesWindowSelection(node);
        ThreadHelper.JoinableTaskFactory.Run(
            async () => await SyncWithProjectSystemAsync(node));
    }

    private void UpdatePropertiesWindowSelection(ExplorerNodeBase? node)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        if (_trackSelection == null)
        {
            return;
        }

        object[] selectedObjects = node switch
        {

            CategoryNode or null => [],
            _ => [node]
        };

        _selectionContainer.SelectableObjects = selectedObjects;
        _selectionContainer.SelectedObjects = selectedObjects;
        _ = _trackSelection.OnSelectChange(_selectionContainer);
    }

    private async Task SyncWithProjectSystemAsync(ExplorerNodeBase? node)
    {
        if (node == null || node.ArtifactCategory != "Assemblies")
        {
            return;
        }

        var options = await GeneralOptions.GetLiveInstanceAsync();
        if (!options.DataverseExplorerSynchronizeWithSolutionExplorer)
        {
            return;
        }
        var openPreview = options.DataverseExplorerOpenInPreviewTab;

        AssemblyNode? assemblyNode;
        PluginTypeNode? pluginTypeNode;
        CustomApiNode? customApiNode;

        assemblyNode = node as AssemblyNode;
        if (assemblyNode != null)
        {
            await SolutionNavigator.SelectInSolutionExplorerAsync(assemblyNode.DisplayName, null);
            return;
        }
        pluginTypeNode = node as PluginTypeNode;
        if (pluginTypeNode != null)
        {
            assemblyNode = pluginTypeNode.Parent as AssemblyNode;
            if (assemblyNode == null)
            {
                return;
            }
            await SolutionNavigator.SelectInSolutionExplorerAsync(assemblyNode.DisplayName, pluginTypeNode.TypeName, openPreview);
        }
        customApiNode = node as CustomApiNode;
        if (customApiNode != null)
        {
            assemblyNode = customApiNode.Parent as AssemblyNode;
            if (assemblyNode == null)
            {
                return;
            }
            await SolutionNavigator.SelectInSolutionExplorerAsync(assemblyNode.DisplayName, customApiNode.TypeName, openPreview);
        }
    }

    private async Task InitializeViewModelAsync()
    {
        if (_viewModel != null)
        {
            await _viewModel.InitializeAsync();
        }
    }

    private async Task<uint> ApplySearchAsync(string searchQuery)
    {
        if (_viewModel == null)
        {
            return 0;
        }

        var updatedFilter = "updated:\"3h\"";
        var filterIndex = searchQuery.IndexOf(updatedFilter, StringComparison.OrdinalIgnoreCase);
        var updatedLastThreeHours = filterIndex >= 0;
        var searchText = updatedLastThreeHours
            ? (searchQuery.Remove(filterIndex, updatedFilter.Length)).Trim()
            : searchQuery;

        return await _viewModel.ApplySearchAsync(searchText, MatchCaseOption.Value, updatedLastThreeHours);
    }

    private sealed class DataverseExplorerSearchTask(
        uint cookie,
        IVsSearchQuery searchQuery,
        IVsSearchCallback searchCallback,
        DataverseExplorerWindow toolWindow)
        : VsSearchTask(cookie, searchQuery, searchCallback)
    {
        protected override void OnStartSearch()
        {
            ErrorCode = VSConstants.S_OK;
            try
            {
                var resultCount = ThreadHelper.JoinableTaskFactory.Run(async () =>
                {
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    return await toolWindow.ApplySearchAsync(SearchQuery.SearchString);
                });
                SearchResults = resultCount;
            }
            catch
            {
                ErrorCode = VSConstants.E_FAIL;
                SearchResults = 0;
            }
            finally
            {
                base.OnStartSearch();
            }
        }

        protected override void OnStopSearch()
        {
            SearchResults = 0;
        }
    }
}

#nullable restore
