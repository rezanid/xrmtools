#nullable enable
namespace XrmTools.WebApi.Entities;

using Newtonsoft.Json;
using System;
using System.Text.Json.Serialization;
using XrmTools.WebApi.Entities.Attributes;

[EntityMetadata("sdkmessageprocessingstepimage", "sdkmessageprocessingstepimages")]
internal class SdkMessageProcessingStepImage : Component<SdkMessageProcessingStepImage>
{
    [JsonPropertyName("sdkmessageprocessingstepimageid")]
    [JsonProperty("sdkmessageprocessingstepimageid")]
    public override Guid? Id { get; set; }
    public string? Name { get; set; }
    public int? ImageType { get; set; }
    public int? VersionNumber { get; set; }
    public string? IntroducedVersion { get; set; }
    public string? EntityAlias { get; set; }
    public string? Description { get; set; }
    public string? MessagePropertyName { get; set; }
    public string? RelatedAttributeName { get; set; }
    public string? Attributes { get; set; }
    public ManagedBooleanProperty? IsCustomizable { get; set; }
}
#nullable restore