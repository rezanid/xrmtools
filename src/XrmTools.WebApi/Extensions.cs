#nullable enable
namespace XrmTools.WebApi;

using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.Xml;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.Core.Helpers;

public static partial class Extensions
{
    //TODO: Using reflection to copy properties is not the best way to do this. Make sure we switch to CastAsync<T> for all responses.
    /// <summary>
    /// Converts HttpResponseMessage to derived type
    /// </summary>
    /// <typeparam name="T">The type derived from HttpResponseMessage</typeparam>
    /// <param name="response">The HttpResponseMessage</param>
    /// <returns></returns>
    public static T As<T>(this HttpResponseMessage response) where T : HttpResponseMessage
    {
        T? typedResponse = (T)Activator.CreateInstance(typeof(T));

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

    public static async Task<ServiceException> AsServiceExceptionAsync(this HttpResponseMessage response)
    {
        string requestId = string.Empty;
        if (response.Headers.Contains("REQ_ID"))
        {
            requestId = response.Headers.GetValues("REQ_ID").FirstOrDefault();
        }

        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        ODataError? oDataError = null;

        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            oDataError = JsonSerializer.Deserialize<ODataError>(content, options);
        }
        catch (Exception)
        {
            // Error may not be in correct OData Error format, so keep trying...
        }

        if (oDataError != null && oDataError.Error != null)
        {

            var exception = oDataError.Error.Message is string msg ? new ServiceException(msg) : new ServiceException()
            {
                ODataError = oDataError,
                Content = content,
                ReasonPhrase = response.ReasonPhrase,
                HttpStatusCode = response.StatusCode,
                RequestId = requestId
            };
            return exception;
        }
        else
        {
            try
            {
                var oDataException = JsonSerializer.Deserialize<ODataException>(content);

                ServiceException otherException = oDataException?.Message is string msg ? new(msg) : new()
                {
                    Content = content,
                    ReasonPhrase = response.ReasonPhrase,
                    HttpStatusCode = response.StatusCode,
                    RequestId = requestId
                };
                return otherException;

            }
            catch (Exception)
            {

            }

            //When nothing else works
            ServiceException exception = new(response.ReasonPhrase)
            {
                Content = content,
                ReasonPhrase = response.ReasonPhrase,
                HttpStatusCode = response.StatusCode,
                RequestId = requestId
            };
            return exception;
        }
    }

    public static async Task<JObject> ReadRootAsync(this HttpContent content)
    {
        if (content == null) return [];

        // TODO: Buffer once so we can safely dispose the HttpResponseMessage afterwards.
        // If LoadIntoBufferAsync isn’t available, I might need to replace with ReadAsByteArrayAsync().
        try
        {
            await content.LoadIntoBufferAsync().ConfigureAwait(false);
        }
        catch
        {
            // Not fatal; we'll still attempt to read.
        }

        var json = await content.ReadAsStringAsync().ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(json)) return [];

        try { return JObject.Parse(json); }
        catch { return []; }
    }
}
#nullable restore