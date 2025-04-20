#nullable enable
namespace XrmTools.Meta.Attributes.Serialization;
using System;

[AttributeUsage(AttributeTargets.Property)]
internal class JsonPrimaryKeyAttribute(string Name = "Id") : Attribute
{
    public string Name { get; } = Name;
}
#nullable restore