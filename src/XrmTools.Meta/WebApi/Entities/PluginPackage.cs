#nullable enable
namespace XrmTools.WebApi.Entities;

using Newtonsoft.Json;
using System;
using System.Text.Json.Serialization;
using XrmTools.WebApi.Entities;
using XrmTools.WebApi.Entities.Attributes;

[EntityMetadata("pluginpackage", "pluginpackages")]
public class PluginPackage : Component<PluginPackage>
{
    public string? Name { get; set; }
    public string? Content { get; set; }
    [JsonPropertyName("pluginpackageid")]
    [JsonProperty("pluginpackageid")]
    public override Guid? Id { get; set; }
    public string? Version { get; set; }
}
#nullable restore