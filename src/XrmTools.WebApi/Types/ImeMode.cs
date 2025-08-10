namespace XrmTools.WebApi.Types;
using System.Text.Json.Serialization;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ImeMode
{
    Auto,
    Inactive,
    Active,
    Disabled
}

public enum EntityFilters
{
    Entity = 1,
    Attributes = 2,
    Privileges = 4,
    Relationships = 8,
    All = Entity | Attributes | Privileges | Relationships
}