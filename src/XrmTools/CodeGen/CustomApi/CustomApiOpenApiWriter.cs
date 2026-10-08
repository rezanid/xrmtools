#nullable enable
namespace XrmTools.CodeGen.CustomApi;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using XrmTools.WebApi.Entities;
using XrmTools.WebApi.Types;
using Api = XrmTools.WebApi.Entities.CustomApi;

/// <summary>Pure metadata-to-OpenAPI conversion. No network, workspace or UI dependencies.</summary>
internal static class CustomApiOpenApiWriter
{
    public static string Write(Api api, Uri serviceUrl, IReadOnlyDictionary<string, EntityMetadata> tables, CustomApiSourceSchemas? source = null)
    {
        if (api.IsFunction) throw new InvalidOperationException("OpenAPI generation for functions is not available yet. Currently, only Custom API actions are supported.");
        var name = RequireName(api.UniqueName);
        if (!Enum.IsDefined(typeof(Api.BindingTypes), api.BindingType)) throw new InvalidOperationException("Unsupported Custom API binding type.");
        var path = "/" + name;
        var parameters = new JArray();
        if (api.BindingType != Api.BindingTypes.Global)
        {
            var logicalName = RequireName(api.BoundEntityLogicalName);
            if (!tables.TryGetValue(logicalName, out var table)) throw new InvalidOperationException("Bound table metadata is missing.");
            path = "/" + RequireName(table.EntitySetName);
            if (api.BindingType == Api.BindingTypes.Entity)
            {
                path += "({recordId})";
                parameters.Add(new JObject { ["name"] = "recordId", ["in"] = "path", ["required"] = true,
                    ["schema"] = new JObject { ["type"] = "string", ["format"] = "uuid" } });
            }
            path += "/Microsoft.Dynamics.CRM." + name;
        }

        var inputs = new JObject();
        var required = new JArray();
        foreach (var field in api.RequestParameters.OrderBy(p => p.UniqueName, StringComparer.Ordinal))
        {
            var fieldName = RequireName(field.UniqueName);
            if (api.BindingType == Api.BindingTypes.Entity && fieldName == "Target")
            {
                if (field.Type != CustomApiFieldType.EntityReference) throw new InvalidOperationException("The bound Target parameter must be an EntityReference.");
                continue;
            }
            AddField(inputs, fieldName, Describe(EnrichSchema(Schema(field.Type, field.LogicalEntityName, tables), source?.Inputs, fieldName), field.DisplayName, field.Description));
            if (!field.IsOptional) required.Add(fieldName);
        }
        var operation = new JObject { ["operationId"] = name, ["summary"] = string.IsNullOrWhiteSpace(api.DisplayName) ? name : api.DisplayName,
            ["x-dataverse-is-private"] = api.IsPrivate };
        if (!string.IsNullOrWhiteSpace(api.Description)) operation["description"] = api.Description;
        if (!string.IsNullOrWhiteSpace(api.ExecutePrivilegeName)) operation["x-dataverse-execute-privilege"] = api.ExecutePrivilegeName;
        if (parameters.Count > 0) operation["parameters"] = parameters;
        if (inputs.Count > 0)
        {
            var schema = ObjectSchema(inputs);
            if (required.Count > 0) schema["required"] = required;
            operation["requestBody"] = new JObject { ["required"] = required.Count > 0, ["content"] = Content(schema) };
        }
        var outputs = api.ResponseProperties.OrderBy(p => p.UniqueName, StringComparer.Ordinal).ToArray();
        var outputProperties = new JObject();
        foreach (var field in outputs)
            AddField(outputProperties, RequireName(field.UniqueName), Describe(EnrichSchema(Schema(field.Type, field.LogicalEntityName, tables), source?.Outputs, field.UniqueName!), field.DisplayName, field.Description));
        JObject response;
        if (outputs.Length == 1 && outputs[0].Type == CustomApiFieldType.Entity)
            response = (JObject)outputProperties.Properties().Single().Value.DeepClone();
        else if (outputs.Length == 1 && outputs[0].Type == CustomApiFieldType.EntityCollection)
            response = ObjectSchema(new JObject { ["value"] = outputProperties.Properties().Single().Value.DeepClone() });
        else response = ObjectSchema(outputProperties);
        operation["responses"] = outputs.Length == 0
            ? new JObject { ["204"] = new JObject { ["description"] = "Action completed successfully." } }
            : new JObject { ["200"] = new JObject { ["description"] = "Action response.", ["content"] = Content(response) } };
        operation["responses"]!["4XX"] = ErrorResponse("Dataverse rejected the request.");
        operation["responses"]!["5XX"] = ErrorResponse("A server or plugin execution error occurred.");
        var schemas = source == null ? new JObject() : (JObject)source.Components.DeepClone();
        schemas["DataverseError"] = ErrorSchema();
        var document = new JObject
        {
            ["openapi"] = "3.0.4",
            ["info"] = new JObject { ["title"] = api.DisplayName ?? name, ["version"] = "1.0.0" },
            ["servers"] = new JArray(new JObject { ["url"] = serviceUrl.AbsoluteUri.TrimEnd('/') }),
            ["paths"] = new JObject { [path] = new JObject { ["post"] = operation } },
            ["components"] = new JObject
            {
                ["securitySchemes"] = new JObject { ["bearerAuth"] = new JObject { ["type"] = "http", ["scheme"] = "bearer" } },
                ["schemas"] = schemas,
            },
            ["security"] = new JArray(new JObject { ["bearerAuth"] = new JArray() }),
        };
        if (source?.Diagnostics.Count > 0) document["x-xrmtools-diagnostics"] = new JArray(source.Diagnostics.Distinct(StringComparer.Ordinal));
        NormalizeReferences(document);
        return document.ToString(Formatting.Indented);
    }

    // OpenAPI 3.0 ignores Reference Object siblings. Keep local annotations in a
    // separate schema, but emit a direct reference when there are no annotations.
    private static void NormalizeReferences(JToken token)
    {
        foreach (var child in token.Children().ToArray()) NormalizeReferences(child);
        if (token is not JObject schema || schema["allOf"] is not JArray composition || composition.Count != 1 ||
            composition[0] is not JObject reference || reference.Property("$ref") == null) return;
        var metadata = new JObject(schema.Properties().Where(p => p.Name != "allOf").Select(p => new JProperty(p.Name, p.Value.DeepClone())));
        if (metadata.Count == 0)
        {
            schema.RemoveAll();
            schema["$ref"] = reference["$ref"]!.DeepClone();
        }
        else
        {
            schema.RemoveAll();
            schema["allOf"] = new JArray(reference.DeepClone(), metadata);
        }
    }

    private static JObject EnrichSchema(JObject schema, IReadOnlyDictionary<string, JObject>? source, string name)
    {
        if (source == null || !source.TryGetValue(name, out var rich)) return schema;
        // Entity references carry their shape in components; avoid duplicating generic entity properties.
        if (rich["allOf"] != null)
        {
            var reference = (JObject)rich.DeepClone();
            if (schema["x-dataverse-logical-name"] != null) reference["x-dataverse-logical-name"] = schema["x-dataverse-logical-name"]!.DeepClone();
            return reference;
        }
        schema.Merge(rich, new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Replace });
        if (rich["items"] != null) schema["items"] = rich["items"]!.DeepClone();
        return schema;
    }

    private static JObject Content(JObject schema) => new() { ["application/json"] = new JObject { ["schema"] = schema } };
    private static JObject ErrorResponse(string description) => new()
    {
        ["description"] = description,
        ["content"] = Content(new JObject { ["$ref"] = "#/components/schemas/DataverseError" }),
    };

    private static JObject ErrorSchema()
    {
        var details = ObjectSchema(new JObject
        {
            ["code"] = new JObject { ["type"] = "string", ["description"] = "Dataverse error code, distinct from the HTTP status code. May be empty." },
            ["message"] = new JObject { ["type"] = "string", ["description"] = "A message describing the error." },
        });
        details["required"] = new JArray("code", "message");
        // Diagnostic annotations are conditional and their values can have different types.
        details["additionalProperties"] = true;
        var envelope = ObjectSchema(new JObject { ["error"] = details });
        envelope["required"] = new JArray("error");
        envelope["additionalProperties"] = true;
        return envelope;
    }
    private static JObject ObjectSchema(JObject properties) => new() { ["type"] = "object", ["properties"] = properties };
    private static JObject Describe(JObject schema, string? title, string? description)
    {
        if (!string.IsNullOrWhiteSpace(title)) schema["title"] = title;
        if (!string.IsNullOrWhiteSpace(description)) schema["description"] = description;
        return schema;
    }
    private static void AddField(JObject properties, string name, JObject schema)
    {
        if (properties.Property(name) != null) throw new InvalidOperationException("Duplicate parameter unique name: " + name);
        properties.Add(name, schema);
    }
    internal static string RequireName(string? name) => name != null && Regex.IsMatch(name, @"\A[A-Za-z_][A-Za-z0-9_]*\z")
        ? name : throw new InvalidOperationException("Custom API metadata contains a missing or unsupported identifier.");

    private static JObject Schema(CustomApiFieldType type, string? logicalName, IReadOnlyDictionary<string, EntityMetadata> tables)
    {
        switch (type)
        {
            case CustomApiFieldType.Boolean: return new JObject { ["type"] = "boolean" };
            case CustomApiFieldType.Integer:
            case CustomApiFieldType.Picklist: return new JObject { ["type"] = "integer", ["format"] = "int32" };
            case CustomApiFieldType.Float: return new JObject { ["type"] = "number", ["format"] = "double" };
            case CustomApiFieldType.Decimal:
            case CustomApiFieldType.Money: return new JObject { ["type"] = "number" };
            case CustomApiFieldType.String: return new JObject { ["type"] = "string" };
            case CustomApiFieldType.Guid: return new JObject { ["type"] = "string", ["format"] = "uuid" };
            case CustomApiFieldType.DateTime: return new JObject { ["type"] = "string", ["format"] = "date-time" };
            case CustomApiFieldType.StringArray: return new JObject { ["type"] = "array", ["items"] = new JObject { ["type"] = "string" } };
            case CustomApiFieldType.EntityCollection: return new JObject { ["type"] = "array", ["items"] = Schema(CustomApiFieldType.Entity, logicalName, tables) };
            case CustomApiFieldType.Entity:
            case CustomApiFieldType.EntityReference:
                var properties = new JObject { ["@odata.type"] = new JObject { ["type"] = "string" } };
                if (!string.IsNullOrWhiteSpace(logicalName))
                {
                    RequireName(logicalName);
                    properties["@odata.type"]!["example"] = "Microsoft.Dynamics.CRM." + logicalName;
                    if (tables.TryGetValue(logicalName!, out var table) && !string.IsNullOrWhiteSpace(table.PrimaryIdAttribute))
                        properties[RequireName(table.PrimaryIdAttribute)] = new JObject { ["type"] = "string", ["format"] = "uuid" };
                }
                var entity = ObjectSchema(properties);
                entity["additionalProperties"] = true;
                if (!string.IsNullOrWhiteSpace(logicalName)) entity["x-dataverse-logical-name"] = logicalName;
                return entity;
            default: throw new InvalidOperationException("Unsupported Custom API parameter type: " + type);
        }
    }
}
