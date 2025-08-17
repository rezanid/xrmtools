namespace XrmTools.Meta.Attributes
{
    using System;

    /// <summary>
    /// Apply this attribute to a plugin class to mark it for plugin registration. By default, the name will be inferred from the type's name,
    /// but you can specify the name explicitly, along with a description and other details.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class PluginAttribute : Attribute
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string FriendlyName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string WorkflowActivityGroupName { get; set; } = string.Empty;
    }
}