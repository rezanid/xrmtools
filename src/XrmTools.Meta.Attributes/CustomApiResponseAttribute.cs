namespace XrmTools.Meta.Attributes
{
    using System;

    /// <summary>
    /// This attribute when used inside a CustomApi plugin class, marks the nested class that 
    /// contains response properties for the custom API. When used on a class that is not nested
    /// and with the name of the custom API, it marks the class as a response class for calling the custom API.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class CustomApiResponseAttribute : Attribute 
    {
        public string CustomApiName { get; set; } = string.Empty;

        /// <summary>
        /// This constructor is used for nested classes that define response properties for custom APIs.
        /// </summary>
        public CustomApiResponseAttribute() { }

        /// <summary>
        /// This constructor is used for response classes that act as OrganizationResponse for calling custom APIs.
        /// </summary>
        /// <param name="customApiName"></param>
        public CustomApiResponseAttribute(string customApiName)
        {
            CustomApiName = customApiName;
        }
    }
}