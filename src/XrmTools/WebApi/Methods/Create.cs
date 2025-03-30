#nullable enable
namespace XrmTools.WebApi.Methods;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using XrmTools.WebApi.Messages;

internal static partial class Extensions
{
    /// <summary>
    /// Creates a record
    /// </summary>
    /// <param name="service">The Service</param>
    /// <param name="entitySetName">The EntitySetName for the table</param>
    /// <param name="record">Contains the data to create the record.</param>
    /// <returns>A reference to the created record.</returns>
    public static async Task<EntityReference> CreateAsync(
        this WebApiService service, 
        string entitySetName, 
        JObject record) {

        var request = new CreateRequest(entitySetName: entitySetName, record:record);

        try
        {
            var response = await service.SendAsync<CreateResponse>(request: request);

            return response.EntityReference;

        }
        catch (Exception)
        {
            throw;
        }
    }
}
#nullable restore