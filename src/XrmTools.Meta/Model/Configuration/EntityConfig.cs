#nullable enable
namespace XrmTools.Meta.Model.Configuration;

using System;

public class EntityConfig
{
    public string? LogicalName { get; set; }
    public string? AttributeNames { get; set; }
    public string BaseType { get; set; } = "Microsoft.Xrm.Sdk.Entity";
}
#nullable restore