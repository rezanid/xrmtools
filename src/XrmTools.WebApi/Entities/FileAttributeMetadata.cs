namespace XrmTools.WebApi.Entities;

using System.Runtime.Serialization;
using XrmTools.WebApi.Entities.Attributes;
using XrmTools.WebApi.Types;

[EntityMetadata("FileAttributeMetadata", "FileAttributeDefinitions")]
public sealed class FileAttributeMetadata : AttributeMetadata
{
    /// <summary>
    /// Maximum file size(in KBs) allowed for this attribute
    /// </summary>
    [DataMember]
    public int? MaxSizeInKB { get; set; }

    public FileAttributeMetadata() : this(null) { }

    public FileAttributeMetadata(string? schemaName)
    {
        AttributeType = AttributeTypeCode.Virtual;
        AttributeTypeName = AttributeTypeDisplayName.FileType;
        IsValidForUpdate = false;
        IsValidForCreate = false;
        RequiredLevel = new AttributeRequiredLevelManagedProperty(AttributeRequiredLevel.None);
        SchemaName = schemaName;
    }
}
