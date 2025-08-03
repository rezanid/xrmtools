namespace XrmTools.WebApi.Entities;
using XrmTools.WebApi.Entities.Attributes;
using XrmTools.WebApi.Types;

[EntityMetadata("RelationshipMetadataBase", "RelationshipDefinitions")]
public abstract class RelationshipMetadataBase
{
    public bool? IsCustomRelationship { get; set; }

    public ManagedBooleanProperty? IsCustomizable { get; set; }

    public bool? IsValidForAdvancedFind { get; set; }

    // Alternate Key
    public string? SchemaName { get; set; }

    public SecurityTypes? SecurityTypes { get; set; }

    public bool? IsManaged { get; internal set; }

    public RelationshipType RelationshipType { get; internal set; }

    public string? IntroducedVersion { get; internal set; }

    protected RelationshipMetadataBase() { }

    protected RelationshipMetadataBase(RelationshipType type)
    {
        RelationshipType = type;
    }
}
