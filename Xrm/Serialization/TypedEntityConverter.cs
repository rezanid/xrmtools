#nullable enable
namespace XrmGen.Xrm.Serialization;

using System;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using XrmGen.Xrm.Model;

public class IgnoreEntityPropertiesConverter : JsonConverter<object>
{
    public override bool CanConvert(Type typeToConvert)
    {
        // Check if the typeToConvert is a TypedEntity or inherits from TypedEntity
        if (typeToConvert.IsValueType) return false;
        var baseType = typeToConvert.BaseType;
        while (baseType is not null)
        {
            if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(TypedEntity<>)) return true;
            baseType = baseType.BaseType;
        }
        return false;
    }

    public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var instance = Activator.CreateInstance(typeToConvert);

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return instance;
            }

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                string jsonPropertyName = reader.GetString();
                reader.Read();

                // Find the property by JsonPropertyName or property name
                var property = typeToConvert.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                    .FirstOrDefault(p =>
                        string.Equals(p.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? p.Name,
                                      jsonPropertyName, StringComparison.OrdinalIgnoreCase));

                if (property != null && !property.GetCustomAttributes<JsonIgnoreAttribute>(true).Any())
                {
                    var propertyValue = JsonSerializer.Deserialize(ref reader, property.PropertyType, options);
                    property.SetValue(instance, propertyValue);
                }
                else
                {
                    // Skip the property if it's not in the derived class or is ignored
                    reader.Skip();
                }
            }
        }

        return instance;
    }

    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        // Get the properties of the derived class (TypedEntity<T>)
        var properties = value.GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

        foreach (var property in properties)
        {
            if (property.GetCustomAttributes<JsonIgnoreAttribute>(true).Any())
            {
                continue; // Skip ignored properties
            }

            var propertyValue = property.GetValue(value);

            // Handle [JsonPropertyName] attribute
            var propertyName = property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name
                ?? options.PropertyNamingPolicy?.ConvertName(property.Name)
                ?? property.Name;

            writer.WritePropertyName(propertyName);
            JsonSerializer.Serialize(writer, propertyValue, propertyValue?.GetType() ?? typeof(object), options);
        }

        writer.WriteEndObject();
    }
}
#nullable restore