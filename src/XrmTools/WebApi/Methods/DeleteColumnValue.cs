#nullable enable
namespace XrmTools.WebApi.Methods;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.WebApi.Messages;

internal static partial class Extensions
{
    /// <summary>
    /// Deletes the value of a column for a table row
    /// </summary>
    /// <param name="service">The service</param>
    /// <param name="entityReference">A reference to the table row.</param>
    /// <param name="propertyName">The name of the property.</param>
    /// <returns></returns>
    public static async Task DeleteColumnValueAsync(
        this IWebApiService service,
        EntityReference entityReference,
        string propertyName,
        CancellationToken cancellationToken = default)
    {
        DeleteColumnValueRequest request = new(entityReference: entityReference, propertyName: propertyName);

        await service.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}
#nullable restore