namespace XrmTools.WebApi.Entities;

using System.Text.Json.Serialization;
using XrmTools.WebApi.Entities.Attributes;
using XrmTools.WebApi.Types;

[EntityMetadata("BooleanAttributeMetadata", "BooleanAttributeDefinitions")]
public sealed class BooleanAttributeMetadata : AttributeMetadata
{
    public bool? DefaultValue { get; set; }

    /// <summary>
    /// String representing the formula of a calculated field.
    /// </summary>
    [JsonPropertyOrder(70)]
    public string? FormulaDefinition { get; set; }

    /// <summary>
    /// Indicates the type of attributes present in the Calculated Field (i.e. persistent,
    /// logical, related, calculated, invalid or any combination of these types)
    /// </summary>
    [JsonPropertyOrder(70)]
    public int? SourceTypeMask { get; internal set; }

    public BooleanOptionSetMetadata? OptionSet { get; set; }

    public BooleanAttributeMetadata()
        : this(null, null)
    {
    }

    public BooleanAttributeMetadata(string schemaName)
        : this(schemaName, null)
    {
    }

    public BooleanAttributeMetadata(BooleanOptionSetMetadata optionSet)
        : this(null, optionSet)
    {
    }

    public BooleanAttributeMetadata(string? schemaName, BooleanOptionSetMetadata? optionSet)
        : base(AttributeTypeCode.Boolean, schemaName)
    {
        OptionSet = optionSet;
    }
}
