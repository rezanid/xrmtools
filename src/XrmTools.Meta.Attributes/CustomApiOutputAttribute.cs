namespace XrmTools.Meta.Attributes
{
    using System;

    /// <summary>
    /// Generates a response property for a Custom API.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="displayName"></param>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class CustomApiOutputAttribute : Attribute
    {
        public string DisplayName { get; set; }
        public string Name { get; set; }
        public string UniqueName { get; set; }
        public string Description { get; set; }
        public CustomApiFieldType Type { get; set; }
        public string LogicalEntityName { get; set; }

        public CustomApiOutputAttribute(CustomApiFieldType type, string displayName)
        {
            Type = type;
            DisplayName = displayName;
        }
        public CustomApiOutputAttribute(CustomApiFieldType type, string displayName, string name) : this(type, displayName)
        {
            Name = name;
        }
        public CustomApiOutputAttribute(CustomApiFieldType type, string displayName, string name, string uniqueName) : this(type, displayName, name)
        {
            UniqueName = uniqueName;
        }
    }
}