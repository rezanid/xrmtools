#nullable enable
namespace XrmTools.Meta.Attributes;
using System;
using XrmTools.Meta.Model;

// Any change in the constructors of this class requires a change in PluginAttributeExtractor.
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public class ImageAttribute(ImageTypes type, string messagePropertyName) : Attribute
{
    public Guid? Id { get; set; }
    /// <summary>
    /// Type of image requested.
    /// </summary>
    public ImageTypes Type { get; set; } = type;
    /// <summary>
    /// Name of the property on the Request message. E.g. "Target".
    /// </summary>
    public string MessagePropertyName { get; set; } = messagePropertyName;

    /// <summary>
    /// Name of SdkMessage processing step image.
    /// </summary>
    public string Name { get; set; } = type.ToString();
    /// <summary>
    /// Key name used to access the pre-image or post-image property bags in a step
    /// </summary>
    public string EntityAlias { get; set; } = type.ToString();
    public string? Attributes { get; set; }
    public string? Description { get; set; }
    public ImageAttribute(ImageTypes type, string messagePropertyName, string attributes) : this(type, messagePropertyName)
    {
        Attributes = attributes;
    }

}