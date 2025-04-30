namespace XrmTools.Meta.Attributes
{
    using System;

    /// <summary>
    /// Flags a property or field as a dependency to be injected during code generation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class DependencyAttribute : Attribute { }
}