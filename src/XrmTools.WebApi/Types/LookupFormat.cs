namespace XrmTools.WebApi.Types;

using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

[JsonConverter(typeof(StringEnumConverter))]
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
