#nullable enable
namespace XrmTools.WebApi.Entities;
using Newtonsoft.Json;
using System;
using System.Text.Json.Serialization;
using XrmTools.WebApi.Entities.Attributes;

[EntityMetadata("sdkmessagefilter", "sdkmessagefilters")]
public class SdkMessageFilter : Component<SdkMessageFilter>
{
    [JsonProperty("sdkmessagefilterid"), JsonPropertyName("sdkmessagefilterid")]
    public override Guid? Id { get; set; }

    [JsonProperty("primaryobjecttypecode"), JsonPropertyName("primaryobjecttypecode")]
    public string? PrimaryObjectTypeCode { get; set; }

    [JsonProperty("iscustomprocessingstepallowed"), JsonPropertyName("iscustomprocessingstepallowed")]
    public bool? IsCustomProcessingStepAllowed { get; set; }

    [JsonProperty("sdkmessageid"), JsonPropertyName("sdkmessageid")]
    public Guid? SdkMessageId { get; set; }
}
#nullable restore