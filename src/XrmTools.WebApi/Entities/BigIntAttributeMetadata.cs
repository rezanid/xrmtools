namespace XrmTools.WebApi.Entities;

using XrmTools.WebApi.Entities.Attributes;
using XrmTools.WebApi.Types;

[EntityMetadata("BigIntAttributeMetadata", "BigIntAttributeDefinitions")]
public sealed class BigIntAttributeMetadata : AttributeMetadata
{
    public const long MinSupportedValue = long.MinValue;

    public const long MaxSupportedValue = long.MaxValue;

    /// <summary>
    /// Required for CreateAttribute Valid for UpdateAttribute
    /// </summary>
    public long? MaxValue { get; internal set; }

    /// <summary>
    /// Required for CreateAttribute Valid for UpdateAttribute
    /// </summary>
    public long? MinValue { get; internal set; }

    public BigIntAttributeMetadata() : this(null) { }

    public BigIntAttributeMetadata(string? schemaName) : base(AttributeTypeCode.BigInt, schemaName) { }
}