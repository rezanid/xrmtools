#nullable enable
namespace XrmTools.DataverseExplorer.Views;

using System.ComponentModel.Design;
using System.ComponentModel;
using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using XrmTools.DataverseExplorer.Models;
using XrmTools.DataverseExplorer.Services;
using XrmTools.DataverseExplorer.ViewModels;
using XrmTools.Logging.Compatibility;
using XrmTools.Options;
using XrmTools.Helpers;
using XrmTools.Services;

internal class DataverseExplorerSource
{
    public required ILogger Logger { get; init; }
    public required IEnumerable<IExplorerCategoryProvider> Categories { get; init; }
    public required IEnumerable<IExplorerCommandProvider> Commands { get; init; }
    public IEnumerable<IExplorerCollectionOperation> CollectionOperations { get; init; } = [];
    public required IPluginRegistrationService Registration { get; init; }
    public required Func<Task<Uri?>> GetEnvironmentUrl { get; init; }

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

    internal static DataverseExplorerWindow? Current { get; private set; }
    internal ExplorerCollectionSession? Collection => _viewModel?.Collection;
    private ExplorerCollectionSession? _toolbarSession;

    private string _browseQuery = string.Empty;
    private bool _browseMatchCase;

    public ILogger Logger { get; set; }

    public DataverseExplorerWindow(DataverseExplorerSource source) : base()
    {
        var componentModel = (IComponentModel)Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(SComponentModel));
        _workspace = componentModel.GetService<VisualStudioWorkspace>();

        BitmapImageMoniker = KnownMonikers.Search;
        Caption = WindowCaption;
        ToolBar = new CommandID(PackageGuids.XrmToolsCmdSetId, PackageIds.DataverseExplorerToolbar);
        ToolBarLocation = (int)VSTWT_LOCATION.VSTWT_TOP;

        Logger = source.Logger;

        var control = new DataverseExplorerWindowControl();
        _viewModel = new DataverseExplorerViewModel(source.Categories, Logger, source.Registration, source.GetEnvironmentUrl);
        _viewModel.SelectedNodeChanged += OnSelectedNodeChanged;
        control.CommandProviders = source.Commands;
        control.CollectionOperations = source.CollectionOperations;
        control.DataContext = _viewModel;
        control.FocusSearchRequested += () => SearchHost?.Activate();
        _viewModel.PropertyChanged += OnWorkspaceChanged;
        Content = control;
    }

    public Task RefreshAsync()
    {
        return _viewModel?.RefreshWorkspaceAsync() ?? Task.CompletedTask;
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
            if (_viewModel.Collection is { } collection) collection.ApplySearch(string.Empty, MatchCaseOption.Value, false);
            else await _viewModel.ApplySearchAsync(string.Empty, MatchCaseOption.Value, false);
        });
    }


    protected override void OnCreate()
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        base.OnCreate();
        // Keep native search on its own row so it does not crowd toolbar commands.
        ErrorHandler.ThrowOnFailure(((IVsWindowFrame)Frame).SetProperty(
            (int)__VSFPROPID5.VSFPROPID_SearchPlacement,
            (int)__VSSEARCHPLACEMENT.SP_STRETCH));
        Current = this;
        _trackSelection = GetService(typeof(STrackSelection)) as ITrackSelection;
        UpdatePropertiesWindowSelection(_viewModel?.SelectedNode);
        _ = InitializeViewModelAsync();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && _viewModel != null)
        {
            _viewModel.SelectedNodeChanged -= OnSelectedNodeChanged;
            _viewModel.PropertyChanged -= OnWorkspaceChanged;
            if (_toolbarSession != null) _toolbarSession.PropertyChanged -= OnCollectionStateChanged;
            if (ReferenceEquals(Current, this)) Current = null;
            _viewModel.Dispose();
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
        if (node == null || _viewModel?.Collection != null)
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

        if (_viewModel.Collection is { } collection) return collection.ApplySearch(searchText, MatchCaseOption.Value, updatedLastThreeHours);
        return await _viewModel.ApplySearchAsync(searchText, MatchCaseOption.Value, updatedLastThreeHours);
    }

    private void OnWorkspaceChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(DataverseExplorerViewModel.Collection)) return;
        if (_toolbarSession != null) _toolbarSession.PropertyChanged -= OnCollectionStateChanged;
        _toolbarSession = _viewModel?.Collection;
        if (_toolbarSession != null) _toolbarSession.PropertyChanged += OnCollectionStateChanged;
        UpdateToolbar();
        if (SearchHost == null) return;
        ThreadHelper.ThrowIfNotOnUIThread();
        if (_viewModel?.Collection != null)
        {
            _browseQuery = SearchHost.SearchQuery?.SearchString ?? string.Empty;
            _browseMatchCase = MatchCaseOption.Value;
            SearchHost.SearchAsync(null!);
            MatchCaseOption.Value = false;
        }
        else
        {
            SearchHost.SearchAsync(null!);
            MatchCaseOption.Value = _browseMatchCase;
            if (!string.IsNullOrEmpty(_browseQuery)) SearchHost.SearchAsync(SearchUtilities.CreateSearchQuery(_browseQuery));
        }
        UpdateToolbar();
    }

    private void OnCollectionStateChanged(object? sender, PropertyChangedEventArgs e) => UpdateToolbar();

    private void UpdateToolbar()
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        if (GetService(typeof(SVsUIShell)) is IVsUIShell shell) shell.UpdateCommandUI(0);
    }

    private sealed class DataverseExplorerSearchTask(
        uint cookie,
        IVsSearchQuery searchQuery,
        IVsSearchCallback searchCallback,
        DataverseExplorerWindow toolWindow)
        : VsSearchTask(cookie, searchQuery, searchCallback)
    {
        private readonly ExplorerCollectionSession? _workspace = toolWindow._viewModel?.Collection;

        protected override void OnStartSearch()
        {
            ErrorCode = VSConstants.S_OK;
            try
            {
                var resultCount = ThreadHelper.JoinableTaskFactory.Run(async () =>
                {
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    if (!ReferenceEquals(_workspace, toolWindow._viewModel?.Collection)) return 0u;
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
