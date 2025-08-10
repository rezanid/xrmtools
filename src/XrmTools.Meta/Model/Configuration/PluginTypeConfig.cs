namespace XrmTools.Meta.Model.Configuration;
using System.Collections.Generic;
using System.Linq;
using XrmTools.WebApi.Entities;

public sealed class PluginTypeConfig : PluginType//, IPluginTypeConfig 
{
    public string? Namespace { get; set; }

    public string? BaseTypeName { get; set; }

    public string? BaseTypeNamespace { get; set; }

    public List<string> BaseTypeMethodNames { get; set; } = [];

    public Dependency? DependencyGraph { get; set; }

    /// <inheritdoc />
    public bool IsNullableEnabled { get; set; }

    public new ICollection<PluginStepConfig> Steps { get; set; } = [];

    public new CustomApi? CustomApi { get; set; }
}
