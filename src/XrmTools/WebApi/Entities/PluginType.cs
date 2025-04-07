#nullable enable
namespace XrmTools.WebApi.Entities;

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using XrmTools.WebApi.Entities.Attributes;

[EntityMetadata("plugintype", "plugintypes")]
internal class PluginType : Component<PluginType>
{
    public int? Minor { get; set; }
    public object? CustomWorkflowActivityInfo { get; set; }
    public string? WorkflowActivityGroupName { get; set; }
    public string? Culture { get; set; }
    public string? PublicKeyToken { get; set; }
    public int? Major { get; set; }
    public string? TypeName { get; set; }
    public string? PluginTypeExportKey { get; set; }
    public string? FriendlyName { get; set; }
    public string? Name { get; set; }
    [JsonPropertyName("plugintypeid")]
    [JsonProperty("plugintypeid")]
    public override Guid? Id { get; set; }
    public int? VersionNumber { get; set; }
    public bool? IsWorkflowActivity { get; set; }
    public string? AssemblynName { get; set; }
    public string? Description { get; set; }
    [JsonPropertyName("plugintype_sdkmessageprocessingstep")]
    public List<SdkMessageProcessingStep> Steps { get; set; } = [];
}
#nullable restore