#nullable enable
namespace XrmTools.Meta.Attributes;

using System;
using XrmTools.Meta.Model;

/// <summary>
/// Generates a response property for a Custom API.
/// </summary>
/// <param name="type"></param>
/// <param name="displayName"></param>
public class CustomApiOutputAttribute(CustomApiFieldType type, string displayName) : Attribute
{
    public string DisplayName { get; set; } = displayName;
    public string Name { get; set; } = displayName.Replace(" ", string.Empty);
    public string UniqueName { get; set; } = displayName.Replace(" ", string.Empty);
    public string? Description { get; set; }
    public CustomApiFieldType Type { get; set; } = type;
    public string? LogicalEntityName { get; set; }
    public CustomApiOutputAttribute(CustomApiFieldType type, string displayName, string name) : this(type, displayName)
    {
        Name = name;
    }
    public CustomApiOutputAttribute(CustomApiFieldType type, string displayName, string name, string uniqueName) : this(type, displayName, name)
    {
        UniqueName = uniqueName;
    }
}
#nullable restore