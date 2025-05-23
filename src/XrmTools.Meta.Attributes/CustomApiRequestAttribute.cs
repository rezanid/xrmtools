namespace XrmTools.Meta.Attributes
{
    using System;

    /// <summary>
    /// This attribute when used inside a CustomApi plugin class, marks the nested class that 
    /// contains request parameters for the custom API. When used on a class that is not nested
    /// and with the name of the custom API, it marks the class as a request class for calling the custom API.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class CustomApiRequestAttribute : Attribute 
    {
        public string CustomApiName { get; set; } = string.Empty;

        /// <summary>
        /// This constructor is used for nested classes that define request parameters for custom APIs.
        /// </summary>
        public CustomApiRequestAttribute() { }

        /// <summary>
        /// This constructor is used for request classes that act as OrganizationRequest for calling custom APIs.
        /// </summary>
        /// <param name="customApiName"></param>
        public CustomApiRequestAttribute(string customApiName)
        {
            CustomApiName = customApiName;
        }
    }

}