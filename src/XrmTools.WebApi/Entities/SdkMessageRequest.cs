namespace XrmTools.WebApi.Entities;

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using XrmTools.WebApi.Entities.Attributes;

[EntityMetadata("sdkmessagerequest", "sdkmessagerequests")]
public class SdkMessageRequest : Component<SdkMessageRequest>
{
    [JsonPropertyName("sdkmessagerequestid")]
    [JsonProperty("sdkmessagerequestid")]
    public override Guid? Id { get; set; }
    public int CustomozationLevel { get; set; } = 0;
    public string? Name { get; set; }
    public string? PrimaryObjectTypeCode { get; set; }
    [JsonProperty("messagerequest_sdkmessagerequestfield"), JsonPropertyName("messagerequest_sdkmessagerequestfield")]
    public List<SdkMessageRequestField> Fields { get; set; } = [];
    [JsonProperty("messagerequest_sdkmessageresponse"), JsonPropertyName("messagerequest_sdkmessageresponse")]
    public List<SdkMessageResponse> Response { get; set; } = [];
}
