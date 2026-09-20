#nullable enable
namespace XrmTools.DataverseExplorer.Models;

using Microsoft.VisualStudio.Imaging.Interop;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using XrmTools.UI;

/// <summary>
/// Base class for all tree nodes in the Dataverse Explorer.
/// Supports hierarchical organization for current and future artifact types.
/// </summary>
public abstract class ExplorerNodeBase : ViewModelBase
{
    private string _displayName = string.Empty;
    private string _description = string.Empty;
    private bool _isExpanded;
    private bool _isLoading;
    private bool _areChildrenLoaded;
    private string? _loadError;
    private ImageMoniker _imageMoniker;

    protected ExplorerNodeBase()
    {
        VisibleChildren = new ListCollectionView(Children) { Filter = item => ((ExplorerNodeBase)item).IsVisible };
    }

    [Browsable(false)]
    public string Id { get; set; } = string.Empty;
    [ReadOnly(true)]
    public string DisplayName { get => _displayName; set => SetProperty(ref _displayName, value); }
    [ReadOnly(true)]
    public string Description { get => _description; set => SetProperty(ref _description, value); }
    [Browsable(false)]
    public ObservableCollection<ExplorerNodeBase> Children { get; } = [];
    [Browsable(false)]
    public bool IsExpanded { get => _isExpanded; set => SetProperty(ref _isExpanded, value); }
    [Browsable(false)]
    public bool IsLoading { get => _isLoading; set => SetProperty(ref _isLoading, value); }
    [Browsable(false)]
    public string? LoadError { get => _loadError; set => SetProperty(ref _loadError, value); }
    [Browsable(false)]
    public bool AreChildrenLoaded
    {
        get => _areChildrenLoaded;
        set
        {
            if (SetProperty(ref _areChildrenLoaded, value)) OnPropertyChanged(nameof(CanLoadChildren));
        }
    }
    [Browsable(false)]
    public bool IsVisible { get; internal set; } = true;
    [Browsable(false)]
    public ICollectionView VisibleChildren { get; }

    // Assigned by the artifact provider, keeping expansion independent of concrete node types.
    internal Func<CancellationToken, Task>? LoadChildrenAsync { get; set; }
    // Root-owned command context: invalidated when this tree is refreshed or its environment changes.
    internal CancellationToken SessionToken { get; set; }
    internal Func<CancellationToken, Task<ExplorerNodeBase?>>? ReloadAsync { get; set; }
    internal Func<Task>? RefreshAsync { get; set; }
    [Browsable(false)]
    public ExplorerNodeBase? Parent { get; set; }
    [Browsable(false)]
    public ImageMoniker ImageMoniker { get => _imageMoniker; set => SetProperty(ref _imageMoniker, value); }
    /// <summary>
    /// Indicates whether this node has children that can be loaded lazily.
    /// </summary>
    public virtual bool CanLoadChildren => LoadChildrenAsync != null && !AreChildrenLoaded;

    /// <summary>
    /// Returns the artifact type category (e.g., "Assemblies", "Tables", "Cloud Flows", "Environment Variables").
    /// </summary>
    public abstract string ArtifactCategory { get; }
}

public abstract class ExplorerNodeBaseWithDates : ExplorerNodeBase
{
    [ReadOnly(true)]
    public DateTimeOffset? CreatedOn { get; set; }
    [ReadOnly(true)]
    public DateTimeOffset? ModifiedOn { get; set; }
}

#nullable restore
