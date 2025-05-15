#nullable enable
namespace XrmTools.Meta.Model;

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.Json.Serialization;

public class ODataQueryResponse<T> : ODataResponse
{
    /// <summary>
    /// A link to the next page of records, if any.
    /// </summary>
    [JsonProperty("@odata.nextLink")]
    [JsonPropertyName("@odata.nextLink")]
    public string? NextLink { get; set; }

    /// <summary>
    /// The records returned.
    /// </summary>
    [JsonProperty("value")]
    [JsonPropertyName("value")]
    public List<T> Value { get; set; } = [];

    /// <summary>
    /// How many records returned. Only populated if '$count=true' is included in the request.queryUri
    /// </summary>
    [JsonProperty("@odata.count")]
    [JsonPropertyName("@odata.count")]
    public int? Count { get; set; }

    /// <summary>
    /// The total number of records matching the filter criteria, up to 5000, irrespective of the page size. Only populated if request.IncludeAnnotations is true.
    /// </summary>
    [JsonProperty("@Microsoft.Dynamics.CRM.totalrecordcount")]
    [JsonPropertyName("@Microsoft.Dynamics.CRM.totalrecordcount")]
    public int? TotalRecordCount { get; set; }

    /// <summary>
    /// Whether the total number of records matching the filter criteria exceeds the TotalRecordCount. Only populated if '$count=true' is included in the request.queryUri
    /// </summary>
    [JsonProperty("@Microsoft.Dynamics.CRM.totalrecordcountlimitexceeded")]
    [JsonPropertyName("@Microsoft.Dynamics.CRM.totalrecordcountlimitexceeded")]
    public bool TotalRecordCountExceeded { get; set; }
}
#nullable restore