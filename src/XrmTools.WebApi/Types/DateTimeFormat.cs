namespace XrmTools.WebApi.Types;
using System.Text.Json.Serialization;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DateTimeFormat
{
    DateOnly,
    DateAndTime
}