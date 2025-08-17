namespace XrmTools.Meta.Attributes
{
    using System;

    /// <summary>
    /// Marks a property or field in a plugin for automatic assignment by the <c>InjectDependencies</c> method.
    /// Remember to call <c>InjectDependencies</c> early in your plugin's <c>Execute</c> method to ensure dependencies are set before use.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class DependencyAttribute : Attribute { }
}