#nullable enable
namespace XrmTools.WebApi.Messages;

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Contains the data to retrieve a column value
/// </summary>
public sealed class GetColumnValueRequest<T> : WebApiRequest<GetColumnValueResponse<T>>
{
    /// <summary>
    /// Initializes the GetColumnValueRequest
    /// </summary>
    /// <param name="entityReference">A reference to the record to get the column data from.</param>
    /// <param name="property">The name of the column.</param>
    public GetColumnValueRequest(EntityReference entityReference, string property)
    {
        Method = HttpMethod.Get;
        RequestUri = new Uri(
            uriString: $"{entityReference.Path}/{property}", 
            uriKind: UriKind.Relative);
    }

    public override Task<GetColumnValueResponse<T>> CreateResponseAsync(HttpResponseMessage raw, CancellationToken ct)
        => GetColumnValueResponse<T>.FromAsync(raw, ct);
}
#nullable restore