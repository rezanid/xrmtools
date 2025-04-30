namespace XrmTools.Meta.Attributes
{
    using System;

    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class DependencyProviderAttribute : Attribute { }
}