namespace XrmTools.Meta.Attributes
{
    using System;

    /// <summary>
    /// Flags a constructor to be used for dependency injection when the class has more than one constructor.
    /// </summary>
    [AttributeUsage(AttributeTargets.Constructor)]
    public sealed class DependencyConstructorAttribute : Attribute
    {
    }
}