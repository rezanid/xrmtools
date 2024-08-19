#nullable enable
namespace XrmGen.Xrm.Serialization;

using XrmGen.Xrm.Model;
using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Reflection;
using Microsoft.Xrm.Sdk;

public class TypedEntityConverter<T> : JsonConverter<T> where T : Entity, ITypedEntity, new()
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var entity = new T();
        var properties = typeToConvert.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                return entity;

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propertyName = reader.GetString();
                reader.Read(); // Move to the value

                var property = properties.FirstOrDefault(p =>
                    p.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name == propertyName ||
                    p.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase));

                if (property != null && !property.IsDefined(typeof(JsonIgnoreAttribute), true))
                {
                    var value = JsonSerializer.Deserialize(ref reader, property.PropertyType, options);
                    property.SetValue(entity, value);
                }
                else
                {
                    reader.Skip(); // Skip if the property doesn't exist or is ignored
                }
            }
        }

        return entity;
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        var properties = value.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        foreach (var property in properties)
        {
            if (property.IsDefined(typeof(JsonIgnoreAttribute), true))
                continue;

            var propertyName = property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? property.Name;
            var propertyValue = property.GetValue(value);

            if (propertyValue != null || !options.DefaultIgnoreCondition.HasFlag(JsonIgnoreCondition.WhenWritingNull))
            {
                writer.WritePropertyName(propertyName);
                JsonSerializer.Serialize(writer, propertyValue, property.PropertyType, options);
            }
        }

        writer.WriteEndObject();
    }
}
#nullable restore