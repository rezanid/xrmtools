#nullable enable
namespace XrmTools.WebApi.Methods;
using XrmTools.WebApi.Batch;
using XrmTools.WebApi.Messages;
using System.Xml.Linq;
using System.Threading.Tasks;
using System;
using System.Linq;

internal static partial class Extensions
{
    /// <summary>
    /// Retrieves the results of a FetchXml query.
    /// </summary>
    /// <param name="service">The service</param>
    /// <param name="entitySetName">The entity set name</param>
    /// <param name="fetchXml">The fetchXml Query</param>
    /// <param name="includeAnnotations">Whether to include annotations with the results.</param>
    /// <returns>FetchXmlResponse</returns>
    public static async Task<FetchXmlResponse> FetchXmlAsync(
        this WebApiService service,
        string entitySetName,
        XDocument fetchXml,
        bool includeAnnotations)
    {
        FetchXmlRequest fetchXmlRequest = new(
            entitySetName: entitySetName,
            fetchXml: fetchXml,
            includeAnnotations);

        // Sending the request as a batch to mitigate issues where FetchXml length exceeds the 
        // max length for a URI sent in the query. This way it will be sent in the body.
        BatchRequest batchRequest = new(await service.GetBaseUrlAsync())
        {
            Requests = [fetchXmlRequest]
        };

        try
        {
            var batchResponse = await service.SendAsync<BatchResponse>(batchRequest);

            var firstResponse = (await batchResponse.ParseResponseAsync()).FirstOrDefault();

            var fetchXmlResponse = firstResponse.As<FetchXmlResponse>();

            return fetchXmlResponse;
        }
        catch (Exception)
        {

            throw;
        }
    }
}
#nullable restore