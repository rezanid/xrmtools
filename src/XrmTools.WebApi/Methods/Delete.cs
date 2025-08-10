#nullable enable
namespace XrmTools.WebApi.Methods;
using System;
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
        this WebApiService service,
        EntityReference entityReference, 
        string? partitionId = null, 
        bool strongConsistency = false, 
        string? eTag = null)
    {
        DeleteRequest request = new(
            entityReference: entityReference,
            partitionId: partitionId,
            strongConsistency: strongConsistency,
            eTag:eTag);

        try
        {
            await service.SendAsync(request: request);
        }
        catch (Exception)
        {
            throw;
        }
    }
}
#nullable restore