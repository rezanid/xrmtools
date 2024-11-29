#nullable enable
namespace XrmTools.Core.Serialization;

using System;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using XrmTools.Core.Helpers;
using XrmTools.Xrm.Model;

/// <summary>
/// This converter is applied to types deriving from <see cref="TypedEntity{T}"/>. All properties inherited from Entity
/// are ignored.
/// </summary>
public class IgnoreEntityPropertiesConverter : JsonConverter<object>
{
    public override bool CanConvert(Type typeToConvert)
        => !typeToConvert.IsValueType && IsSubclassOfRawGeneric(typeof(TypedEntity<>), typeToConvert);

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
                var jsonPropertyName = reader.GetString();
                reader.Read();

                // Find the property by JsonPropertyName or property name
                var property =
                    typeToConvert.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                                 .FirstOrDefault(p =>
                                    string.Equals(
                                        p.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? p.Name,
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
        var type = value.GetType();
        var properties = type
            .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.GetProperty)
            //.Union([type.GetProperty("Id")])
            .Where(p => p != null)
            .OrderBy(p => p.GetCustomAttribute<JsonPropertyOrderAttribute>()?.Order ?? 0);

        foreach (var property in properties)
        {
            var ignoreAttributes = property.GetCustomAttributes<JsonIgnoreAttribute>(true);
            if (ignoreAttributes.Any())
            {
                var ignoreAttribute = ignoreAttributes.First();
                if (ignoreAttribute.Condition == JsonIgnoreCondition.Always ||
                    (ignoreAttribute.Condition == JsonIgnoreCondition.WhenWritingDefault &&
                     Equals(property.GetValue(value), property.PropertyType.GetDefaultValue())) ||
                    (ignoreAttribute.Condition == JsonIgnoreCondition.WhenWritingNull &&
                     property.GetValue(value) == null))
                {
                    continue; // Skip ignored properties
                }
            }

            var propertyValue = property.GetValue(value);

            // Handle [JsonPropertyName] attribute
            var propertyName = property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name
                ?? options.PropertyNamingPolicy?.ConvertName(property.Name)
                ?? property.Name;

            if (propertyValue == null && options.DefaultIgnoreCondition == JsonIgnoreCondition.WhenWritingNull)
            {
                continue; // Skip properties with null values
            }

            writer.WritePropertyName(propertyName);
            JsonSerializer.Serialize(writer, propertyValue, propertyValue?.GetType() ?? typeof(object), options);
        }

        writer.WriteEndObject();
    }


    private static bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
    {
        while (toCheck != null && toCheck != typeof(object))
        {
            var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
            if (generic == cur)
            {
                return true;
            }
            toCheck = toCheck.BaseType;
        }
        return false;
    }
}
#nullable restore