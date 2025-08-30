namespace XrmTools.WebApi.Entities;

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using XrmTools.WebApi.Entities.Attributes;

[EntityMetadata("sdkmessagepair", "sdkmessagepairs")]
public class SdkMessagePair : Component<SdkMessagePair>
{
    [JsonPropertyName("sdkmessagepairid")]
    [JsonProperty("sdkmessagepairid")]
    public override Guid? Id { get; set; }
    /// <summary>
    /// Example: "2011/Organization.svc" or "api/data"
    /// </summary>
    public string? Endpoint { get; set; }

    public int CustomizationLevel { get; set; } = 0;
    public string? SdkMessageBindingInformation { get; set; }
    public string? DeprecatedVersion { get; set; }
    /// <summary>
    /// Some examples: 
    /// "http://schemas.microsoft.com/xrm/7.1/Contracts"
    /// "http://schemas.microsoft.com/crm/2011/Contracts"
    /// "http://schemas.microsoft.com/crm/2007/WebServices"
    /// "http://schemas.microsoft.com/xrm/2011/new/"
    /// "http://schemas.microsoft.com/xrm/2011/msdyn/"
    /// "http://schemas.microsoft.com/xrm/2011//"
    /// "http://schemas.microsoft.com/xrm/2011/adx/"
    /// "http://schemas.microsoft.com/xrm/2011/msdynmkt/"
    /// </summary>
    public string? Namespace { get; set; }
    [JsonProperty("messagepair_sdkmessagerequest"), JsonPropertyName("messagepair_sdkmessagerequest")]
    public List<SdkMessageRequest> Request { get; set; } = [];

}
