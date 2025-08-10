namespace XrmTools.WebApi.Entities;

using System.Text.Json.Serialization;
using XrmTools.WebApi.Entities.Attributes;
using XrmTools.WebApi.Types;

[EntityMetadata("MoneyAttributeMetadata", "MoneyAttributeDefinitions")]
public sealed class MoneyAttributeMetadata : AttributeMetadata
{
    public const double MinSupportedValue = -922337203685477.0;

    public const double MaxSupportedValue = 922337203685477.0;

    public const int MinSupportedPrecision = 0;

    public const int MaxSupportedPrecision = 4;

    public const int MaxSupportedPrecisionAfterCurrencyConversion = 10;

    /// <summary>
    /// Required for CreateAttribute Valid for UpdateAttribute
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
    /// Required for CreateAttribute Valid for UpdateAttribute
    /// </summary>
    public int? PrecisionSource { get; set; }

    public string? CalculationOf { get; set; }

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

    [JsonPropertyOrder(70)]
    public bool? IsBaseCurrency { get; internal set; }

    public MoneyAttributeMetadata() : this(null) { }

    public MoneyAttributeMetadata(string? schemaName) : base(AttributeTypeCode.Money, schemaName) { }
}