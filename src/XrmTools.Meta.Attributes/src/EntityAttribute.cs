namespace XrmTools.Meta.Attributes
{
    using System;

    /// <summary>
    /// Apply this attribute to an empty file or to AssemblyInfo.cs to generate a strongly-typed entity class.
    /// You can specify which attributes to include by listing them in the AttributeNames property.
    /// Each time you save the file, all entities will be regenerated.
    /// Example usage: [assembly: Entity("contact", AttributeNames = "firstname,lastname")]
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
    public class EntityAttribute : Attribute
    {
        /// <summary>
        /// Logical name of the entity.
        /// </summary>
        public string LogicalName { get; set; } = string.Empty;

        /// <summary>
        /// Comma-delimited list of attributes to include in the entity.
        /// </summary>
        public string AttributeNames { get; set; } = string.Empty;

        public EntityAttribute() { }

        public EntityAttribute(string logicalName)
        {
            LogicalName = logicalName;
        }
    }
}