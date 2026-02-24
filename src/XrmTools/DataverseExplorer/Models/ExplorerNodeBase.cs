#nullable enable
namespace XrmTools.DataverseExplorer.Models;

using System;
using System.Collections.ObjectModel;

/// <summary>
/// Base class for all tree nodes in the Dataverse Explorer.
/// Supports hierarchical organization for current and future artifact types.
/// </summary>
public abstract class ExplorerNodeBase
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ObservableCollection<ExplorerNodeBase> Children { get; } = [];
    public bool IsExpanded { get; set; }
    public bool IsLoading { get; set; }
    public ExplorerNodeBase? Parent { get; set; }

    /// <summary>
    /// Indicates whether this node has children that can be loaded lazily.
    /// </summary>
    public abstract bool CanLoadChildren { get; }

    /// <summary>
    /// Returns the artifact type category (e.g., "Assemblies", "Cloud Flows", "Environment Variables").
    /// </summary>
    public abstract string ArtifactCategory { get; }
}

#nullable restore
