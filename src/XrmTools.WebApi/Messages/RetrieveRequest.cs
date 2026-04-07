#nullable enable
namespace XrmTools.WebApi.Messages;

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;


/// <summary>
/// Contains the data to retrieve a record
/// </summary>
public sealed class RetrieveRequest : WebApiRequest<RetrieveResponse>
{
    /// <summary>
    /// Initializes the RetrieveRequest
    /// </summary>
    /// <param name="entityReference">A reference to the record to retrieve</param>
    /// <param name="query">The query parameters to determine the data to return.</param>
    /// <param name="includeAnnotations">Whether to include annotations in the response.</param>
    /// <param name="eTag">The current ETag value to compare.</param>
    public RetrieveRequest(
        EntityReference entityReference, 
        //string entitySetName,
        string? query, bool includeAnnotations = false, string? eTag = null, string? partitionid = null)
    {
        Method = HttpMethod.Get;

        RequestUri = new Uri(
            uriString: BuildUri(entityReference, query, partitionid),
            //uriString: $"{entitySetName}{parameters}",
            uriKind: UriKind.Relative);
        if (includeAnnotations)
        {
            Headers.Add("Prefer", "odata.include-annotations=\"*\"");
        }
        if (eTag != null)
        {
            // Don't return record if it is the same on the server
            Headers.Add("If-None-Match", eTag);
        }
    }

    public override Task<RetrieveResponse> CreateResponseAsync(HttpResponseMessage raw, CancellationToken ct)
        => RetrieveResponse.FromAsync(raw, ct);

    private static string BuildUri(EntityReference entityReference, string? query, string? partitionId)
    {
        var parameters = string.IsNullOrWhiteSpace(query) ? string.Empty : query;

        if (string.IsNullOrWhiteSpace(partitionId))
        {
            return $"{entityReference.Path}{parameters}";
        }

        if (!string.IsNullOrEmpty(parameters) && parameters.StartsWith("?", StringComparison.Ordinal))
        {
            parameters = string.Concat("&", parameters[1..]);
        }
        else if (!string.IsNullOrEmpty(parameters) && !parameters.StartsWith("&", StringComparison.Ordinal))
        {
            parameters = string.Concat("&", parameters);
        }

        return $"{entityReference.Path}?partitionId={partitionId}{parameters}";
    }
}

public sealed class RetrieveRequest<T> : WebApiRequest<RetrieveResponse<T>>
{
    private readonly RetrieveRequest inner;

    public RetrieveRequest(
        EntityReference entityReference,
        string? query,
        bool includeAnnotations = false,
        string? eTag = null,
        string? partitionid = null)
    {
        inner = new RetrieveRequest(entityReference, query, includeAnnotations, eTag, partitionid);
        Method = inner.Method;
        RequestUri = inner.RequestUri;
        foreach (var header in inner.Headers)
        {
            Headers.TryAddWithoutValidation(header.Key, header.Value);
        }
    }

    public override Task<RetrieveResponse<T>> CreateResponseAsync(HttpResponseMessage raw, CancellationToken ct)
        => RetrieveResponse<T>.FromAsync(raw, ct);
}
#nullable restore