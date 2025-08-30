namespace XrmTools.Meta.Attributes
{
    using System;

    /// <summary>
    /// Specifies metadata for a plugin assembly, including its unique identifier, source type, isolation mode, and
    /// associated solution.
    /// </summary>
    /// <remarks>Apply this attribute to an assembly to provide information used by plugin management systems,
    /// such as identifying the assembly, determining its source, and configuring its execution isolation.</remarks>
    [AttributeUsage(AttributeTargets.Assembly)]
    public class PluginAssemblyAttribute : Attribute
    {
        public static SourceTypes DefaultSourceType = SourceTypes.Database;
        public static IsolationModes DefaultIsolationMode = IsolationModes.Sandbox;
        public string Id { get; set; } = string.Empty;
        public SourceTypes SourceType { get; set; } = DefaultSourceType;
        public IsolationModes IsolationMode { get; set; } = DefaultIsolationMode;
    }
}