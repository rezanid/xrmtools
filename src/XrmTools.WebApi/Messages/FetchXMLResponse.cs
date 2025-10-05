#nullable enable
namespace XrmTools.WebApi.Messages;

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Contains the response from the FetchXmlRequest
/// </summary>
public sealed class FetchXmlResponse
{
    // Keep what you need from the HTTP layer (status/headers) without holding the raw message.
    public HttpStatusCode StatusCode { get; private set; }
    public IReadOnlyDictionary<string, IEnumerable<string>> Headers { get; private set; }

    /// <summary>The records returned.</summary>
    public JArray Records { get; private set; } = [];
    /// <summary>How many records returned.</summary>
    public int? Count { get; private set; }
    /// <summary>A paging cookie value for subsequent requests (when IncludeAnnotations is true).</summary>
    public string? FetchxmlPagingCookie { get; private set; }  // empty string when missing
    /// <summary>
    /// Total records matching the filter (up to 5000), irrespective of page size (when IncludeAnnotations is true).
    /// </summary>
    public int? TotalRecordCount { get; private set; }
    /// <summary>Whether the total record count limit was exceeded.</summary>
    public bool TotalRecordCountExceeded { get; private set; }
    /// <summary>Whether more records match the query filter.</summary>
    public bool MoreRecords { get; private set; }

    // Private ctor enforces initialization of required non-nullable members
    private FetchXmlResponse(HttpStatusCode statusCode, IReadOnlyDictionary<string, IEnumerable<string>> headers)
    {
        StatusCode = statusCode;
        Headers = headers;
    }

    public static async Task<FetchXmlResponse> FromAsync(HttpResponseMessage raw, CancellationToken ct = default)
    {
        if (raw == null) throw new ArgumentNullException(nameof(raw));

        if (!raw.IsSuccessStatusCode)
        {
            var error = await raw.AsServiceExceptionAsync().ConfigureAwait(false);
            throw error;
        }

        var headers = raw.Headers.ToDictionary(h => h.Key, h => h.Value, StringComparer.OrdinalIgnoreCase);

        var root = raw.Content != null
            ? await raw.Content.ReadRootAsync().ConfigureAwait(false)
            : new JObject();

        var resp = new FetchXmlResponse(raw.StatusCode, headers)
        {
            // Safe, cached lookups (prefer non-null defaults for ergonomics)
            Records = (root["value"] as JArray) ?? [],
            Count = (int?)root["@odata.count"],
            FetchxmlPagingCookie = (string?)root["@Microsoft.Dynamics.CRM.fetchxmlpagingcookie"] ?? string.Empty,
            TotalRecordCount = (int?)root["@Microsoft.Dynamics.CRM.totalrecordcount"],
            TotalRecordCountExceeded =
                (bool?)root["@Microsoft.Dynamics.CRM.totalrecordcountlimitexceeded"] ?? false,
            MoreRecords = (bool?)root["@Microsoft.Dynamics.CRM.morerecords"] ?? false
        };

        return resp;
    }
}
#nullable restore