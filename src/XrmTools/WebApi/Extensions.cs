#nullable enable
namespace XrmTools.WebApi;

using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using XrmTools.Meta.Model;
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
}
#nullable restore