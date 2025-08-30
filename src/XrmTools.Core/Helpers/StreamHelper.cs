namespace XrmTools.Core.Helpers;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

internal static class StreamHelper
{
    // Newtonsoft Configuration:
    private static readonly JsonSerializerSettings jsonSerializerSettings = new()
    {
        TypeNameHandling = TypeNameHandling.Auto,
        NullValueHandling = NullValueHandling.Ignore,
        //ContractResolver = new IgnoreEntityPropertiesResolver()
    };

    private static readonly JsonSerializerSettings jsonSerializerSettingsForFile = new()
    {
        TypeNameHandling = TypeNameHandling.Auto,
        NullValueHandling = NullValueHandling.Ignore,
        //ContractResolver = new EntityResolverForFile()
    };

    internal static T Deserialize<T>(this Stream stream, bool isFile = false)
    {
        using var sr = new StreamReader(stream);
        using var jr = new JsonTextReader(sr);
        var settings = isFile ? jsonSerializerSettingsForFile : jsonSerializerSettings;
        var serializer = JsonSerializer.Create(settings);
        return serializer.Deserialize<T>(jr);
    }

    internal static T DeserializeXrmType<T>(this Stream stream)
    {
        var settings = new JsonSerializerSettings
        {
            ContractResolver = new KnownTypeContractResolver(),
            Formatting = Formatting.Indented
        };
        using var sr = new StreamReader(stream);
        using var jr = new JsonTextReader(sr);
        var serializer = JsonSerializer.Create(settings);
        return serializer.Deserialize<T>(jr);
    }

    public class KnownTypeContractResolver : DefaultContractResolver
    {
        protected override JsonObjectContract CreateObjectContract(Type objectType)
        {
            // Create the default contract first
            var contract = base.CreateObjectContract(objectType);

            // Check if the objectType has the KnownTypeAttribute
            var knownTypeAttributes = objectType
                .GetCustomAttributes<KnownTypeAttribute>()
                .Select(attr => attr.Type)
                .ToArray();

            if (knownTypeAttributes.Any())
            {
                contract.Converter = new PolymorphicODataConverter(objectType, knownTypeAttributes);
            }

            return contract;
        }

        private class PolymorphicODataConverter(Type baseType, IEnumerable<Type> knownTypes) : JsonConverter
        {
            private readonly Type _baseType = baseType;
            private readonly Dictionary<string, Type> _typeMapping = knownTypes.ToDictionary(
                    t => t.Name,
                    t => t,
                    StringComparer.OrdinalIgnoreCase);

            public override bool CanConvert(Type objectType) => true; // _baseType.IsAssignableFrom(objectType);

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                var jsonObject = Newtonsoft.Json.Linq.JObject.Load(reader);
                var odataType = jsonObject["@odata.type"]?.ToString();


                if (!string.IsNullOrWhiteSpace(odataType))
                {
                    // Remove the namespace prefix if present (OData uses `#Namespace.TypeName`)
                    var cleanTypeName = odataType.TrimStart('#');

                    // #Microsoft.Dynamics.CRM.ComplexPicklistAttributeMetadata -> Microsoft.Xrm.Sdk.Metadata.PicklistAttributeMetadata
                    var matchingType = _typeMapping.FirstOrDefault(
                        kv => kv.Key.EndsWith(cleanTypeName, StringComparison.OrdinalIgnoreCase));
                    if (!matchingType.Equals(default(KeyValuePair<string, Type>)))
                    {
                        // Deserialize into the matched type
                        //return jsonObject.ToObject(matchingType.Value, serializer);
                        return serializer.Deserialize(reader, matchingType.Value);
                    }
                }

                //return jsonObject.ToObject(objectType);
                return serializer.Deserialize(reader);
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                var jsonObject = Newtonsoft.Json.Linq.JObject.FromObject(value, serializer);
                jsonObject["@odata.type"] = "#" + value.GetType().FullName;
                jsonObject.WriteTo(writer);
            }
        }
    }

}
