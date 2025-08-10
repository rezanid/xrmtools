namespace XrmTools.WebApi.Entities;

using XrmTools.WebApi.Entities.Attributes;
using XrmTools.WebApi.Types;

[EntityMetadata("StatusAttributeMetadata", "StatusAttributeDefinitions")]
public sealed class StatusAttributeMetadata : EnumAttributeMetadata
{
    public StatusAttributeMetadata() : base(AttributeTypeCode.Status, null) { }
}
