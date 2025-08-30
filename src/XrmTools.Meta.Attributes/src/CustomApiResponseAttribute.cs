namespace XrmTools.Meta.Attributes
{
    using System;

    /// <summary>
    /// Apply this attribute to a class to indicate that its properties should be included as response properties
    /// in the Custom API where the class is used.
    /// </summary>
    /// <remarks>
    /// Currently, the class that the attribute is applied to, should be nested inside a Custom API plugin.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class CustomApiResponseAttribute : Attribute { }
}