#nullable enable
namespace XrmTools.Meta.Attributes;
using System;
using XrmTools.Meta.Model;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public class ImageAttribute(ImageTypes type, string name, string attributes) : Attribute
{
    public ImageTypes Type { get; set; } = type;
    public string Name { get; set; } = name;
    public string Attributes { get; set; } = attributes;

    public Guid? Id { get; set; }
    public string? Description { get; set; }
    public string? EntityAlias { get; set; }
    public string? MessagePropertyName { get; set; }

    public ImageAttribute(ImageTypes type, string attributes) : this(type, type.ToString(), attributes) { }
}