#nullable enable
namespace XrmTools.DataverseExplorer.Services;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.DataverseExplorer.Models;

/// <summary>
/// Service for loading artifact data from Dataverse and building the explorer tree.
/// Loaded state is owned by nodes in the current explorer session.
/// </summary>
internal interface IExplorerDataService
{
    /// <summary>
    /// Loads standalone assemblies, or the assemblies belonging to the specified package.
    /// </summary>
    Task<IEnumerable<AssemblyNode>> LoadAssembliesAsync(CancellationToken cancellationToken, Guid? packageId = null);

    Task<IEnumerable<PackageNode>> LoadPackagesAsync(CancellationToken cancellationToken);

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

}

#nullable restore
