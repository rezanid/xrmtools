namespace XrmTools.WebApi.Entities;

using System.Text.Json.Serialization;
using XrmTools.WebApi.Entities.Attributes;
using XrmTools.WebApi.Types;

[EntityMetadata("EntityNameAttributeMetadata", "EntityNameAttributeDefinitions")]
public sealed class EntityNameAttributeMetadata : EnumAttributeMetadata
{
    [JsonPropertyOrder(90)]
    public bool IsEntityReferenceStored { get; internal set; }

    public EntityNameAttributeMetadata() : this(null) { }

    public EntityNameAttributeMetadata(string? schemaName) : base(AttributeTypeCode.EntityName, schemaName) { }
}