namespace XrmTools.WebApi.Entities;

#nullable enable
internal record EntityMetadata(string LogicalName, string EntitySetName);
//public class EntityMetadata
//{
//    public string LogicalName { get; set; } = string.Empty;
//    public string EntitySetName { get; set; } = string.Empty;
//    public string? DisplayName { get; set; }
//    public string? Description { get; set; }
//    public bool IsCustomEntity { get; set; }
//    public bool IsIntersect { get; set; }
//    public bool IsValidForAdvancedFind { get; set; }
//    public bool IsValidForQueue { get; set; }
//    public bool IsValidForFormAssistant { get; set; }

//    public EntityMetadata(string logicalName, string sntitySetName)
//    {
//        LogicalName = logicalName;
//        EntitySetName = sntitySetName;
//    }
//}
#nullable restore