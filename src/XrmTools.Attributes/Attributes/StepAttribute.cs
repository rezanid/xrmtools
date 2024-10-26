#nullable enable
namespace XrmTools.Meta.Attributes;
using System;
using XrmTools.Meta.Model;

/// <summary>
/// Adds plugin step to a plugin type.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public class StepAttribute(
    MessageNames message, string entityName, string filteringAttributes, Stages stage, ExecutionMode mode) : Attribute
{
    // Constructor (aka positional) parameters
    public ExecutionMode Mode { get; set; } = mode;
    public string PrimaryEntityName { get; } = entityName;
    public string FilteringAttributes { get; } = filteringAttributes;
    public Stages Stage { get; } = stage;
    public string MessageName { get; } = message.ToString();

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
