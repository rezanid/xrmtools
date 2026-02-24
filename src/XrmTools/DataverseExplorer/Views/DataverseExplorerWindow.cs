#nullable enable
namespace XrmTools.DataverseExplorer.Views;

using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using XrmTools.DataverseExplorer.Services;
using XrmTools.DataverseExplorer.ViewModels;
using XrmTools.Logging.Compatibility;

public class DataverseExplorerSource
{
    public required ILogger Logger { get; init; }
    public required IExplorerDataService DataService { get; init; }

}

[Guid(WindowGuidString)]
public class DataverseExplorerWindow : ToolWindowPane // BaseToolWindow<DataverseExplorerWindow>
{
    public const string WindowGuidString = "12345678-1234-1234-1234-123456789012";
    public const string WindowCaption = "Dataverse Explorer";

    private DataverseExplorerViewModel? _viewModel;
    
    public ILogger Logger { get; set; }

    public IExplorerDataService DataService { get; set; }

    public DataverseExplorerWindow(DataverseExplorerSource source) : base()
    {
        BitmapImageMoniker = KnownMonikers.Search;
        Caption = "Dataverse Explorer";

        Logger = source.Logger;
        DataService = source.DataService;

        var control = new DataverseExplorerWindowControl();
        _viewModel = new DataverseExplorerViewModel(DataService, Logger);
        control.DataContext = _viewModel;
        Content = control;
    }

    //[ImportingConstructor]
    //public DataverseExplorerWindow(IExplorerDataService dataService, ILogger logger) : this()
    //{
    //    var control = new DataverseExplorerWindowControl();
    //    _viewModel = new DataverseExplorerViewModel(dataService, logger);
    //    control.DataContext = _viewModel;
    //    Content = control;
    //}

    protected override void OnCreate()
    {
        base.OnCreate();
        _ = InitializeViewModelAsync();
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
