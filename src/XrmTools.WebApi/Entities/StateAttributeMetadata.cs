namespace XrmTools.WebApi.Entities;

using XrmTools.WebApi.Entities.Attributes;
using XrmTools.WebApi.Types;

[EntityMetadata("StateAttributeMetadata", "StateAttributeDefinitions")]
public sealed class StateAttributeMetadata : EnumAttributeMetadata
{
    public StateAttributeMetadata() : base(AttributeTypeCode.State, null) { }
}
