#nullable enable
namespace XrmTools.DataverseExplorer.Services;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.DataverseExplorer.Models;

/// <summary>
/// Service for loading artifact data from Dataverse and building the explorer tree.
/// Handles caching and indexing to track what data has been loaded.
/// </summary>
internal interface IExplorerDataService
{
    /// <summary>
    /// Loads all plugin assemblies from Dataverse.
    /// </summary>
    Task<IEnumerable<AssemblyNode>> LoadAssembliesAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Loads plugin types and their steps/images for a given assembly.
    /// </summary>
    Task<IEnumerable<ExplorerNodeBase>> LoadAssemblyChildrenAsync(AssemblyNode assembly, CancellationToken cancellationToken);

    /// <summary>
    /// Loads all Dataverse table definitions.
    /// </summary>
    Task<IEnumerable<TableNode>> LoadTablesAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Loads columns, relationships, keys, forms, and views for a table.
    /// </summary>
    Task<IEnumerable<ExplorerNodeBase>> LoadTableChildrenAsync(TableNode table, CancellationToken cancellationToken);

    /// <summary>
    /// Loads steps and images for a given plugin type.
    /// </summary>
    Task<IEnumerable<ExplorerNodeBase>> LoadPluginTypeChildrenAsync(PluginTypeNode pluginType, CancellationToken cancellationToken);

    /// <summary>
    /// Loads images for a given plugin step.
    /// </summary>
    Task<IEnumerable<PluginImageNode>> LoadPluginStepChildrenAsync(PluginStepNode step, CancellationToken cancellationToken);

    /// <summary>
    /// Clears all cached data, forcing a refresh on next load.
    /// </summary>
    void ClearCache();

    /// <summary>
    /// Searches through loaded nodes by name and description.
    /// </summary>
    IEnumerable<ExplorerNodeBase> Search(string searchTerm, IEnumerable<ExplorerNodeBase> nodes);
}

#nullable restore
