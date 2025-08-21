namespace XrmTools.Meta.Attributes
{
    using System;

    /// <summary>
    /// Apply this attribute to an assembly to specify the target solution for plugin registration generated from the project.
    /// Provide the unique name of the solution, and optionally a friendly name (if different) and a description.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class SolutionAttribute : Attribute
    {
        public string UniqueName { get; set; }
        public string FriendlyName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public SolutionAttribute(string uniqueName)
        {
            UniqueName = uniqueName;
        }
        public SolutionAttribute(string uniqueName, string friendlyName) : this(uniqueName)
        {
            FriendlyName = friendlyName;
        }
    }
}