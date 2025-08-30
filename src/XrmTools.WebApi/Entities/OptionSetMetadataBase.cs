namespace XrmTools.WebApi.Entities;

using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using XrmTools.WebApi.Entities.Attributes;
using XrmTools.WebApi.Types;

[KnownType(typeof(OptionSetMetadata))]
[KnownType(typeof(BooleanOptionSetMetadata))]
[EntityMetadata("OptionSetMetadataBase", "GlobalOptionSetDefinitions")]
public abstract class OptionSetMetadataBase : MetadataBase
{
    public required Label Description { get; set; }

    public required Label DisplayName { get; set; }

    public bool? IsCustomOptionSet { get; set; }

    public bool? IsGlobal { get; set; }

    public bool? IsManaged { get; internal set; }

    public required ManagedBooleanProperty IsCustomizable { get; set; }

    //[Alternatekey]
    public required string Name { get; set; }

    public required string ExternalTypeName { get; set; }

    public OptionSetType? OptionSetType { get; set; }

    [JsonPropertyOrder(60)]
    public required string IntroducedVersion { get; set; }
}