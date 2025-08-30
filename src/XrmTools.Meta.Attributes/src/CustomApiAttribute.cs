namespace XrmTools.Meta.Attributes
{
    using System;

    /// <summary>
    /// Generates a Custom API for a plugin. If no name has been given, name of the type will be used.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class CustomApiAttribute : Attribute
    {
        public string DisplayName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string UniqueName { get; }
        public string Description { get; set; } = string.Empty;
        public ProcessingStepTypes StepType { get; set; } = ProcessingStepTypes.SyncAndAsync;
        public BindingTypes BindingType { get; set; } = BindingTypes.Global;
        public string BoundEntityLogicalName { get; set; } = string.Empty;
        public string ExecutePrivilegeName { get; set; } = string.Empty;
        public bool IsFunction { get; set; } = false;
        public bool IsPrivate { get; set; } = false;
        public CustomApiAttribute(string uniqueName)
        {
            UniqueName = uniqueName;
        }
        public CustomApiAttribute(string uniqueName, string displayName) : this(uniqueName)
        {
            DisplayName = displayName;
        }
        public CustomApiAttribute(string uniqueName, string displayName, string name) : this(uniqueName, displayName)
        {
            UniqueName = uniqueName;
            DisplayName = displayName;
            Name = name;
        }
    }
}