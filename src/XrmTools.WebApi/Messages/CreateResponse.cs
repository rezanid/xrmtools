namespace XrmTools.WebApi.Messages;

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Contains the response from the CreateRequest
/// </summary>
public sealed class CreateResponse
{
    private CreateResponse(HttpStatusCode statusCode, IReadOnlyDictionary<string, IEnumerable<string>> headers)
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
    /// A reference to the record created.
    /// </summary>
    public EntityReference? EntityReference { get; }

    internal static Task<CreateResponse> FromAsync(HttpResponseMessage raw, CancellationToken ct = default)
        => Task.FromResult(new CreateResponse(raw.StatusCode, raw.Headers.ToHeaderDictionary()));
}