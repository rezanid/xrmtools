#nullable enable
namespace XrmTools.WebApi.Entities;

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using XrmTools.Meta.Attributes;
using XrmTools.WebApi.Types;
using XrmTools.WebApi.Entities.Attributes;

[EntityMetadata("sdkmessageprocessingstep", "sdkmessageprocessingsteps")]
public class SdkMessageProcessingStep : Component<SdkMessageProcessingStep>
{
    public enum InvocationSources
    {
        Internal = -1,
        Parent = 0,
        Child = 1,
    }

    [JsonPropertyName("sdkmessageprocessingstepid")]
    [JsonProperty("sdkmessageprocessingstepid")]
    public override Guid? Id { get; set; }
    public ExecutionMode? Mode { get; set; }
    public bool CanUseReadonlyConnection { get; set; }
    public SupportedDeployments? SupportedDeployment { get; set; }
    public bool? EnablePluginProfiler { get; set; }
    public int? StateCode { get; set; }
    public object? EventExpander { get; set; }
    public string? Name { get; set; }
    public string? Configuration { get; set; }
    public bool? CanBeBypassed { get; set; }
    public string? IntroducedVersion { get; set; }
    public int? VersionNumber { get; set; }
    public bool AsyncAutoDelete { get; set; } = false;
    /// <summary>
    /// Execution order of the step in the pipeline. Lower numbers are executed first.
    /// </summary>
    public int? Rank { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public Stages? Stage { get; set; }
    public int? StatusCode { get; set; }
    public string? FilteringAttributes { get; set; }
    public InvocationSources? InvocationSource { get; set; }
    public string? RuntimeIntegrationProperties { get; set; }
    public ManagedBooleanProperty? IsCustomizable { get; set; }
    public ManagedBooleanProperty? IsHidden { get; set; }
    [JsonPropertyName("sdkmessageprocessingstepid_sdkmessageprocessingstepimage")]
    public List<SdkMessageProcessingStepImage> Images { get; set; } = [];
    [JsonPropertyName("sdkmessagefilterid")]
    public SdkMessageFilter? SdkMessageFilter { get; set; }
    [JsonPropertyName("sdkmessageid")]
    public SdkMessage? Message { get; set; }
    //[JsonPropertyName("impersonatinguserid")]
    //public SystemUser? ImpersonatingUser { get; set; }
}
#nullable restore