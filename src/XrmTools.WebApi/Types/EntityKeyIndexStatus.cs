namespace XrmTools.WebApi.Types;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

[JsonConverter(typeof(StringEnumConverter))]
public enum EntityKeyIndexStatus
{
    [EnumMember]
    Pending,
    [EnumMember]
    InProgress,
    [EnumMember]
    Active,
    [EnumMember]
    Failed
}