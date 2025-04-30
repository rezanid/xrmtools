#nullable enable
namespace XrmTools.WebApi.Entities;

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using XrmTools.WebApi.Entities.Attributes;

[EntityMetadata("pluginassembly", "pluginassemblies")]
internal class PluginAssembly : Component<PluginAssembly>
{

    [JsonPropertyName("pluginassemblyid")]
    [JsonProperty("pluginassemblyid")]
    public override Guid? Id { get; set; }
    public int? Minor { get; set; }
    public string? Path { get; set; }
    public bool? IsPasswordSet { get; set; }
    public string? Culture { get; set; }
    public string? Username { get; set; }
    public int? Sourcetype { get; set; }
    public int? Authtype { get; set; }
    public string? PublicKeyToken { get; set; }
    public string? IntroducedVersion { get; set; }
    public int? Major { get; set; }
    public int? IsolationMode { get; set; }
    public string? Password { get; set; }
    public string? Content { get; set; }
    public string? Name { get; set; }
    public string? Url { get; set; }
    public int? VersionNumber { get; set; }
    public string? Description { get; set; }
    public string? SourceHash { get; set; }
    public ManagedBooleanProperty? IsCustomizable { get; set; }
    public ManagedBooleanProperty? IsHidden { get; set; }
    [JsonPropertyName("pluginassembly_plugintype")]
    public List<PluginType> PluginTypes { get; set; } = [];
}
#nullable restore