#nullable enable
namespace XrmTools.WebApi.Messages;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Contains the data to create and retrieve a record.
/// </summary>
public sealed class CreateRetrieveRequest : WebApiRequest<CreateRetrieveResponse>
{

    /// <summary>
    /// Initializes a CreateRetrieveRequest
    /// </summary>
    /// <param name="entitySetName">The name of the entity set</param>
    /// <param name="record">The record to create.</param>
    /// <param name="query">The query for data to return.</param>
    /// <param name="includeAnnotations">Whether the results should include annotations</param>
    public CreateRetrieveRequest(string entitySetName, JObject record, string? query, bool includeAnnotations = false)
    {
        Method = HttpMethod.Post;
        RequestUri = new Uri(uriString: $"{entitySetName}{query ?? string.Empty}", uriKind: UriKind.Relative);
        Content = new StringContent(
            content: record.ToString(Formatting.None),
            encoding: System.Text.Encoding.UTF8,
            mediaType: "application/json");
        if (includeAnnotations)
        {
            Headers.Add("Prefer", "odata.include-annotations=\"*\"");
        }
        Headers.Add("Prefer", "return=representation");
    }

    public override Task<CreateRetrieveResponse> CreateResponseAsync(HttpResponseMessage raw, CancellationToken ct)
        => CreateRetrieveResponse.FromAsync(raw, ct);
}
#nullable restore