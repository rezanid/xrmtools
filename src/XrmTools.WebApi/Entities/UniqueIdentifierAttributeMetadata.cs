namespace XrmTools.WebApi.Entities;
using XrmTools.WebApi.Entities.Attributes;
using XrmTools.WebApi.Types;

[EntityMetadata("UniqueIdentifierAttributeMetadata", "UniqueIdentifierAttributeDefinitions")]
public sealed class UniqueIdentifierAttributeMetadata : AttributeMetadata
{
    public UniqueIdentifierAttributeMetadata() : this(null) { }

    public UniqueIdentifierAttributeMetadata(string? schemaName) : base(AttributeTypeCode.Uniqueidentifier, schemaName) { }
}
