namespace XrmTools.WebApi.Entities;

using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using XrmTools.WebApi.Entities.Attributes;
using XrmTools.WebApi.Types;

[KnownType(typeof(MultiSelectPicklistAttributeMetadata))]
[KnownType(typeof(EntityNameAttributeMetadata))]
[KnownType(typeof(PicklistAttributeMetadata))]
[KnownType(typeof(StateAttributeMetadata))]
[KnownType(typeof(StatusAttributeMetadata))]
[EntityMetadata("EnumAttributeMetadata", "EnumAttributeDefinitions")]
public abstract class EnumAttributeMetadata : AttributeMetadata
{
    public int? DefaultFormValue { get; set; }

    public OptionSetMetadata? OptionSet { get; set; }

    [JsonPropertyOrder(91)]
    internal string? ParentEnumAttributeLogicalName { get; set; }

    protected EnumAttributeMetadata() { }

    protected EnumAttributeMetadata(AttributeTypeCode attributeType, string? schemaName) : base(attributeType, schemaName) { }
}