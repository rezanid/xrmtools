namespace XrmTools.WebApi.Entities;
using XrmTools.WebApi.Entities.Attributes;
using XrmTools.WebApi.Types;

[EntityMetadata("ManyToManyRelationshipMetadata", "ManyToManyRelationshipDefinitions")]
public sealed class ManyToManyRelationshipMetadata : RelationshipMetadataBase
{
    public AssociatedMenuConfiguration? Entity1AssociatedMenuConfiguration { get; set; }

    public AssociatedMenuConfiguration? Entity2AssociatedMenuConfiguration { get; set; }

    public string? Entity1LogicalName { get; set; }

    public string? Entity2LogicalName { get; set; }

    public string? IntersectEntityName { get; set; }

    public string? Entity1IntersectAttribute { get; set; }

    public string? Entity2IntersectAttribute { get; set; }

    public string? Entity1NavigationPropertyName { get; set; }

    public string? Entity2NavigationPropertyName { get; set; }

    public ManyToManyRelationshipMetadata() : base(RelationshipType.ManyToManyRelationship) { }
}
