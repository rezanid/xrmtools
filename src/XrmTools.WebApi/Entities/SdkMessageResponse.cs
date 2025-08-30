namespace XrmTools.WebApi.Entities;

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using XrmTools.WebApi.Entities.Attributes;

[EntityMetadata("sdkmessageresponse", "sdkmessageresponses")]
public class SdkMessageResponse : Component<SdkMessageResponse>
{
    [JsonPropertyName("sdkmessageresponseid")]
    [JsonProperty("sdkmessageresponseid")]
    public override Guid? Id { get; set; }
    public int CustomizationLevel { get; set; } = 0;
    [JsonProperty("messageresponse_sdkmessageresponsefield"), JsonPropertyName("messageresponse_sdkmessageresponsefield")]
    public List<SdkMessageResponseField> Fields { get; set; } = [];
}
