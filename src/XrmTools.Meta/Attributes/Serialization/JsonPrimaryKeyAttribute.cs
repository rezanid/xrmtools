#nullable enable
namespace XrmTools.Meta.Attributes.Serialization;
using System;

[AttributeUsage(AttributeTargets.Property)]
public class JsonPrimaryKeyAttribute(string Name = "Id") : Attribute
{
    public string Name { get; } = Name;
}
#nullable restore