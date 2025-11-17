namespace XrmTools.Meta.Attributes
{
    using System;

    /// <summary>
    /// Marks a dependency for automatic assignment by InjectDependencies. Can be applied to
    /// properties, fields, or constructor parameters. Optional Name allows named resolution.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, Inherited = true, AllowMultiple = false)]
    public sealed class DependencyAttribute : Attribute
    {
        /// <summary>
        /// Optional name for named dependency resolution.
        /// Usage: [Dependency("MyName")] or [Dependency(Name = "MyName")]
        /// </summary>
        public string Name { get; set; }

        public DependencyAttribute() { }
        public DependencyAttribute(string name) { Name = name; }
    }
}