namespace XrmTools.WebApi.Types;

using Newtonsoft.Json.Converters;
using System;
using System.Runtime.Serialization;

[Flags]
[Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
public enum SecurityTypes
{
    [EnumMember(Value = "None")]
    None = 0,
    [EnumMember(Value = "Append")]
    Append = 1,
    [EnumMember(Value = "ParentChild")]
    ParentChild = 2,
    [EnumMember(Value = "Pointer")]
    Pointer = 4,
    [EnumMember(Value = "Inheritance")]
    Inheritance = 8
}
