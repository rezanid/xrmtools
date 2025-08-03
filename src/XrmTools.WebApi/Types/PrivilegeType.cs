namespace XrmTools.WebApi.Types;
using System.Text.Json.Serialization;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PrivilegeType
{
    None,
    Create,
    Read,
    Write,
    Delete,
    Assign,
    Share,
    Append,
    AppendTo
}