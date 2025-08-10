namespace XrmTools.WebApi.Entities;
using System.Text.Json.Serialization;
using XrmTools.WebApi.Entities.Attributes;
using XrmTools.WebApi.Types;

[EntityMetadata("MemoAttributeMetadata", "MemoAttributeDefinitions")]
public sealed class MemoAttributeMetadata : AttributeMetadata
{
    public const int MinSupportedLength = 1;

    public const int MaxSupportedLength = 1048576;

    /// <summary>
    /// Valid for CreateAttribute Valid for UpdateAttribute
    /// </summary>
    public StringFormat? Format { get; set; }

    /// <summary>
    /// Valid for UpdateAttribute. Valid for UpdateAttribute.
    /// </summary>
    [JsonPropertyOrder(60)]
    public MemoFormatName? FormatName { get; set; }

    /// <summary>
    /// Valid for CreateAttribute Valid for UpdateAttribute
    /// </summary>
    public ImeMode? ImeMode { get; set; }

    /// <summary>
    /// Required on non-email Memo attributes for CreateAttribute Valid on UpdateAttribute
    /// </summary>
    public int? MaxLength { get; set; }

    /// <summary>
    /// Valid on RetrieveEntityRequest Valid on RetrieveAllEntitiesRequest Valid on RetrieveAttributeRequest
    /// Valid on RetrieveMetadataChanges
    /// </summary>
    [JsonPropertyOrder(70)]
    public bool? IsLocalizable { get; internal set; }

    public MemoAttributeMetadata() : this(null) { }

    public MemoAttributeMetadata(string? schemaName) : base(AttributeTypeCode.Memo, schemaName) { }
}