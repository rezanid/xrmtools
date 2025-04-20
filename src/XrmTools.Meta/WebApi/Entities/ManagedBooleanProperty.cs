#nullable enable
namespace XrmTools.WebApi.Entities;

public class ManagedBooleanProperty
{
    public bool Value { get; set; }
    public bool CanBeChanged { get; set; }
    public string ManagedPropertyLogicalName { get; set; }
}
#nullable restore