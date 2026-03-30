namespace XrmTools.WebApi.Types;

using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

[Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
public enum LookupFormat
{
    [EnumMember(Value = "None")]
    None,
    [EnumMember(Value = "Connection")]
    Connection,
    [EnumMember(Value = "Regarding")]
    Regarding,
    [EnumMember(Value = "Text")]
    Text
}
