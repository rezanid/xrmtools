namespace XrmTools.WebApi.Entities;
using System.Text.Json.Serialization;
using XrmTools.WebApi.Entities.Attributes;
using XrmTools.WebApi.Types;

[EntityMetadata("IntegerAttributeMetadata", "IntegerAttributeDefinitions")]
public sealed class IntegerAttributeMetadata : AttributeMetadata
{
    public const int MinSupportedValue = int.MinValue;

    public const int MaxSupportedValue = int.MaxValue;

    /// <summary>
    /// Required for CreateAttribute Ignored for UpdateAttribute
    /// </summary>
    public IntegerFormat? Format { get; set; }

    //
    // Summary:
    /// <summary>
    /// Required for CreateAttribute Valid for UpdateAttribute
    /// </summary>
    public int? MaxValue { get; set; }

    /// <summary>
    /// Required for CreateAttribute Valid for UpdateAttribute
    /// </summary>
    public int? MinValue { get; set; }

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

    public IntegerAttributeMetadata() : this(null) { }

    public IntegerAttributeMetadata(string? schemaName) : base(AttributeTypeCode.Integer, schemaName) { }
}