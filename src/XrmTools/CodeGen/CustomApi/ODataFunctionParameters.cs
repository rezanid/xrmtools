#nullable enable
namespace XrmTools.CodeGen.CustomApi;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using XrmTools.WebApi.Entities;
using XrmTools.WebApi.Types;

/// <summary>Describes serialized function alias values rather than ordinary query values.</summary>
internal static class ODataFunctionParameters
{
    internal static void Validate(CustomApi api)
    {
        if (!api.IsFunction) return;
        if (api.ResponseProperties.Count == 0) throw new InvalidOperationException("A Custom API function must have at least one response property.");
        foreach (var field in api.RequestParameters)
            if (field.Type is CustomApiFieldType.Entity or CustomApiFieldType.EntityCollection && string.IsNullOrWhiteSpace(field.LogicalEntityName))
                throw new InvalidOperationException("Function input '" + field.UniqueName + "' is an open entity type. Dataverse functions do not support open entity request parameters. Use an action instead.");
    }

    internal static JObject Parameter(CustomApiRequestParameter field, JObject valueSchema, IReadOnlyDictionary<string, EntityMetadata> tables, OpenApiSpecificationVersion version = OpenApiSpecificationVersion.V3_0)
    {
        var name = CustomApiOpenApiWriter.RequireName(field.UniqueName);
        var schema = new JObject { ["type"] = "string" };
        var pattern = field.Type switch
        {
            CustomApiFieldType.String => "'(?:[^']|'')*'",
            CustomApiFieldType.Boolean => "(?:true|false)",
            CustomApiFieldType.Integer or CustomApiFieldType.Picklist => "-?[0-9]+",
            CustomApiFieldType.Float => "(?:-?[0-9]+(?:\\.[0-9]+)?(?:[eE][+-]?[0-9]+)?|INF|-INF|NaN)",
            CustomApiFieldType.Decimal or CustomApiFieldType.Money => "-?[0-9]+(?:\\.[0-9]+)?",
            _ => null,
        };
        if (pattern != null) schema["pattern"] = "^(?:" + pattern + (field.IsOptional ? "|null" : "") + ")$";
        // Keep documentation and enum labels visible even though the wire value is text.
        foreach (var extension in valueSchema.Properties().Where(p => p.Name.StartsWith("x-enum", StringComparison.Ordinal)))
            schema[extension.Name] = extension.Value.DeepClone();
        if (valueSchema["title"] != null) schema["title"] = valueSchema["title"]!.DeepClone();
        if (valueSchema["enum"] is JArray values)
        {
            var literals = new JArray(values.Select(v => v.ToString(Formatting.None)));
            schema["enum"] = literals;
            if (field.IsOptional)
            {
                literals.Add("null");
                if (schema["x-enumNames"] is JArray names) names.Add("Null");
                if (schema["x-enumDescriptions"] is JObject descriptions) descriptions["null"] = "Null (no value supplied)";
            }
        }
        var description = (string?)valueSchema["description"];
        var serialization = field.Type == CustomApiFieldType.String
            ? "Supply an OData string literal: surround it with single quotes and double embedded apostrophes. Empty string is ''."
            : field.Type is CustomApiFieldType.Entity or CustomApiFieldType.EntityReference or CustomApiFieldType.EntityCollection or CustomApiFieldType.StringArray
                ? "Supply compact JSON as the alias value; do not surround the JSON with single quotes."
                : "Supply an unquoted OData literal for " + field.Type + ".";
        var optional = field.IsOptional ? " Omitting this alias assignment supplies null; the parameter stays in the function signature. Explicit null is the unquoted literal null." : " This alias assignment is required.";
        var parameter = new JObject
        {
            ["name"] = "@" + name, ["in"] = "query", ["required"] = !field.IsOptional,
            ["style"] = "form", ["explode"] = false, ["allowReserved"] = false,
            ["description"] = (string.IsNullOrWhiteSpace(description) ? "" : description + "\n\n") + serialization + optional + " Enter the literal without URL encoding; encode it once when constructing the URL.",
            ["schema"] = schema, ["example"] = Example(field, tables, valueSchema),
            ["x-odata-parameter-name"] = name, ["x-odata-value-schema"] = valueSchema.DeepClone(),
        };
        if (OpenApiSchemaVersion.IsLatest(version))
        {
            var examples = new JObject { ["literal"] = SerializedExample(parameter, (string)parameter["example"]!, "OData literal") };
            if (field.IsOptional) examples["null"] = SerializedExample(parameter, "null", "No value");
            if (field.Type == CustomApiFieldType.String) examples["emptyString"] = SerializedExample(parameter, "''", "Empty string");
            parameter.Remove("example");
            parameter["examples"] = examples;
        }
        return parameter;
    }

    private static JObject SerializedExample(JObject parameter, string literal, string summary) => new()
    {
        ["summary"] = summary,
        // The parameter's data type is an OData literal string, not its logical value.
        ["dataValue"] = literal,
        ["serializedValue"] = (string)parameter["name"]! + "=" + Uri.EscapeDataString(literal),
    };

    internal static string Assignment(JObject parameter) =>
        (string?)parameter["examples"]?["literal"]?["serializedValue"]
        ?? (string)parameter["name"]! + "=" + Uri.EscapeDataString((string)parameter["example"]!);

    internal static string Serialize(CustomApiFieldType type, JToken value)
    {
        if (value.Type == JTokenType.Null) return "null";
        if (type == CustomApiFieldType.String)
            return "'" + value.Value<string>()!.Replace("'", "''") + "'";
        if (type is CustomApiFieldType.Guid or CustomApiFieldType.DateTime) return value.Value<string>()!;
        if (type == CustomApiFieldType.Float)
        {
            var number = value.Value<double>();
            if (double.IsPositiveInfinity(number)) return "INF";
            if (double.IsNegativeInfinity(number)) return "-INF";
            if (double.IsNaN(number)) return "NaN";
        }
        return value.ToString(Formatting.None);
    }

    private static string Example(CustomApiRequestParameter field, IReadOnlyDictionary<string, EntityMetadata> tables, JObject schema)
    {
        if (schema["enum"] is JArray values && values.Count > 0) return Serialize(field.Type, values[0]);
        JToken value;
        switch (field.Type)
        {
            case CustomApiFieldType.String: value = new JValue("O'Brien & café/+%"); break;
            case CustomApiFieldType.Boolean: value = new JValue(true); break;
            case CustomApiFieldType.Guid: value = new JValue("00000000-0000-0000-0000-000000000000"); break;
            case CustomApiFieldType.DateTime: value = new JValue("2000-01-01T00:00:00Z"); break;
            case CustomApiFieldType.StringArray: value = new JArray("O'Brien", "café/+%"); break;
            case CustomApiFieldType.Entity:
            case CustomApiFieldType.EntityReference:
            case CustomApiFieldType.EntityCollection:
                var setName = field.LogicalEntityName != null && tables.TryGetValue(field.LogicalEntityName, out var table)
                    ? table.EntitySetName : "REPLACE_WITH_ENTITY_SET_NAME";
                var entity = new JObject { ["@odata.id"] = setName + "(00000000-0000-0000-0000-000000000000)" };
                value = field.Type == CustomApiFieldType.EntityCollection ? new JArray(entity) : entity;
                break;
            default: value = new JValue(0); break;
        }
        return Serialize(field.Type, value);
    }
}
