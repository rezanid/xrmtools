namespace XrmTools.WebApi.Entities;
using XrmTools.WebApi.Entities.Attributes;
using XrmTools.WebApi.Types;

[EntityMetadata("EntityKeyMetadata", "EntityKeyDefinitions")]
public class EntityKeyMetadata
{
    public Label? DisplayName { get; set; }
    public string? LogicalName { get; set; }
    public string? SchemaName { get; set; }
    public string? EntityLogicalName { get; set; }
    public string[]? KeyAttributes { get; set; }
    public ManagedBooleanProperty? IsCustomizable { get; set; }
    public bool? IsManaged { get; set; }
    public string? IntroducedVersion { get; set; }
    public EntityKeyIndexStatus? EntityKeyIndexStatus { get; set; }
    public EntityReference? AsyncJob { get; set; }
    public bool? IsSynchronous { get; set; }
    public bool? IsExportKey { get; set; }
    public bool? IsSecondaryKey { get; set; }
}
