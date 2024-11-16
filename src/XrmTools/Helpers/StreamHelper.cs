namespace XrmTools.Helpers;

using Newtonsoft.Json;
using System.IO;
using XrmTools.Xrm.Serialization;

internal static class StreamHelper
{
    // Newtonsoft Configuration:
    private static readonly JsonSerializerSettings jsonSerializerSettings = new()
    {
        TypeNameHandling = TypeNameHandling.Auto,
        NullValueHandling = NullValueHandling.Ignore,
        ContractResolver = new IgnoreEntityPropertiesResolver()
    };

    internal static T Deserialize<T>(this Stream stream)
    {
        using var sr = new StreamReader(stream);
        using var jr = new JsonTextReader(sr);
        var serializer = JsonSerializer.Create(jsonSerializerSettings);
        return serializer.Deserialize<T>(jr);
    }
}
