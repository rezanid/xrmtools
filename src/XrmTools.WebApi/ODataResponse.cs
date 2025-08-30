#nullable enable
namespace XrmTools.WebApi;

using Newtonsoft.Json;
using System.Text.Json.Serialization;

public class ODataResponse
{
    [JsonProperty("@odata.context"), JsonPropertyName("@odata.context")]
    public string Context { get; set; } = null!;
}
#nullable restore