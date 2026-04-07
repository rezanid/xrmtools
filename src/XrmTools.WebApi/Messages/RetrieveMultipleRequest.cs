#nullable enable
namespace XrmTools.WebApi.Messages;

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Contains the data to retrieve data from Dataverse for a specified table.
/// </summary>
public sealed class RetrieveMultipleRequest : WebApiRequest<RetrieveMultipleResponse>
{
    /*
    This message does not provide for an explicit entityset name like RetrieveRequest does.
    This is because the caller should include the entityset name as part of the query.
    This allows this method to work with: 
        - simple entity collections, such as 'accounts'
        - collection valued navigation properties, such as 'accounts(guid)/Account_Tasks'
        - Full Urls, such as those in the NextLink property.
        - Metadata resources such as EntityDefinitions
    */

    /// <summary>
    /// Initializes the RetrieveMultipleRequest
    /// </summary>
    /// <param name="queryUri">An absolute or relative Uri</param>
    /// <param name="maxPageSize">The page size.</param>
    /// <param name="includeAnnotations">Whether to include annotations in the response.</param>
    public RetrieveMultipleRequest(string queryUri, int? maxPageSize = null, bool includeAnnotations = false)
    {
        Method = HttpMethod.Get;

        if (queryUri.StartsWith("http"))
        {
            RequestUri = new Uri(queryUri, UriKind.Absolute);
        }
        else
        {
            RequestUri = new Uri(queryUri, UriKind.Relative);
        }

        if (maxPageSize.HasValue)
        {
            if (maxPageSize > 0 && maxPageSize < 5000)
            {
                Headers.Add("Prefer", $"odata.maxpagesize={maxPageSize.Value}");
            }
            else
            {
                throw new ArgumentOutOfRangeException("MaxPageSize must be greater than 0 and less than 5000.");
            }
        }
        

        if (includeAnnotations)
        {
            Headers.Add("Prefer", "odata.include-annotations=\"*\"");
        }
    }

    public override Task<RetrieveMultipleResponse> CreateResponseAsync(HttpResponseMessage raw, CancellationToken ct)
        => RetrieveMultipleResponse.FromAsync(raw, ct);
}

public sealed class RetrieveMultipleRequest<T> : WebApiRequest<ODataQueryResponse<T>>
{
    private readonly RetrieveMultipleRequest inner;

    public RetrieveMultipleRequest(string queryUri, int? maxPageSize = null, bool includeAnnotations = false)
    {
        inner = new RetrieveMultipleRequest(queryUri, maxPageSize, includeAnnotations);
        Method = inner.Method;
        RequestUri = inner.RequestUri;
        foreach (var header in inner.Headers)
        {
            Headers.TryAddWithoutValidation(header.Key, header.Value);
        }
    }

    public override Task<ODataQueryResponse<T>> CreateResponseAsync(HttpResponseMessage raw, CancellationToken ct)
        => raw.CastAsync<ODataQueryResponse<T>>();
}
#nullable restore