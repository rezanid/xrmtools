namespace XrmTools.Meta.Attributes
{
    using System;

    /// <summary>
    /// Specifies the global option set generation mode for code generation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
    public class CodeGenGlobalOptionSetAttribute : Attribute
    {
        public GlobalOptionSetGenerationMode Mode { get; set; }
        public CodeGenGlobalOptionSetAttribute(GlobalOptionSetGenerationMode mode)
        {
            Mode = mode;
        }
    }

    public enum GlobalOptionSetGenerationMode
    {
        /// <summary>
        /// Enum will be generated inside each entity class using it.
        /// </summary>
        NestedInEntityClass,
        /// <summary>
        /// Enum will be generated in GlobalOptionSet.cs file.
        /// </summary>
        GlobalOptionSetFile,
    }
}