namespace XrmTools.WebApi.Messages;

using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

/// <summary>
/// Contains the data to execute a query using FetchXml
/// </summary>
public sealed class FetchXmlRequest : WebApiRequest<FetchXmlResponse>
{
    /// <summary>
    /// Initializes the FetchXmlRequest
    /// </summary>
    /// <param name="entitySetName">The name of the entity set.</param>
    /// <param name="fetchXml">The document containing the fetchXml</param>
    /// <param name="includeAnnotations">Whether annotations should be included in the response.</param>
    public FetchXmlRequest(string entitySetName, XDocument fetchXml, bool includeAnnotations = false)
        : this(
            entitySetName ?? throw new ArgumentNullException(nameof(entitySetName)),
            (fetchXml ?? throw new ArgumentNullException(nameof(fetchXml))).ToString(SaveOptions.DisableFormatting),
            includeAnnotations)
    { }

    public FetchXmlRequest(string entitySetName, string fetchXml, bool includeAnnotations = false)
    {
        if (string.IsNullOrWhiteSpace(entitySetName))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(entitySetName));
        if (string.IsNullOrWhiteSpace(fetchXml))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(fetchXml));

        Method = HttpMethod.Get;
        RequestUri = new Uri(
            $"{entitySetName}?fetchXml={WebUtility.UrlEncode(fetchXml)}&$count=true",
            UriKind.Relative);

        if (includeAnnotations)
        {
            Headers.Add("Prefer", "odata.include-annotations=\"*\"");
        }
    }

    public override Task<FetchXmlResponse> CreateResponseAsync(HttpResponseMessage raw, CancellationToken ct)
        => FetchXmlResponse.FromAsync(raw, ct);
}