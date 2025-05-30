﻿#nullable enable
namespace XrmTools.WebApi.Methods;
using System;
using System.Threading.Tasks;
using XrmTools.WebApi.Messages;

internal static partial class Extensions
{
    /// <summary>
    /// Retrieves a single column value from a table.
    /// </summary>
    /// <typeparam name="T">The type of the value</typeparam>
    /// <param name="service">The service</param>
    /// <param name="entityReference">A reference to the record.</param>
    /// <param name="property">The name of the column.</param>
    /// <returns></returns>
    public static async Task<T> GetColumnValue<T>(
        this WebApiService service,
        EntityReference entityReference, 
        string property)
    {
        GetColumnValueRequest request = new(
            entityReference: entityReference,
            property: property);

        try
        {
            GetColumnValueResponse<T> response = await service.SendAsync<GetColumnValueResponse<T>>(request: request);

            return response.Value;

        }
        catch (Exception)
        {
            throw;
        }
    }
}
#nullable restore