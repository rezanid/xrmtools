#nullable enable
namespace XrmTools.WebApi.Methods;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.WebApi.Messages;

internal static partial class Extensions
{
    /// <summary>
    /// Creates a record and retrieves it.
    /// </summary>
    /// <param name="service">The Service</param>
    /// <param name="entitySetName">The EntitySetName for the table</param>
    /// <param name="record">Contains the data to create the record.</param>
    /// <param name="query">The query string parameters</param>
    /// <param name="includeAnnotations">Whether to include annotations with the data.</param>
    /// <returns>The created record.</returns>
    public static async Task<JObject> CreateRetrieveAsync(
        this IWebApiService service,
        string entitySetName,
        JObject record,
        string? query,
        bool includeAnnotations = false,
        CancellationToken cancellationToken = default)
    {
        CreateRetrieveRequest request = new(
            entitySetName: entitySetName, 
            record: record,
            query: query,
            includeAnnotations: includeAnnotations);

        CreateRetrieveResponse response = await service.SendAsync(request, cancellationToken).ConfigureAwait(false);
        return response.Record;
    }
}
#nullable restore