#nullable enable
namespace XrmTools.Meta.Serialization;

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

internal class CommaDelimitedStringConverter : JsonConverter<List<string>>
{
    public override List<string>? ReadJson(JsonReader reader, Type objectType, List<string>? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
            return null;

        var value = reader.Value as string;
        if (string.IsNullOrEmpty(value))
            return [];

        return [.. value!.Split([','], StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim())];
    }

    public override void WriteJson(JsonWriter writer, List<string>? value, JsonSerializer serializer)
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }

        writer.WriteValue(string.Join(",", value));
    }
}
#nullable restore