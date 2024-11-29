namespace XrmTools.Meta.Model;

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.Json.Serialization;

public class ODataQueryResponse<T> : ODataResponse
{
    [JsonProperty("@odata.nextLink")]
    [JsonPropertyName("@odata.nextLink")]
    public string NextLink { get; set; }
    [JsonProperty("value")]
    [JsonPropertyName("value")]
    public List<T> Entities { get; set; } = [];
}
