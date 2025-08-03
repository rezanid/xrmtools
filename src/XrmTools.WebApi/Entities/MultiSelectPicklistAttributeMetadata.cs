namespace XrmTools.WebApi.Entities;

using System.Collections.Generic;
using System.Text.Json.Serialization;
using XrmTools.WebApi.Entities.Attributes;
using XrmTools.WebApi.Types;

[EntityMetadata("MultiSelectPicklistAttributeMetadata", "MultiSelectPicklistAttributeDefinitions")]
public sealed class MultiSelectPicklistAttributeMetadata : EnumAttributeMetadata
{
    /// <summary>
    /// String representing the formula of a calculated field.
    /// </summary>
    public string? FormulaDefinition { get; set; }

    /// <summary>
    /// Indicates the type of attributes present in the Calculated Field (i.e. persistent,
    /// logical, related, calculated, invalid or any combination of these types)
    /// </summary>
    public int? SourceTypeMask { get; internal set; }

    /// <summary>
    /// Gets or sets parent picklist attribute's logical name.
    /// </summary>
    [JsonPropertyOrder(91)]
    public string? ParentPicklistLogicalName
    {
        get => ParentEnumAttributeLogicalName;
        set => ParentEnumAttributeLogicalName = value;
    }

    /// <summary>
    /// Gets list (logical names) of a picklist attribute's child attributes
    /// </summary>
    [JsonPropertyOrder(91)]
    public List<string> ChildPicklistLogicalNames { get; set; } = [];

    public MultiSelectPicklistAttributeMetadata() : this(null) { }

    public MultiSelectPicklistAttributeMetadata(string? schemaName)
    {
        AttributeType = AttributeTypeCode.Virtual;
        AttributeTypeName = AttributeTypeDisplayName.MultiSelectPicklistType;
        SchemaName = schemaName;
    }
}