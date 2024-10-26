#nullable enable
namespace XrmTools.Meta.Attributes;
using System;

/// <summary>
/// Marks a class as a plugin. The type name will be inferred from the class name.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public class PluginAttribute(string name) : Attribute
{
    public string? Id { get; set; }
    public string? Name { get; } = name;
    public string? FriendlyName { get; set; } = name;
    public string? Description { get; set; }
    public string? WorkflowActivityGroupName { get; set; }
}