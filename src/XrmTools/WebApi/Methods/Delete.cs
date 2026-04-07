#nullable enable
namespace XrmTools.WebApi.Methods;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.WebApi.Messages;

internal static partial class Extensions
{
    /// <summary>
    /// Deletes a record.
    /// </summary>
    /// <param name="service">The Service</param>
    /// <param name="entityReference">A reference to the record to delete.</param>
    /// <param name="partitionId">The partition key to use.</param>
    /// <param name="strongConsistency">Whether strong consistency should be applied.</param>
    /// <param name="eTag">The current ETag value to compare.</param>
    /// <returns></returns>
    public static async Task DeleteAsync(
        this IWebApiService service,
        EntityReference entityReference, 
        string? partitionId = null, 
        bool strongConsistency = false, 
        string? eTag = null,
        CancellationToken cancellationToken = default)
    {
        DeleteRequest request = new(
            entityReference: entityReference,
            partitionId: partitionId,
            strongConsistency: strongConsistency,
            eTag:eTag);

        await service.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}
#nullable restore