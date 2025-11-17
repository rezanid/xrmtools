namespace XrmTools.Meta.Model.Configuration;

public sealed class TypeSymbol
{
    public string Name { get; set; } = string.Empty;
    public bool IsAbstract { get; set; } = false;
    public bool IsVirtual { get; set; } = false;
}
