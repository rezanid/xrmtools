#nullable enable
namespace XrmTools.DataverseExplorer.Views;

using System.ComponentModel.Design;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
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
}

#nullable restore
