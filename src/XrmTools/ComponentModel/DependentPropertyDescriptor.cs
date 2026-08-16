namespace XrmTools.ComponentModel;

using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

internal sealed class DependentPropertyDescriptor(PropertyDescriptor baseProperty, object instance) : PropertyDescriptor(baseProperty)
{
    public override bool IsReadOnly => baseProperty.IsReadOnly || ComputeReadOnly();

    private bool ComputeReadOnly()
    {
        foreach (var rule in baseProperty.Attributes.OfType<ReadOnlyWhenAttribute>())
        {
            var dep = instance.GetType().GetProperty(rule.DependencyProperty,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (dep is null)
                continue;

            var value = dep.GetValue(instance);

            // Rule semantics: read-only unless dependency == expected
            if (Equals(value, rule.ExpectedValue))
                return true;
        }

        return false;
    }

    public override Type ComponentType => baseProperty.ComponentType;
    public override Type PropertyType => baseProperty.PropertyType;
    public override bool CanResetValue(object component) => baseProperty.CanResetValue(component);
    public override object GetValue(object component) => baseProperty.GetValue(component);
    public override void ResetValue(object component) => baseProperty.ResetValue(component);
    public override void SetValue(object component, object value) => baseProperty.SetValue(component, value);
    public override bool ShouldSerializeValue(object component) => baseProperty.ShouldSerializeValue(component);
}