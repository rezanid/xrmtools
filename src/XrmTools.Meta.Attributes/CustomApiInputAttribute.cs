namespace XrmTools.Meta.Attributes
{
    using System;

    /// <summary>
    /// Generates a request parameter for a Custom API.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class CustomApiInputAttribute : Attribute
    {
        public string DisplayName { get; set; }
        public string Name { get; set; }
        public string UniqueName { get; set; }
        public string Description { get; set; }
        public CustomApiFieldType Type { get; set; }
        public string LogicalEntityName { get; set; }
        public bool IsOptional { get; set; } = false;

        public CustomApiInputAttribute(CustomApiFieldType type, string displayName)
        {
            Type = type;
            DisplayName = displayName;
        }
        public CustomApiInputAttribute(CustomApiFieldType type, string displayName, string name) : this(type, displayName)
        {
            Name = name;
        }
        public CustomApiInputAttribute(CustomApiFieldType type, string displayName, string name, string uniqueName) : this(type, displayName, name)
        {
            UniqueName = uniqueName;
        }
    }
}