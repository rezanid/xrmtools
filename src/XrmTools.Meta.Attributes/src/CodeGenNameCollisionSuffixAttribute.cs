namespace XrmTools.Meta.Attributes
{
    using System;

    /// <summary>
    /// Specifies the suffix appended to a generated member (property) name when it would otherwise
    /// be identical to the name of its enclosing type. This situation occurs when a Dataverse column
    /// shares the logical/schema name of its table (e.g. a table "xxx_postalcode" containing a column
    /// "xxx_postalcode"), which C# forbids (compiler error CS0542). When omitted, the default suffix
    /// "Value" is used, so the column above generates a property named "PostalCodeValue".
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
    internal class CodeGenNameCollisionSuffixAttribute : Attribute
    {
        /// <summary>
        /// The suffix to append to a generated member name that collides with its enclosing type name.
        /// </summary>
        public string Suffix { get; set; }

        public CodeGenNameCollisionSuffixAttribute(string suffix)
        {
            Suffix = suffix;
        }
    }
}
