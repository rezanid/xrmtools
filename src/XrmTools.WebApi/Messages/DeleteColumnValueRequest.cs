#nullable enable
namespace XrmTools.WebApi.Messages;

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Contains the data to delete a column value
/// </summary>
public sealed class DeleteColumnValueRequest : WebApiRequest<EmptyResponse>
{
    /// <summary>
    /// Initializes a DeleteColumnValueRequest
    /// </summary>
    /// <param name="entityReference">A reference to a record that has the property</param>
    /// <param name="propertyName">The name of the property with the value to delete.</param>
    public DeleteColumnValueRequest(EntityReference entityReference, string propertyName)
    {
        Method = HttpMethod.Delete;
        RequestUri = new Uri(
            uriString: $"{entityReference.Path}/{propertyName}",
            uriKind: UriKind.Relative);
    }

    public override Task<EmptyResponse> CreateResponseAsync(HttpResponseMessage raw, CancellationToken ct)
        => Task.FromResult(EmptyResponse.Instance);
}
#nullable restore