#nullable enable
namespace XrmTools.WebApi.Methods;

using System.Threading;
using System.Threading.Tasks;
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
        bool includeAnnotations = false,
        CancellationToken cancellationToken = default)
    {
        var request = new RetrieveMultipleRequest(
            queryUri: queryUri,
            maxPageSize: maxPageSize,
            includeAnnotations: includeAnnotations);

        return await service.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }

    public static async Task<ODataQueryResponse<T>> RetrieveMultipleAsync<T>(
        this IWebApiService service,
        string queryUri,
        int? maxPageSize = null,
        bool includeAnnotations = false,
        CancellationToken cancellationToken = default)
    {
        var request = new RetrieveMultipleRequest<T>(
            queryUri: queryUri,
            maxPageSize: maxPageSize,
            includeAnnotations: includeAnnotations);

        return await service.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}
#nullable restore