namespace XrmTools.WebApi.Types;

using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

[Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
public enum AttributeRequiredLevel
{
    [EnumMember(Value = "None")]
    None,
    [EnumMember(Value = "SystemRequired")]
    SystemRequired,
    [EnumMember(Value = "ApplicationRequired")]
    ApplicationRequired,
    [EnumMember(Value = "Recommended")]
    Recommended
}