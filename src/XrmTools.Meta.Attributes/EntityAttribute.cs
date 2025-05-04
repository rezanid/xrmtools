namespace XrmTools.Meta.Attributes
{
    using System;

    /// <summary>
    /// Generates and includes a typed entity in the assembly.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class EntityAttribute : Attribute
    {
        /// <summary>
        /// Logical name of the entity.
        /// </summary>
        public string LogicalName { get; set; }

        /// <summary>
        /// Comma-delimited list of attributes to include in the entity.
        /// </summary>
        public string AttributeNames { get; set; }

        public EntityAttribute() { }

        public EntityAttribute(string logicalName)
        {
            LogicalName = logicalName;
        }
    }
}