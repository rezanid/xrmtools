namespace XrmTools.Core.Helpers;

using Newtonsoft.Json;
using System.IO;
using XrmTools.Core.Serialization;

internal static class StreamHelper
{
    // Newtonsoft Configuration:
    private static readonly JsonSerializerSettings jsonSerializerSettings = new()
    {
        TypeNameHandling = TypeNameHandling.Auto,
        NullValueHandling = NullValueHandling.Ignore,
        ContractResolver = new IgnoreEntityPropertiesResolver()
    };

    private static readonly JsonSerializerSettings jsonSerializerSettingsForFile = new()
    {
        TypeNameHandling = TypeNameHandling.Auto,
        NullValueHandling = NullValueHandling.Ignore,
        ContractResolver = new EntityResolverForFile()
    };

    internal static T Deserialize<T>(this Stream stream, bool isFile = false)
    {
        using var sr = new StreamReader(stream);
        using var jr = new JsonTextReader(sr);
        var settings = isFile ? jsonSerializerSettingsForFile : jsonSerializerSettings;
        var serializer = JsonSerializer.Create(settings);
        return serializer.Deserialize<T>(jr);
    }
}
