#nullable enable
namespace XrmTools.WebApi.Entities;

using XrmTools.WebApi.Entities.Attributes;

[EntityMetadata("EntityMetadata", "EntityDefinitions")]
public class EntityMetadata(string LogicalName, string EntitySetName)
{
    public int? ActivityTypeMask { get; set; }
    public string LogicalName { get; set; } = LogicalName;
    public string EntitySetName { get; set; } = EntitySetName;
    public string? DisplayName { get; set; }
    public string? Description { get; set; }
    public bool IsCustomEntity { get; set; }
    public bool IsIntersect { get; set; }
    public bool IsValidForAdvancedFind { get; set; }
    public bool IsValidForQueue { get; set; }
    public bool IsValidForFormAssistant { get; set; }
    //public AttributeMetadata[] Attributes { get; set; } = [];
}
#nullable restore