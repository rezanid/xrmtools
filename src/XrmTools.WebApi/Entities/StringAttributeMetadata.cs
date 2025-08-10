namespace XrmTools.WebApi.Entities;
using System.Text.Json.Serialization;
using XrmTools.WebApi.Entities.Attributes;
using XrmTools.WebApi.Types;

[EntityMetadata("StringAttributeMetadata", "StringAttributeDefinitions")]
public sealed class StringAttributeMetadata : AttributeMetadata
{
    public const int MinSupportedLength = 1;

    public const int MaxSupportedLength = 4000;

    //
    // Summary:
    //     Gets or sets required for CreateAttribute. Ignored for UpdateAttribute. Use FormatName
    //     instead of Format
    public StringFormat? Format { get; set; }

    //
    // Summary:
    //     Gets or sets valid for CreateAttribute. Valid for UpdateAttribute.
    [JsonPropertyOrder(60)]
    public StringFormatName? FormatName { get; set; }

    //
    // Summary:
    //     Gets or sets valid for CreateAttribute. Valid for UpdateAttribute.
    public ImeMode? ImeMode { get; set; }

    /// <summary>
    /// Gets or sets required for CreateAttribu;te Valid for UpdateAttribute
    /// </summary>
    public int? MaxLength { get; set; }

    /// <summary>
    /// Gets or sets required for CreateAttribute Valid for UpdateAttribute
    /// </summary>
    public string? YomiOf { get; set; }

    /// <summary>
    /// Gets valid on RetrieveEntityRequest Valid on RetrieveAllEntitiesRequest Valid
    /// on RetrieveAttributeRequest Valid on RetrieveMetadataChanges
    /// </summary>
    [JsonPropertyOrder(70)]
    public bool? IsLocalizable { get; internal set; }

    [JsonPropertyOrder(90)]
    public int? DatabaseLength { get; internal set; }

    /// <summary>
    /// Gets or sets string representing the formula of a calculated field.
    /// </summary>
    [JsonPropertyOrder(70)]
    public string? FormulaDefinition { get; set; }

    /// <summary>
    /// Gets indicates the type of attributes present in the Calculated Field (i.e. persistent,
    /// logical, related, calculated, invalid or any combination of these types)
    /// </summary>
    [JsonPropertyOrder(70)]
    public int? SourceTypeMask { get; internal set; }

    public StringAttributeMetadata() : this(null) { }

    public StringAttributeMetadata(string? schemaName) : base(AttributeTypeCode.String, schemaName) { }
}