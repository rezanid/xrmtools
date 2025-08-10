namespace XrmTools.WebApi.Entities;

using System.Collections.Generic;
using System.Text.Json.Serialization;
using XrmTools.WebApi.Entities.Attributes;
using XrmTools.WebApi.Types;

[EntityMetadata("PicklistAttributeMetadata", "PicklistAttributeDefinitions")]
public sealed class PicklistAttributeMetadata : EnumAttributeMetadata
{
    //
    // Summary:
    //     String representing the formula of a calculated field.
    [JsonPropertyOrder(70)]
    public string? FormulaDefinition { get; set; }

    //
    // Summary:
    //     Indicates the type of attributes present in the Calculated Field (i.e. persistent,
    //     logical, related, calculated, invalid or any combination of these types)
    [JsonPropertyOrder(70)]
    public int? SourceTypeMask { get; set; }

    //
    // Summary:
    //     Gets or sets parent picklist attribute's logical name.
    [JsonPropertyOrder(91)]
    public string? ParentPicklistLogicalName
    {
        get => ParentEnumAttributeLogicalName; 
        set => ParentEnumAttributeLogicalName = value;
    }

    //
    // Summary:
    //     Gets list (logical names) of a picklist attribute's child attributes
    [JsonPropertyOrder(91)]
    public List<string> ChildPicklistLogicalNames { get; set; } = [];

    public PicklistAttributeMetadata()
        : this(null)
    {
    }

    public PicklistAttributeMetadata(string? schemaName)
        : base(AttributeTypeCode.Picklist, schemaName)
    {
    }
}
