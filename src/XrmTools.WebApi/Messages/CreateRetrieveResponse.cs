#nullable enable
namespace XrmTools.WebApi.Messages;

using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Contains the data from the a CreateRetrieveRequest
/// </summary>
public sealed class CreateRetrieveResponse
{
    private CreateRetrieveResponse(HttpStatusCode statusCode, IReadOnlyDictionary<string, IEnumerable<string>> headers, JObject record)
    {
        StatusCode = statusCode;
        Headers = headers;
        Record = record;
    }

    public HttpStatusCode StatusCode { get; }

    public IReadOnlyDictionary<string, IEnumerable<string>> Headers { get; }

    /// <summary>
    /// The record created.
    /// </summary>
    public JObject Record { get; }

    internal static async Task<CreateRetrieveResponse> FromAsync(HttpResponseMessage raw, CancellationToken ct = default)
        => new(raw.StatusCode, raw.Headers.ToHeaderDictionary(), await raw.Content.ReadRootAsync().ConfigureAwait(false));
}
#nullable restore