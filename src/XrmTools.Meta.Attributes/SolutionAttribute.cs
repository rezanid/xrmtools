namespace XrmTools.Meta.Attributes
{
    using System;

    /// <summary>
    /// Generates a Custom API for a plugin. If no name has been given, name of the type will be used.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class SolutionAttribute : Attribute
    {
        public string UniqueName { get; set; }
        public string FriendlyName { get; set; }
        public string Description { get; set; }

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