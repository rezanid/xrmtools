#nullable enable
namespace XrmTools.Meta.Attributes;
using System;
using XrmTools.Meta.Model;

// Any change in the constructors of this class requires a change in PluginAttributeExtractor.
/// <summary>
/// Adds plugin step to a plugin type. This attribute should only be applied after a <see cref="PluginAttribute" /> to the class.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public class StepAttribute(
    string entityName, string message, string filteringAttributes, Stages stage, ExecutionMode mode) : Attribute
{
    // Constructor (aka positional) parameters
    public ExecutionMode Mode { get; set; } = mode;
    public string PrimaryEntityName { get; } = entityName;
    public string FilteringAttributes { get; } = filteringAttributes;
    public Stages Stage { get; } = stage;
    public string MessageName { get; } = message;

    // Named parameters
    public string? Id { get; set; }
    public int ExecutionOrder { get; set; } = 1;
    public string? ImpersonatingUserFullname { get; set; }
    public SupportedDeployments? SupportedDeployment { get; set; }
    public PluginStepStates? State { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Configuration { get; set; }
    public bool CanBeBypassed { get; set; } = false;
    public bool AsyncAutoDelete { get; set; } = false;
}
#nullable restore