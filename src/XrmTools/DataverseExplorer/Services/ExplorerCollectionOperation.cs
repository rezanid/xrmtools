#nullable enable
namespace XrmTools.DataverseExplorer.Services;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.DataverseExplorer.Models;

/// <summary>Domain behavior for a collection task; interaction belongs to the shared session.</summary>
internal interface IExplorerCollectionOperation
{
    string Title { get; }
    string ActionLabel { get; }
    bool SupportsScope(ExplorerNodeBase scope);
    Task<IReadOnlyList<ExplorerCollectionCandidate>> LoadAsync(ExplorerNodeBase scope, Uri environment, CancellationToken token);
    Task ExecuteAsync(IReadOnlyList<ExplorerCollectionCandidate> selection, Uri environment, CancellationToken token);
}

/// <summary>Optional companion documentation output, enabled independently for each session.</summary>
internal interface IExplorerCollectionDocumentationOperation : IExplorerCollectionOperation
{
    Task ExecuteWithDocumentationAsync(IReadOnlyList<ExplorerCollectionCandidate> selection, Uri environment, CancellationToken token);
}

internal sealed record ExplorerCollectionCandidate(string Id, string DisplayName, string UniqueName,
    string GroupId, string GroupName, object Target, DateTimeOffset? ModifiedOn = null);
