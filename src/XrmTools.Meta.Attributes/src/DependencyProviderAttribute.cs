namespace XrmTools.Meta.Attributes
{
    using System;

    /// <summary>
    /// Marks a property in a plugin (or its base class) as a provider for dependency injection by the <c>InjectDependencies</c> method.
    /// Remember to call <c>InjectDependencies</c> early in your plugin's <c>Execute</c> method to ensure dependencies are set before use.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class DependencyProviderAttribute : Attribute { }
}