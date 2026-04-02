#nullable enable
namespace XrmTools.WebApi.Messages;

using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;


/// <summary>
/// Contains the data from the RetrieveRequest
/// </summary>
public sealed class RetrieveResponse
{
    private RetrieveResponse(HttpStatusCode statusCode, IReadOnlyDictionary<string, IEnumerable<string>> headers, JObject record)
    {
        StatusCode = statusCode;
        Headers = headers;
        Record = record;
    }

    public HttpStatusCode StatusCode { get; }

    public IReadOnlyDictionary<string, IEnumerable<string>> Headers { get; }

    public JObject Record { get; }

    internal static async Task<RetrieveResponse> FromAsync(HttpResponseMessage raw, CancellationToken ct = default)
        => new(raw.StatusCode, raw.Headers.ToHeaderDictionary(), await raw.Content.ReadRootAsync().ConfigureAwait(false));
}

public sealed class RetrieveResponse<T>
{
    private RetrieveResponse(HttpStatusCode statusCode, IReadOnlyDictionary<string, IEnumerable<string>> headers, T? record)
    {
        StatusCode = statusCode;
        Headers = headers;
        Record = record;
    }

    public HttpStatusCode StatusCode { get; }

    public IReadOnlyDictionary<string, IEnumerable<string>> Headers { get; }

    /// <summary>
    /// The record returned.
    /// </summary>
    public T? Record { get; }

    internal static async Task<RetrieveResponse<T>> FromAsync(HttpResponseMessage raw, CancellationToken ct = default)
    {
        using var stream = await raw.Content.ReadAsStreamAsync().ConfigureAwait(false);
        var record = await JsonSerializer.DeserializeAsync<T>(stream, global::XrmTools.WebApi.Extensions.SerializerOptions, ct).ConfigureAwait(false);
        return new RetrieveResponse<T>(raw.StatusCode, raw.Headers.ToHeaderDictionary(), record);
    }
}
#nullable restore