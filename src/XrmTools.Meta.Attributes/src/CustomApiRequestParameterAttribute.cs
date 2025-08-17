namespace XrmTools.Meta.Attributes
{
    using System;

    /// <summary>
    /// Apply this attribute to a property to control how it is converted to a request parameter.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class CustomApiRequestParameterAttribute : Attribute 
    {
        public string UniqueName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsOptional { get; set; } = true;
        public string LogicalEntityName { get; set; } =string.Empty;
    }
}