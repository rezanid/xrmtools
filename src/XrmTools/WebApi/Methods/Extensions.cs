namespace XrmTools.WebApi.Methods;
using System.Text.Json;
using System.Text.Json.Serialization;

internal partial class Extensions
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };
}
