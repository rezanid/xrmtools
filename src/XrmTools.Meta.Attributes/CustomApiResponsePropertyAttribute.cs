namespace XrmTools.Meta.Attributes
{
    using System;

    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class CustomApiResponsePropertyAttribute : Attribute 
    {
        public string UniqueName { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string LogicalEntityName { get; set; }
    }
}