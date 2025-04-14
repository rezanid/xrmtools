#nullable enable
namespace XrmTools.Meta.Attributes;
using System;
using XrmTools.Meta.Model;

// NOTE! Any change in the constructors of this class requires a change in  XrmTools.Analyzers.AttributeConvertor.

/// <summary>
/// Adds plugin step to a plugin type. This attribute should only be applied after a <see cref="PluginAttribute" /> to the class.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public class StepAttribute(string messageName, Stages stage, ExecutionMode mode) : Attribute
{
    // Constructor (aka positional) parameters
    public ExecutionMode Mode { get; set; } = mode;
    public Stages Stage { get; } = stage;
    public string MessageName { get; } = messageName;

    // Named parameters
    public string? PrimaryEntityName { get; set; }
    public string? FilteringAttributes { get; set; }
    public string? Id { get; set; }
    public int ExecutionOrder { get; set; } = 1;
    public string? ImpersonatingUserFullname { get; set; }
    public SupportedDeployments? SupportedDeployment { get; set; } = SupportedDeployments.Server;
    public PluginStepStates? State { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Configuration { get; set; }
    public bool CanBeBypassed { get; set; } = false;
    public bool AsyncAutoDelete { get; set; } = false;

    public StepAttribute(string messageName, string primaryEntityName, Stages stage, ExecutionMode mode) : this(messageName, stage, mode)
        => PrimaryEntityName = primaryEntityName;

    public StepAttribute(string messageName, string primaryEntityName, string filteringAttributes, Stages stage, ExecutionMode mode) 
        : this(messageName, primaryEntityName, stage, mode)
        => FilteringAttributes = filteringAttributes;
}
#nullable restore