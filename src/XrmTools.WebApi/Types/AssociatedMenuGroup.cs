namespace XrmTools.WebApi.Types;

using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

[Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
public enum AssociatedMenuGroup
{
    [EnumMember(Value = "Details")]
    Details,
    [EnumMember(Value = "Sales")]
    Sales,
    [EnumMember(Value = "Service")]
    Service,
    [EnumMember(Value = "Marketing")]
    Marketing
}