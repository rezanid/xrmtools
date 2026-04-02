#nullable enable
namespace XrmTools.WebApi.Methods;

using System.Threading;
using System.Threading.Tasks;
using XrmTools.WebApi.Messages;

internal static partial class Extensions
{
    /// <summary>
    /// Sets the value of a column for a table row
    /// </summary>
    /// <typeparam name="T">The type of value</typeparam>
    /// <param name="service">The service</param>
    /// <param name="entityReference">A reference to the table row.</param>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="value">The value to set</param>
    /// <returns></returns>
    public static async Task SetColumnValueAsync<T>(
        this IWebApiService service,
        EntityReference entityReference,
        string propertyName,
        T value,
        CancellationToken cancellationToken = default)
    {
        SetColumnValueRequest<T> request = new(entityReference, propertyName, value);

        await service.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}
#nullable restore