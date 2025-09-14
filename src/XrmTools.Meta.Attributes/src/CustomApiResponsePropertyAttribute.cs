namespace XrmTools.Meta.Attributes
{
    using System;

    /// <summary>
    /// Apply this attribute to a property to control how it is converted to a response property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class CustomApiResponsePropertyAttribute : Attribute 
    {
        /// <summary>
        /// This is the name that will be used in the response of the API.
        /// </summary>
        public string UniqueName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string LogicalEntityName { get; set; } = string.Empty;
    }
}