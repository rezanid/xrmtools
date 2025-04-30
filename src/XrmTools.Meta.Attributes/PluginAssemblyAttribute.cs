namespace XrmTools.Meta.Attributes
{
    using System;

    // Any change in the constructors of this class requires a change in PluginAttributeExtractor.
    [AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
    public class PluginAssemblyAttribute : Attribute
    {
        public static SourceTypes DefaultSourceType = SourceTypes.Database;
        public static IsolationModes DefaultIsolationMode = IsolationModes.Sandbox;
        public string Id { get; set; }
        public SourceTypes SourceType { get; set; } = DefaultSourceType;
        public IsolationModes IsolationMode { get; set; } = DefaultIsolationMode;
        public string SolutionId { get; set; }
    }
}