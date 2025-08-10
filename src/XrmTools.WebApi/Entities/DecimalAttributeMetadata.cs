namespace XrmTools.WebApi.Entities;

using System.Text.Json.Serialization;
using XrmTools.WebApi.Entities.Attributes;
using XrmTools.WebApi.Types;

[EntityMetadata("DecimalAttributeMetadata", "DecimalAttributeMetadataDefinitions")]
public sealed class DecimalAttributeMetadata : AttributeMetadata
{
    public const double MinSupportedValue = -100000000000.0;

    public const double MaxSupportedValue = 100000000000.0;

    public const int MinSupportedPrecision = 0;

    public const int MaxSupportedPrecision = 10;

    public decimal? MaxValue { get; set; }

    public decimal? MinValue { get; set; }

    public int? Precision { get; set; }

    public ImeMode? ImeMode { get; set; }

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

    public DecimalAttributeMetadata() : this(null) { }

    public DecimalAttributeMetadata(string? schemaName) : base(AttributeTypeCode.Decimal, schemaName) { }
}