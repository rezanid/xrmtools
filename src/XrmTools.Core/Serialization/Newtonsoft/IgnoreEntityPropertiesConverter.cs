#nullable enable
namespace XrmTools.Core.Serialization;

using System.Linq;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Reflection;
using Microsoft.Xrm.Sdk;
using XrmTools.Xrm.Model;

public class IgnoreEntityPropertiesConverter<T> : JsonConverter<T> where T : Entity, ITypedEntity, new()
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Create an instance of T
        var instance = new T();

        // Read the JSON and populate the instance
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return instance;
            }

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                string? propertyName = reader.GetString();
                if (string.IsNullOrEmpty(propertyName))
                {
                    throw new JsonException("Property name cannot be null or empty.");
                }
                reader.Read();

                // Get the property in the derived class (TypedEntity<T>)
                PropertyInfo property = typeToConvert.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

                if (property != null && !property.GetCustomAttributes(typeof(JsonIgnoreAttribute), true).Any())
                {
                    var propertyValue = JsonSerializer.Deserialize(ref reader, property.PropertyType, options);
                    property.SetValue(instance, propertyValue);
                }
                else
                {
                    // Skip the property if it's not in the derived class
                    reader.Skip();
                }
            }
        }

        return instance;
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        // Get the properties of the derived class (TypedEntity<T>)
        var properties = value.GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

        foreach (var property in properties)
        {
            if (property.GetCustomAttributes(typeof(JsonIgnoreAttribute), true).Any())
            {
                continue; // Skip ignored properties
            }

            var propertyValue = property.GetValue(value);
            var propertyName = options.PropertyNamingPolicy?.ConvertName(property.Name) ?? property.Name;

            writer.WritePropertyName(propertyName);
            JsonSerializer.Serialize(writer, propertyValue, propertyValue?.GetType() ?? typeof(object), options);
        }

        writer.WriteEndObject();
    }
}
#nullable restore