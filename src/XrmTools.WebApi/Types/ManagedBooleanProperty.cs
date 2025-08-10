namespace XrmTools.WebApi.Types;

public class ManagedBooleanProperty : ManagedProperty<bool>
{
    public ManagedBooleanProperty() : this(value: false) { }

    public ManagedBooleanProperty(bool value) 
        : base(value) { }

    internal ManagedBooleanProperty(bool value, string? logicalName) 
        : base(value, logicalName) { }
}
#nullable restore