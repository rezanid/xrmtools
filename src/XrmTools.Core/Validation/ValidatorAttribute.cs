#nullable enable
namespace XrmTools.Validation;
using System;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
internal sealed class ValidatorAttribute : Attribute
{
    /// <summary>
    /// The category of the validator. This is used to group validators together. Use
    /// only the values from <see cref="Categories"/> class."/>
    /// </summary>
    public string? Category { get; init; }
    public int Priority { get; init; } = 0;
    public bool Enabled { get; init; } = true;
}
#nullable restore
