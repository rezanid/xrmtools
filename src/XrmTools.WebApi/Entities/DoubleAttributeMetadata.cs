namespace XrmTools.WebApi.Entities;

using System.Text.Json.Serialization;
using XrmTools.WebApi.Entities.Attributes;
using XrmTools.WebApi.Types;

[EntityMetadata("DoubleAttributeMetadata", "DoubleAttributeDefinitions")]
public sealed class DoubleAttributeMetadata : AttributeMetadata
{
    public const double MinSupportedValue = -100000000000.0;

    public const double MaxSupportedValue = 100000000000.0;

    public const int MinSupportedPrecision = 0;

    public const int MaxSupportedPrecision = 5;

    /// <summary>
    /// Valid for CreateAttribute Valid for UpdateAttribute
    /// </summary>
    public ImeMode? ImeMode { get; set; }

    /// <summary>
    /// Required for CreateAttribute Valid for UpdateAttribute
    /// </summary>
    public double? MaxValue { get; set; }

    /// <summary>
    /// Required for CreateAttribute Valid for UpdateAttribute
    /// </summary>
    public double? MinValue { get; set; }

    /// <summary>
    /// Required for CreateAttribute Valid for UpdateAttribute
    /// </summary>
    public int? Precision { get; set; }

    /// <summary>
    /// String representing the formula of a calculated field.
    /// </summary>
    [JsonPropertyOrder(70)]
    public string? FormulaDefinition { get; set; }

    /// <summary>
    /// Indicates the type of attributes present in the Formula Field (i.e. persistent,
    /// logical, related, calculated, invalid or any combination of these types)
    /// </summary>
    [JsonPropertyOrder(70)]
    public int? SourceTypeMask { get; internal set; }

    public DoubleAttributeMetadata() : this(null) { }

    public DoubleAttributeMetadata(string? schemaName) : base(AttributeTypeCode.Double, schemaName) { }
}