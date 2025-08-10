namespace XrmTools.WebApi.Types;

using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

[JsonConverter(typeof(StringEnumConverter))]
public enum RelationshipType
{
    [EnumMember(Value = "OneToManyRelationship")]
    OneToManyRelationship = 0,
    [EnumMember(Value = "ManyToManyRelationship")]
    ManyToManyRelationship = 1,
    Default = 0
}