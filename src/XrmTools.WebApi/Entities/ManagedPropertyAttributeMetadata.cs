namespace XrmTools.WebApi.Entities;
using XrmTools.WebApi.Entities.Attributes;
using XrmTools.WebApi.Types;

[EntityMetadata("ManagedPropertyAttributeMetadata", "ManagedPropertyAttributeDefinitions")]
public sealed class ManagedPropertyAttributeMetadata : AttributeMetadata
{
    public const int EmptyParentComponentType = 0;

    public string? ManagedPropertyLogicalName { get; internal set; }

    public int? ParentComponentType { get; internal set; }

    public string? ParentAttributeName { get; internal set; }

    public AttributeTypeCode ValueAttributeTypeCode { get; internal set; }

    public ManagedPropertyAttributeMetadata() : this(null) { }

    public ManagedPropertyAttributeMetadata(string? schemaName) : base(AttributeTypeCode.ManagedProperty, schemaName) { }
}
