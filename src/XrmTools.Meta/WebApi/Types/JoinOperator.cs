namespace XrmTools.WebApi.Types;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

[JsonConverter(typeof(StringEnumConverter))]
public enum JoinOperator
{
    Inner,
    Outer,
    Natural,
    MatchFirstRowUsingCrossApply,
    In,
    Exists,
    Any,
    NotAny,
    All,
    NotAll
}
