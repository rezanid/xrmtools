namespace XrmTools.WebApi.Messages;

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Contains the response from the UpsertRequest
/// </summary>
public sealed class UpsertResponse
{
    private UpsertResponse(HttpStatusCode statusCode, IReadOnlyDictionary<string, IEnumerable<string>> headers)
    {
        StatusCode = statusCode;
        Headers = headers;
        EntityReference = headers.GetHeaderValue("OData-EntityId") is string entityId && !string.IsNullOrWhiteSpace(entityId)
            ? new EntityReference(entityId)
            : null;
    }

    public HttpStatusCode StatusCode { get; }

    public IReadOnlyDictionary<string, IEnumerable<string>> Headers { get; }

    /// <summary>
    /// A reference to the record.
    /// </summary>
    public EntityReference? EntityReference { get; }

    internal static Task<UpsertResponse> FromAsync(HttpResponseMessage raw, CancellationToken ct = default)
        => Task.FromResult(new UpsertResponse(raw.StatusCode, raw.Headers.ToHeaderDictionary()));
}