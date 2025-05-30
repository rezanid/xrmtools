namespace XrmTools.Meta.Attributes
{
    using System;

    /// <summary>
    /// This attribute when used inside a CustomApi plugin class, marks the nested class that 
    /// contains response properties for the custom API.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class CustomApiResponseAttribute : Attribute { }
}