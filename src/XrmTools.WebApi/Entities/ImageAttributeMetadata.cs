namespace XrmTools.WebApi.Entities;

using XrmTools.WebApi.Entities.Attributes;
using XrmTools.WebApi.Types;

[EntityMetadata("ImageAttributeMetadata", "ImageAttributeDefinitions")]
public sealed class ImageAttributeMetadata : AttributeMetadata
{
    public bool? IsPrimaryImage { get; set; }

    public short? MaxHeight { get; internal set; }

    public short? MaxWidth { get; internal set; }

    /// <summary>
    /// Maximum file size(in KBs) allowed for this attribute
    /// </summary>
    public int? MaxSizeInKB { get; set; }

    /// <summary>
    /// Indicates whether this Image attribute can store full image
    /// </summary>
    public bool? CanStoreFullImage { get; set; }

    public ImageAttributeMetadata() : this(null) { }

    public ImageAttributeMetadata(string? schemaName)
    {
        AttributeType = AttributeTypeCode.Virtual;
        AttributeTypeName = AttributeTypeDisplayName.ImageType;
        SchemaName = schemaName;
    }
}