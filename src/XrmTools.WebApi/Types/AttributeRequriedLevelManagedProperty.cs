namespace XrmTools.WebApi.Types;
public sealed class AttributeRequiredLevelManagedProperty : ManagedProperty<AttributeRequiredLevel>
{
    public bool IsValueModified { get; set; } = true;

    public bool IsCanBeChangedPropertyModified { get; set; } = true;

    public AttributeRequiredLevelManagedProperty(AttributeRequiredLevel value) 
        : base(value) { }

    internal AttributeRequiredLevelManagedProperty(AttributeRequiredLevel value, string? logicalName) 
        : base(value, logicalName) { }
}
