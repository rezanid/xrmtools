namespace XrmTools.WebApi.Entities;
using System;
using System.Text.Json.Serialization;
using XrmTools.WebApi.Entities.Attributes;
using XrmTools.WebApi.Types;

[EntityMetadata("DateTimeAttributeMetadata", "DateTimeAttributeDefinitions")]
public sealed class DateTimeAttributeMetadata : AttributeMetadata
{
    private static readonly DateTime _minDateTime = new(1753, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private static readonly DateTime _maxDateTime = new(9999, 12, 30, 23, 59, 59, DateTimeKind.Utc);

    public static DateTime MinSupportedValue => _minDateTime;

    public static DateTime MaxSupportedValue => _maxDateTime;

    /// <summary>
    /// Required for CreateAttribute Valid for UpdateAttribute
    /// </summary>
    public DateTimeFormat? Format { get; set; }

    /// <summary>
    /// Valid for CreateAttribute Valid for UpdateAttribute
    /// </summary>
    public ImeMode? ImeMode { get; set; }

    /// <summary>
    /// Indicates the type of attributes present in the Calculated Field (i.e. persistent,
    /// logical, related, calculated, invalid or any combination of these types)
    /// </summary>
    [JsonPropertyOrder(70)]
    public int? SourceTypeMask { get; internal set; }

    /// <summary>
    /// String representing the formula of a calculated field.
    /// </summary>
    [JsonPropertyOrder(70)]
    public string? FormulaDefinition { get; set; }

    /// <summary>
    /// Valid for CreateAttribute. Valid for UpdateAttribute.
    /// </summary>
    [JsonPropertyOrder(71)]
    public DateTimeBehavior? DateTimeBehavior { get; set; }

    [JsonPropertyOrder(71)]
    public ManagedBooleanProperty? CanChangeDateTimeBehavior { get; set; }

    public DateTimeAttributeMetadata() : this(null) { }

    public DateTimeAttributeMetadata(DateTimeFormat? format) : this(format, null) { }

    public DateTimeAttributeMetadata(DateTimeFormat? format, string? schemaName) : base(AttributeTypeCode.DateTime, schemaName)
    {
        Format = format;
    }
}
