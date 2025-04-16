#nullable enable
namespace XrmTools.WebApi.Entities;

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using XrmTools.Meta.Model;
using XrmTools.WebApi.Entities.Attributes;

[EntityMetadata("sdkmessageprocessingstep", "sdkmessageprocessingsteps")]
internal class SdkMessageProcessingStep : Component<SdkMessageProcessingStep>
{
    [JsonPropertyName("sdkmessageprocessingstepid")]
    [JsonProperty("sdkmessageprocessingstepid")]
    public override Guid? Id { get; set; }
    public int Mode { get; set; }
    public bool CanUseReadonlyConnection { get; set; }
    public int? SupportedDeployment { get; set; }
    public bool? EnablePluginProfiler { get; set; }
    public int? StateCode { get; set; }
    public object? EventExpander { get; set; }
    public string? Name { get; set; }
    public string? Configuration { get; set; }
    public bool? CanBeBypassed { get; set; }
    public string? IntroducedVersion { get; set; }
    public int? VersionNumber { get; set; }
    public bool? AsyncAutoDelete { get; set; }
    public int? Rank { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public Stages? Stage { get; set; }
    public int? StatusCode { get; set; }
    public string? FilteringAttributes { get; set; }
    public string? RuntimeIntegrationProperties { get; set; }
    public ManagedBooleanProperty? IsCustomizable { get; set; }
    public ManagedBooleanProperty? IsHidden { get; set; }
    [JsonPropertyName("sdkmessageprocessingstepid_sdkmessageprocessingstepimage")]
    public List<SdkMessageProcessingStepImage> Images { get; set; } = [];
}
#nullable restore