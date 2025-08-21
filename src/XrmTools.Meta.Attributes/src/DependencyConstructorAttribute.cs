namespace XrmTools.Meta.Attributes
{
    using System;

    /// <summary>
    /// Marks the constructor to be used by the <c>InjectDependencies</c> method.
    /// Use this attribute when your class has multiple constructors.
    /// Call <c>InjectDependencies</c> early in your plugin's <c>Execute</c> method to ensure dependencies are set before use.
    /// </summary>
    [AttributeUsage(AttributeTargets.Constructor)]
    public sealed class DependencyConstructorAttribute : Attribute { }
}