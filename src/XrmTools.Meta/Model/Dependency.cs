namespace XrmTools.Meta.Model;
using System.Collections.Generic;

public class Dependency
{
    public string? PropertyName { get; set; }
    public string FullTypeName { get; set; } = string.Empty;
    public string ShortTypeName { get; set; } = string.Empty;
    public List<Dependency> Dependencies { get; set; } = [];
    public bool IsProperty { get; set; }
    public bool IsLocalVariableNeeded { get; set; }
    public string? ProvidedByBaseProperty { get; set; }
}