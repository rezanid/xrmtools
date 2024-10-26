#nullable enable
namespace XrmTools.Meta.Attributes;
using System;
using XrmTools.Meta.Model;

[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
public class PluginAssemblyAttribute : Attribute
{
    public static SourceTypes DefaultSourceType = SourceTypes.Database;
    public static IsolationModes DefaultIsolationMode = IsolationModes.Sandbox;
    public string? Id { get; set; }
    public SourceTypes? SourceType { get; set; } = DefaultSourceType;
    public IsolationModes? IsolationMode { get; set; } = DefaultIsolationMode;
    public string? SolutionId { get; set; }
}