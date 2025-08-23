namespace XrmTools.Meta.Attributes
{
    using System;

    /// <summary>
    /// Specifies prefixes to replace during entity code generation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
    public class CodeGenReplacePrefixesAttribute : Attribute
    {
        /// <summary>
        /// Comma-separated list of prefixes to replace.
        /// </summary>
        public string Prefixes { get; set; }

        /// <summary>
        /// Comma-separated list of prefixes to replace with.
        /// </summary>
        public string ReplaceWith { get; set; }

        public CodeGenReplacePrefixesAttribute(string prefixes) : this(prefixes, string.Empty) { }

        public CodeGenReplacePrefixesAttribute(string prefixes, string replaceWith)
        {
            Prefixes = prefixes;
            ReplaceWith = replaceWith;
        }
    }
}