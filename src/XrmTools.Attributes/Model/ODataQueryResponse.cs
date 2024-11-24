namespace XrmTools.Meta.Model;

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.Json.Serialization;

public class ODataQueryResponse<T>
{
    [JsonProperty("@odata.nextLink")]
    [JsonPropertyName("@odata.nextLink")]
    public string NextLink { get; set; }
    [JsonProperty("@odata.context")]
    [JsonPropertyName("@odata.context")]
    public string Context { get; set; }
    [JsonProperty("value")]
    [JsonPropertyName("value")]
    public List<T> Entities { get; set; } = [];
}
