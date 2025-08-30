namespace XrmTools.WebApi.Types;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

[JsonConverter(typeof(StringEnumConverter))]
public enum CascadeType
{
    NoCascade,
    Cascade,
    Active,
    UserOwned,
    RemoveLink,
    Restrict
}
