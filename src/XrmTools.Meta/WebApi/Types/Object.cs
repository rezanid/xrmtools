﻿#nullable enable
namespace XrmTools.WebApi.Types;

public class Object
{
    public Object() { }

    public Object(ObjectType type, string value)
    {
        Type = type;
        Value = value;
    }
    public ObjectType Type { get; set; }
    public string Value { get; set; }
}
#nullable restore