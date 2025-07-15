namespace XrmTools.Meta.Attributes
{
    using System;

    /// <summary>
    /// Flags a property or a field as a dependency to be injected.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class DependencyAttribute : Attribute { }
}