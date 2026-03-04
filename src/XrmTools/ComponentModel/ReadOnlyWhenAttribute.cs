namespace XrmTools.ComponentModel;

using System;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public sealed class ReadOnlyWhenAttribute(string dependencyProperty, object expectedValue) : Attribute
{
    public string DependencyProperty { get; } = dependencyProperty;
    public object ExpectedValue { get; } = expectedValue;
}