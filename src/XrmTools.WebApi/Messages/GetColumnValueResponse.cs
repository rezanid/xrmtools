namespace XrmTools.WebApi.Messages;

using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Contains the data from the GetColumnValueRequest.
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class GetColumnValueResponse<T>
{
    private GetColumnValueResponse(HttpStatusCode statusCode, IReadOnlyDictionary<string, IEnumerable<string>> headers, T? value)
    {
        StatusCode = statusCode;
        Headers = headers;
        Value = value;
    }

    public HttpStatusCode StatusCode { get; }

    public IReadOnlyDictionary<string, IEnumerable<string>> Headers { get; }

    /// <summary>
    /// The requested typed column  value.
    /// </summary>
    public T? Value { get; }

    internal static async Task<GetColumnValueResponse<T>> FromAsync(HttpResponseMessage raw, CancellationToken ct = default)
    {
        using var document = raw.Content is null
            ? null
            : JsonDocument.Parse(await raw.Content.ReadAsStringAsync().ConfigureAwait(false));

        var value = document is not null && document.RootElement.TryGetProperty("value", out var element)
            ? JsonSerializer.Deserialize<T>(element.GetRawText(), Extensions.SerializerOptions)
            : default;

        return new GetColumnValueResponse<T>(raw.StatusCode, raw.Headers.ToHeaderDictionary(), value);
    }
}