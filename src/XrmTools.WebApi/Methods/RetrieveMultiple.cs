#nullable enable
namespace XrmTools.WebApi.Methods;

using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.Meta.Model;
using XrmTools.WebApi.Messages;

internal static partial class Extensions
{
    /// <summary>
    /// Retrieves the results of an OData query.
    /// </summary>
    /// <param name="service">The Service.</param>
    /// <param name="queryUri">An absolute or relative Uri.</param>
    /// <param name="maxPageSize">The maximum number of records to return in a page.</param>
    /// <param name="includeAnnotations">Whether to include annotations with the results.</param>
    /// <returns></returns>
    public static async Task<RetrieveMultipleResponse> RetrieveMultipleAsync(
        this IWebApiService service,
        string queryUri,
        int? maxPageSize = null,
        bool includeAnnotations = false)
    {
        var request = new RetrieveMultipleRequest(
            queryUri: queryUri,
            maxPageSize: maxPageSize,
            includeAnnotations: includeAnnotations);
        try
        {
            return await service.SendAsync<RetrieveMultipleResponse>(request: request);
        }
        catch (Exception)
        {
            throw;
        }
    }

    public static async Task<ODataQueryResponse<T>> RetrieveMultipleAsync<T>(
        this IWebApiService service,
        string queryUri,
        int? maxPageSize = null,
        bool includeAnnotations = false)
    {
        var request = new RetrieveMultipleRequest(
            queryUri: queryUri,
            maxPageSize: maxPageSize,
            includeAnnotations: includeAnnotations);
        try
        {
            using var response = await service.SendAsync<HttpResponseMessage>(request: request).ConfigureAwait(false);
            using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            var result =  await JsonSerializer.DeserializeAsync<ODataQueryResponse<T>>(stream, SerializerOptions).ConfigureAwait(false);
            return result!;
        }
        catch (Exception)
        {
            throw;
        }
    }
}
#nullable restore