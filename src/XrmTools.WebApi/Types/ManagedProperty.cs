namespace XrmTools.WebApi.Types;

using System.Runtime.Serialization;
using System.Text.Json.Serialization;

[KnownType(typeof(ManagedBooleanProperty))]
[KnownType(typeof(AttributeRequiredLevelManagedProperty))]
public abstract class ManagedProperty<T>
{
    public T Value { get; set; }
    public bool CanBeChanged { get; set; } = false;
    public string? ManagedPropertyLogicalName { get; set; }

    protected ManagedProperty(T value, string? managedPropertyLogicalName = null)
    {
        Value = value;
        ManagedPropertyLogicalName = managedPropertyLogicalName;
        CanBeChanged = true;
    }
}
