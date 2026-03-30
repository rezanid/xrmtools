#nullable enable
namespace XrmTools.DataverseExplorer.Models;

using Microsoft.VisualStudio.Imaging.Interop;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

/// <summary>
/// Base class for all tree nodes in the Dataverse Explorer.
/// Supports hierarchical organization for current and future artifact types.
/// </summary>
public abstract class ExplorerNodeBase
{
    [Browsable(false)]
    public string Id { get; set; } = string.Empty;
    [ReadOnly(true)]
    public string DisplayName { get; set; } = string.Empty;
    [ReadOnly(true)]
    public string Description { get; set; } = string.Empty;
    [Browsable(false)]
    public ObservableCollection<ExplorerNodeBase> Children { get; } = [];
    [Browsable(false)]
    public bool IsExpanded { get; set; }
    [Browsable(false)]
    public bool IsLoading { get; set; }
    [Browsable(false)]
    public ExplorerNodeBase? Parent { get; set; }
    [Browsable(false)]
    public ImageMoniker ImageMoniker { get; set; }
    /// <summary>
    /// Indicates whether this node has children that can be loaded lazily.
    /// </summary>
    public abstract bool CanLoadChildren { get; }

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
