namespace XrmTools.WebApi.Types;

using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

[Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
public enum AssociatedMenuBehavior
{
    [EnumMember(Value = "UseCollectionName")]
    UseCollectionName,
    [EnumMember(Value = "UseLabel")]
    UseLabel,
    [EnumMember(Value = "DoNotDisplay")]
    DoNotDisplay
}