namespace XrmTools.WebApi.Entities;

using System.Collections.ObjectModel;
using XrmTools.WebApi.Entities.Attributes;
using XrmTools.WebApi.Types;

[EntityMetadata("OneToManyRelationshipMetadata", "OneToManyRelationshipDefinitions")]
public sealed class OneToManyRelationshipMetadata : RelationshipMetadataBase
{
    public AssociatedMenuConfiguration? AssociatedMenuConfiguration { get; set; }

    public CascadeConfiguration? CascadeConfiguration { get; set; }

    public string? ReferencedAttribute { get; set; }

    public string? ReferencedEntity { get; set; }

    public string? ReferencingAttribute { get; set; }

    public string? ReferencingEntity { get; set; }

    public bool? IsHierarchical { get; set; }

    public string? EntityKey { get; set; }

    public Collection<RelationshipAttribute> RelationshipAttributes { get; set; } = [];

    public bool? IsRelationshipAttributeDenormalized { get; set; }

    public string? ReferencedEntityNavigationPropertyName { get; set; }

    public string? ReferencingEntityNavigationPropertyName { get; set; }

    public int? RelationshipBehavior { get; set; }

    public bool? IsDenormalizedLookup { get; set; }

    public string? DenormalizedAttributeName { get; set; }

    public OneToManyRelationshipMetadata() : base(RelationshipType.OneToManyRelationship) { }
}
