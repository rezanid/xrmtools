namespace XrmTools.Meta.Attributes
{
    using System;

    /// <summary>
    /// Enables OrganizationContext generation for typed entities.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
    public sealed class CodeGenOrganizationContextAttribute : Attribute
    {
        /// <summary>
        /// Name of the OrganizationContext to generate.
        /// </summary>
        public string Name { get; }

        public CodeGenOrganizationContextAttribute(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
            }

            Name = name;
        }
    }
}