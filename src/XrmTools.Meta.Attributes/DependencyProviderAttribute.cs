namespace XrmTools.Meta.Attributes
{
    using System;

    /// <summary>
    /// Specifies that a property is a dependency provider in a plugin or its parent.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class DependencyProviderAttribute : Attribute { }
}