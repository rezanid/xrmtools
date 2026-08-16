#nullable enable
namespace XrmTools.WebApi;

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using XrmTools.Core.Helpers;

public static partial class Extensions
{
    internal static JsonSerializerOptions SerializerOptions { get; } = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };

    [Obsolete("Use typed WebApiRequest<TResponse> responses instead.")]
    /// <summary>
    /// Converts HttpResponseMessage to derived type
    /// </summary>
    /// <typeparam name="T">The type derived from HttpResponseMessage</typeparam>
    /// <param name="response">The HttpResponseMessage</param>
    /// <returns></returns>
    public static T As<T>(this HttpResponseMessage response) where T : HttpResponseMessage
    {
        T? typedResponse = (T?)Activator.CreateInstance(typeof(T))
            ?? throw new InvalidOperationException($"Unable to create {typeof(T).Name}.");

        //Copy the properties
        typedResponse.StatusCode = response.StatusCode;
        response.Headers.ToList().ForEach(h => {
            typedResponse.Headers.TryAddWithoutValidation(h.Key, h.Value);
        });
        typedResponse.Content = response.Content;
        return typedResponse;
    }

    public static async Task<T> CastAsync<T>(this HttpResponseMessage response) where T : ODataResponse
    {
        using var content = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        return content.Deserialize<T>() ??
            throw new InvalidOperationException($"Unable to deserialize {typeof(T).Name}");
    }

    internal static IReadOnlyDictionary<string, IEnumerable<string>> ToHeaderDictionary(this HttpHeaders headers)
        => headers.ToDictionary(header => header.Key, header => header.Value, StringComparer.OrdinalIgnoreCase);

    internal static string? GetHeaderValue(this HttpHeaders headers, string headerName)
        => headers.TryGetValues(headerName, out var values) ? values.FirstOrDefault() : null;

    internal static string? GetHeaderValue(this IReadOnlyDictionary<string, IEnumerable<string>> headers, string headerName)
        => headers.TryGetValue(headerName, out var values) ? values.FirstOrDefault() : null;

    public static EntityReference? GetEntityReference(this HttpResponseMessage response)
    {
        var value = response.Headers.GetHeaderValue("OData-EntityId");
        return string.IsNullOrWhiteSpace(value) ? null : new EntityReference(value!);
    }

    public static async Task<ServiceException> AsServiceExceptionAsync(this HttpResponseMessage response)
    {
        var requestId = response.Headers.GetHeaderValue("REQ_ID") ?? string.Empty;

        var content = response.Content is null
            ? string.Empty
            : await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        ODataError? oDataError = null;

        try
        {
            oDataError = JsonSerializer.Deserialize<ODataError>(content, SerializerOptions);
        }
        catch (Exception)
        {
            // Error may not be in correct OData Error format, so keep trying...
        }

        if (oDataError != null && oDataError.Error != null)
        {
            return CreateServiceException(
                message: oDataError.Error.Message ?? response.ReasonPhrase,
                response: response,
                requestId: requestId,
                content: content,
                oDataError: oDataError);
        }

        try
        {
            var oDataException = JsonSerializer.Deserialize<ODataException>(content, SerializerOptions);
            var message = oDataException?.Message ?? oDataException?.ExceptionMessage ?? response.ReasonPhrase;

            return CreateServiceException(message, response, requestId, content);
        }
        catch (Exception)
        {
        }

        return CreateServiceException(response.ReasonPhrase, response, requestId, content);
    }

    private static ServiceException CreateServiceException(
        string? message,
        HttpResponseMessage response,
        string requestId,
        string content,
        ODataError? oDataError = null)
        => new(message ?? response.ReasonPhrase ?? $"HTTP {(int)response.StatusCode}")
        {
            ODataError = oDataError,
            Content = content,
            ReasonPhrase = response.ReasonPhrase,
            HttpStatusCode = response.StatusCode,
            RequestId = requestId
        };

    public static async Task<JObject> ReadRootAsync(this HttpContent? content)
    {
        if (content == null) return new JObject();

        try
        {
            await content.LoadIntoBufferAsync().ConfigureAwait(false);
        }
        catch
        {
            // Not fatal; we'll still attempt to read.
        }

        var json = await content.ReadAsStringAsync().ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(json)) return new JObject();

        try { return JObject.Parse(json); }
        catch { return new JObject(); }
    }
}
#nullable restore