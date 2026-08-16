#nullable enable
namespace XrmTools.WebApi.Messages;

using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Contains the response from the RetrieveMultipleRequest
/// </summary>
public sealed class RetrieveMultipleResponse
{        
    private RetrieveMultipleResponse(HttpStatusCode statusCode, IReadOnlyDictionary<string, IEnumerable<string>> headers, JObject root)
    {
        StatusCode = statusCode;
        Headers = headers;
        Records = (JArray?)root.GetValue("value") ?? [];
        Count = (int?)root.GetValue("@odata.count");
        TotalRecordCount = (int?)root.GetValue("@Microsoft.Dynamics.CRM.totalrecordcount");
        TotalRecordCountExceeded = root.GetValue("@Microsoft.Dynamics.CRM.totalrecordcountlimitexceeded")?.ToString() == "True";
        NextLink = root.GetValue("@odata.nextLink")?.ToString();
    }

    public HttpStatusCode StatusCode { get; }

    public IReadOnlyDictionary<string, IEnumerable<string>> Headers { get; }

    /// <summary>
    /// The records returned.
    /// </summary>
    public JArray Records { get; }

    /// <summary>
    /// How many records returned. Only populated if '$count=true' is included in the request.queryUri
    /// </summary>
    public int? Count { get; }

    /// <summary>
    /// The total number of records matching the filter criteria, up to 5000, irrespective of the page size. Only populated if request.IncludeAnnotations is true.
    /// </summary>
    public int? TotalRecordCount { get; }

    /// <summary>
    /// Whether the total number of records matching the filter criteria exceeds the TotalRecordCount. Only populated if '$count=true' is included in the request.queryUri
    /// </summary>
    public bool TotalRecordCountExceeded { get; }

    /// <summary>
    /// A link to the next page of records, if any.
    /// </summary>
    public string? NextLink { get; }

    internal static async Task<RetrieveMultipleResponse> FromAsync(HttpResponseMessage raw, CancellationToken ct = default)
        => new(raw.StatusCode, raw.Headers.ToHeaderDictionary(), await raw.Content.ReadRootAsync().ConfigureAwait(false));
}
#nullable restore