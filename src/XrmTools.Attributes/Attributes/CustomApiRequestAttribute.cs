#nullable enable
namespace XrmTools.Meta.Attributes;
using System;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class CustomApiRequestAttribute : Attribute { }
#nullable restore