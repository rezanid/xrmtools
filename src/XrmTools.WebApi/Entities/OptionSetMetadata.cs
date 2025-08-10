namespace XrmTools.WebApi.Entities;

using System.Collections.Generic;
using System.Text.Json.Serialization;
using XrmTools.WebApi.Entities.Attributes;

[EntityMetadata("OptionSetMetadata", "OptionSetDefinitions")]
public sealed class OptionSetMetadata : OptionSetMetadataBase
{
    public IList<OptionMetadata> Options { get; set; } = [];

    [JsonPropertyOrder(91)]
    public string? ParentOptionSetName { get; set; }
}
