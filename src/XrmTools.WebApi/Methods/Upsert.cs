#nullable enable
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using XrmTools.WebApi.Messages;

namespace XrmTools.WebApi.Methods;

internal static partial class Extensions
{
    /// <summary>
    /// Performs an Upsert operation on a record
    /// </summary>
    /// <param name="service">The Service</param>
    /// <param name="entityReference">A reference to the record to upsert.</param>
    /// <param name="record">The data for the record.</param>
    /// <param name="upsertBehavior">Controls whether to block Create or Update operations.</param>
    /// <returns></returns>
    public static async Task<EntityReference> UpsertAsync(
        this WebApiService service, 
        EntityReference entityReference, 
        JObject record, 
        UpsertBehavior upsertBehavior)
    {

        UpsertRequest request = new(
            entityReference: entityReference, 
            record: record, 
            upsertBehavior:upsertBehavior);

        try
        {
            UpsertResponse response = await service.SendAsync<UpsertResponse>(request: request);
            return response.EntityReference;
        }
        catch (Exception)
        {
            throw;
        }
    }
}
#nullable restore