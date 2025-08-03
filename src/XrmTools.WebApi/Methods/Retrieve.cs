#nullable enable
namespace XrmTools.WebApi.Methods;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using XrmTools.WebApi.Messages;

internal static partial class Extensions
{
    /// <summary>
    /// Retrieves a record.
    /// </summary>
    /// <param name="service">The service.</param>
    /// <param name="entityReference">A reference to the record to retrieve</param>
    /// <param name="query">The query string parameters</param>
    /// <param name="includeAnnotations">Whether to include annotations with the data.</param>
    /// <param name="eTag">The current ETag value to compare.</param>
    /// <returns></returns>
    public static async Task<JObject> RetrieveAsync(
        this IWebApiService service,
        EntityReference entityReference, 
        //string entitySetName,
        string? query, 
        bool includeAnnotations = false,
        string? eTag = null,
        string? partitionId = null)
    {
        var request = new RetrieveRequest(
            entityReference: entityReference,
            //entitySetName,
            query: query,
            includeAnnotations: includeAnnotations,
            eTag: eTag, 
            partitionid:partitionId);

        try
        {
            RetrieveResponse response = await service.SendAsync<RetrieveResponse>(request: request);

            return await response.GetRecordAsync();

        }
        catch (Exception)
        {
            throw;
        }
    }

    public static async Task<T?> RetrieveAsync<T>(
        this IWebApiService service,
        EntityReference entityReference,
        string? query,
        bool includeAnnotations = false,
        string? eTag = null,
        string? partitionId = null)
    {
        RetrieveRequest request = new(
            entityReference: entityReference,
            query: query,
            includeAnnotations: includeAnnotations,
            eTag: eTag,
            partitionid: partitionId);
        try
        {
            var response = await service.SendAsync<RetrieveResponse<T>>(request: request);
            return await response.GetRecordAsync();
        }
        catch (Exception)
        {
            throw;
        }
    }
}
#nullable restore