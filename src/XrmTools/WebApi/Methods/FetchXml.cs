#nullable enable
namespace XrmTools.WebApi.Methods;
using XrmTools.WebApi.Batch;
using XrmTools.WebApi.Messages;
using System.Xml.Linq;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Threading;

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
    public static async Task<FetchXmlResponse?> FetchXmlAsync(
        this IWebApiService service,
        string entitySetName,
        XDocument fetchXml,
        bool includeAnnotations, CancellationToken cancellationToken)
    {
        FetchXmlRequest fetchXmlRequest = new(
            entitySetName: entitySetName,
            fetchXml: fetchXml,
            includeAnnotations);

        return await SendFetchXmlAsync(service, fetchXmlRequest, cancellationToken);
    }

    public static async Task<FetchXmlResponse?> FetchXmlAsync(
        this IWebApiService service,
        string entitySetName,
        string fetchXml,
        bool includeAnnotations,
        CancellationToken cancellationToken)
    {
       FetchXmlRequest fetchXmlRequest = new(
            entitySetName: entitySetName,
            fetchXml: fetchXml,
            includeAnnotations);

        return await SendFetchXmlAsync(service, fetchXmlRequest, cancellationToken);
    }

    private static async Task<FetchXmlResponse?> SendFetchXmlAsync(IWebApiService service, FetchXmlRequest fetchXmlRequest, CancellationToken cancellationToken)
    {
        // Sending the request as a batch to mitigate issues where FetchXml length exceeds the 
        // max length for a URI sent in the query. This way it will be sent in the body.
        var baseUrl = await service.GetBaseUrlAsync();
        if (baseUrl is null) return null;
        BatchRequest batchRequest = new(baseUrl)
        {
            Requests = [fetchXmlRequest]
        };

        try
        {
            var batchResponse = await service.SendAsync<BatchResponse>(batchRequest, cancellationToken).ConfigureAwait(false);

            var firstResponse = (await batchResponse.ParseResponseAsync().ConfigureAwait(false)).FirstOrDefault();

            var fetchXmlResponse = await fetchXmlRequest.CreateResponseAsync(firstResponse, cancellationToken).ConfigureAwait(false);

            return fetchXmlResponse;
        }
        catch (Exception)
        {
            throw;
        }
    }
}
#nullable restore