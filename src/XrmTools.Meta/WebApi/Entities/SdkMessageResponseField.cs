namespace XrmTools.WebApi.Entities;

using Newtonsoft.Json;
using System;
using System.Text.Json.Serialization;
using XrmTools.WebApi.Entities.Attributes;

[EntityMetadata("sdkmessageresponsefield", "sdkmessageresponsefields")]
internal class SdkMessageResponseField : Component<SdkMessageResponseField>
{
    [JsonPropertyName("sdkmessageresponsefieldid")]
    [JsonProperty("sdkmessageresponsefieldid")]
    public override Guid? Id { get; set; }
    public string? ClrFormatter { get; set; } = null;
    public int? CustomizationLevel { get; set; } = 0;
    public int? FieldMask { get; set; } = null;
    public string? Name { get; set; } = null;
    public string? PublicName { get; set; } = null;
    public string? ParameterBindingInformation { get; set; } = null;
    public string? Formatter { get; set; } = null;
    public int Position { get; set; } = 0;
    public string? Value { get; set; }
}
