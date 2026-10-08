#nullable enable
namespace XrmTools.CodeGen.CustomApi;

using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
using System.Linq;

public enum OpenApiSpecificationVersion
{
    [Description("OpenAPI 3.0.4 (compatibility)")] V3_0,
    [Description("OpenAPI 3.2.1")] V3_2,
}

/// <summary>Applies schema dialect differences only to schema locations, never to example payloads.</summary>
internal static class OpenApiSchemaVersion
{
    internal static bool IsLatest(OpenApiSpecificationVersion version) => version switch
    {
        OpenApiSpecificationVersion.V3_0 => false,
        OpenApiSpecificationVersion.V3_2 => true,
        _ => throw new ArgumentOutOfRangeException(nameof(version)),
    };

    internal static void Apply(JObject document, OpenApiSpecificationVersion version)
    {
        var latest = IsLatest(version);
        if (document["components"]?["schemas"] is JObject components)
            foreach (var component in components.Properties()) Schema(component.Value, latest);
        if (document["paths"] is not JObject paths) return;
        foreach (var path in paths.Properties())
        {
            Parameters(path.Value["parameters"], latest);
            foreach (var method in new[] { "get", "post", "put", "patch", "delete", "head", "options", "trace" })
            {
                if (path.Value[method] is not JObject operation) continue;
                Parameters(operation["parameters"], latest);
                Content(operation["requestBody"]?["content"], latest);
                if (operation["responses"] is JObject responses)
                    foreach (var response in responses.Properties()) Content(response.Value["content"], latest);
            }
        }
    }

    private static void Parameters(JToken? parameters, bool latest)
    {
        if (parameters is not JArray array) return;
        foreach (var parameter in array)
        {
            Schema(parameter["schema"], latest);
            Schema(parameter["x-odata-value-schema"], latest);
            Content(parameter["content"], latest);
        }
    }

    private static void Content(JToken? content, bool latest)
    {
        if (content is JObject mediaTypes)
            foreach (var media in mediaTypes.Properties()) Schema(media.Value["schema"], latest);
    }

    private static void Schema(JToken? token, bool latest)
    {
        if (token is not JObject schema) return;
        foreach (var key in new[] { "properties", "$defs", "patternProperties" })
            if (schema[key] is JObject properties)
                foreach (var property in properties.Properties()) Schema(property.Value, latest);
        foreach (var key in new[] { "items", "additionalProperties", "propertyNames", "not", "if", "then", "else" }) Schema(schema[key], latest);
        foreach (var key in new[] { "allOf", "anyOf", "oneOf", "prefixItems" })
            if (schema[key] is JArray children)
                foreach (var child in children) Schema(child, latest);

        if (schema["allOf"] is JArray composition && composition.Count == 1 && composition[0] is JObject reference && reference.Property("$ref") != null)
        {
            var metadata = new JObject(schema.Properties().Where(p => p.Name != "allOf").Select(p => new JProperty(p.Name, p.Value.DeepClone())));
            schema.RemoveAll();
            if (latest || metadata.Count == 0)
            {
                schema["$ref"] = reference["$ref"]!.DeepClone();
                foreach (var property in metadata.Properties().ToArray()) schema.Add(property.Name, property.Value.DeepClone());
            }
            else schema["allOf"] = new JArray(reference.DeepClone(), metadata);
        }
        if (!latest) return;
        if (schema.Property("example") is JProperty example)
        {
            schema["examples"] = new JArray(example.Value.DeepClone());
            example.Remove();
        }
        // In 3.0 nullable only modifies an explicitly declared type.
        // Requiredness remains independent of whether a value can be null.
        if ((bool?)schema["nullable"] == true && schema["type"] is JValue type && type.Value is string typeName)
        {
            schema["type"] = new JArray(typeName, "null");
            if (schema["enum"] is JArray enumValues && !enumValues.Any(v => v.Type == JTokenType.Null)) enumValues.Add(JValue.CreateNull());
        }
        schema.Remove("nullable");
        if (schema["enum"] is JArray values && schema["x-enumDescriptions"] is JObject descriptions && schema["oneOf"] == null)
        {
            var names = schema["x-enumNames"] as JArray;
            schema["oneOf"] = new JArray(values.Select((value, index) =>
            {
                var member = new JObject { ["const"] = value.DeepClone() };
                if (names != null && index < names.Count) member["title"] = names[index]!.DeepClone();
                var key = value.Type == JTokenType.String ? value.Value<string>()! : value.ToString(Newtonsoft.Json.Formatting.None);
                if (descriptions[key] != null) member["description"] = descriptions[key]!.DeepClone();
                return member;
            }));
        }
    }
}
