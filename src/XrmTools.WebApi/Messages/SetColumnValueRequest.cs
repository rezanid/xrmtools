#nullable enable
namespace XrmTools.WebApi.Messages;

using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Contains the data to set a column value
/// </summary>
/// <typeparam name="T">The type of the column value to set</typeparam>
public sealed class SetColumnValueRequest<T> : WebApiRequest<EmptyResponse>
{
    /// <summary>
    /// Initializes the SetColumnValueRequest
    /// </summary>
    /// <param name="entityReference">A reference to the record that has the column.</param>
    /// <param name="propertyName">The name of the column</param>
    /// <param name="value">The value to set</param>
    public SetColumnValueRequest(EntityReference entityReference, string propertyName, T value)
    {
        Method = HttpMethod.Put;
        RequestUri = new Uri(
            uriString: $"{entityReference.Path}/{propertyName}", 
            uriKind: UriKind.Relative);
        Content = new StringContent(
            content: $"{{\"value\": {JsonSerializer.Serialize(value, Extensions.SerializerOptions)}}}", 
            encoding: Encoding.UTF8, 
            mediaType: "application/json");
    }

    public override Task<EmptyResponse> CreateResponseAsync(HttpResponseMessage raw, CancellationToken ct)
        => Task.FromResult(EmptyResponse.Instance);
}
#nullable restore