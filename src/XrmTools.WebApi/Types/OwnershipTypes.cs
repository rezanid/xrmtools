namespace XrmTools.WebApi.Types;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

[JsonConverter(typeof(StringEnumConverter))]
public enum OwnershipTypes
{
    [EnumMember(Value = "None")]
    None = 0,
    [EnumMember(Value = "UserOwned")]
    UserOwned = 1,
    [EnumMember(Value = "TeamOwned")]
    TeamOwned = 2,
    [EnumMember(Value = "BusinessOwned")]
    BusinessOwned = 4,
    [EnumMember(Value = "OrganizationOwned")]
    OrganizationOwned = 8,
    [EnumMember(Value = "BusinessParented")]
    BusinessParented = 0x10,
    [EnumMember(Value = "Filtered")]
    Filtered = 0x20
}