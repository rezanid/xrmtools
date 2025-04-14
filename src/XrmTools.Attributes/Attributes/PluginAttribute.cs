#nullable enable
namespace XrmTools.Meta.Attributes;
using System;

// Any change in the constructors of this class requires a change in PluginAttributeExtractor.
/// <summary>
/// Marks a class as a plugin. The type name will be inferred from the class name.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public class PluginAttribute : Attribute
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? FriendlyName { get; set; }
    public string? Description { get; set; }
    public string? WorkflowActivityGroupName { get; set; }
}