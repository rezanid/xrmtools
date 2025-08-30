namespace XrmTools.WebApi.Types;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

[JsonConverter(typeof(StringEnumConverter))]
public enum CustomApiFieldType
{
    Boolean = 0,
    DateTime = 1,
    Decimal = 2,
    Entity = 3,
    EntityCollection = 4,
    EntityReference = 5,
    Float = 6,
    Integer = 7,
    Money = 8,
    Picklist = 9,
    String = 10,
    StringArray = 11,
    Guid = 12,
}
