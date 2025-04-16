#nullable enable
namespace XrmTools.Meta.Attributes;

using System;
using XrmTools.Meta.Model;

/// <summary>
/// Generates a Custom API for a plugin. If no name has been given, name of the type will be used.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class CustomApiAttribute(string uniqueName) : Attribute
{
    public string? DisplayName { get; set; }
    public string? Name { get; set; }
    public string? UniqueName { get; set; } = uniqueName;
    public string? Description { get; set; }
    public ProcessingStepTypes StepType { get; set; } = ProcessingStepTypes.SyncAndAsync;
    public BindingTypes BindingType { get; set; } = BindingTypes.Global;
    public string? BoundEntityLogicalName { get; set; }
    public string? ExecutePrivilegeName { get; set; } 
    public bool IsFunction { get; set; } = false;
    public bool IsPrivate { get; set; } = false;
    public CustomApiAttribute(string uniqueName, string displayName) : this(uniqueName)
    {
        DisplayName = displayName;
    }
    public CustomApiAttribute(string uniqueName, string displayName, string name) : this(uniqueName, displayName)
    {
        UniqueName = uniqueName;
        DisplayName = displayName;
        Name = name;
    }
}
#nullable restore