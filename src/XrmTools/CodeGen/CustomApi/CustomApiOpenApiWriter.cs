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
    public static string Write(Api api, Uri? serviceUrl, IReadOnlyDictionary<string, EntityMetadata> tables, CustomApiSourceSchemas? source = null, OpenApiSpecificationVersion version = OpenApiSpecificationVersion.V3_0)
        => Build(api, serviceUrl, tables, source, version).ToString(Formatting.Indented);

    internal static JObject Build(Api api, Uri? serviceUrl, IReadOnlyDictionary<string, EntityMetadata> tables, CustomApiSourceSchemas? source = null, OpenApiSpecificationVersion version = OpenApiSpecificationVersion.V3_0)
    {
        var latest = OpenApiSchemaVersion.IsLatest(version);
        ODataFunctionParameters.Validate(api);
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
        var aliases = new List<string>();
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
            if (api.IsFunction)
            {
                aliases.Add(fieldName + "=@" + fieldName);
                parameters.Add(ODataFunctionParameters.Parameter(field, (JObject)inputs[fieldName]!, tables, version));
            }
        }
        if (api.IsFunction) path += "(" + string.Join(",", aliases) + ")";
        var operation = new JObject { ["operationId"] = name, ["summary"] = string.IsNullOrWhiteSpace(api.DisplayName) ? name : api.DisplayName,
            ["x-dataverse-is-private"] = api.IsPrivate };
        if (!string.IsNullOrWhiteSpace(api.Description)) operation["description"] = api.Description;
        if (!string.IsNullOrWhiteSpace(api.ExecutePrivilegeName)) operation["x-dataverse-execute-privilege"] = api.ExecutePrivilegeName;
        if (parameters.Count > 0) operation["parameters"] = parameters;
        if (inputs.Count > 0 && !api.IsFunction)
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
            : new JObject { ["200"] = new JObject { ["description"] = api.IsFunction ? "Function response." : "Action response.", ["content"] = Content(response) } };
        if (api.IsFunction)
        {
            var query = string.Join("&", parameters.OfType<JObject>().Where(p => (string?)p["in"] == "query")
                .Select(ODataFunctionParameters.Assignment));
            var url = (serviceUrl?.AbsoluteUri.TrimEnd('/') ?? "https://YOUR-ORGANIZATION.crm.dynamics.com/api/data/v9.2") + path + (query.Length > 0 ? "?" + query : "");
            operation["x-codeSamples"] = new JArray(new JObject { ["lang"] = "HTTP", ["label"] = "OData function request", ["source"] =
                "GET " + url + "\nAuthorization: Bearer REPLACE_WITH_ACCESS_TOKEN\nAccept: application/json\nOData-Version: 4.0\nOData-MaxVersion: 4.0" });
        }
        operation["responses"]!["4XX"] = ErrorResponse("Dataverse rejected the request.");
        operation["responses"]!["5XX"] = ErrorResponse("A server or plugin execution error occurred.");
        var schemas = source == null ? new JObject() : (JObject)source.Components.DeepClone();
        schemas["DataverseError"] = ErrorSchema();
        var document = new JObject
        {
            ["openapi"] = latest ? "3.2.1" : "3.0.4",
            ["info"] = new JObject { ["title"] = api.DisplayName ?? name, ["version"] = "1.0.0" },
            ["servers"] = serviceUrl != null
                ? new JArray(new JObject { ["url"] = serviceUrl.AbsoluteUri.TrimEnd('/') })
                : new JArray(new JObject { ["url"] = "https://{organization}.crm.dynamics.com/api/data/v9.2",
                    ["description"] = "Replace the organization variable with your Dataverse organization, or edit this URL for your region.",
                    ["variables"] = new JObject { ["organization"] = new JObject { ["default"] = "YOUR-ORGANIZATION" } } }),
            ["paths"] = new JObject { [path] = new JObject { [api.IsFunction ? "get" : "post"] = operation } },
            ["components"] = new JObject
            {
                ["securitySchemes"] = new JObject { ["bearerAuth"] = new JObject { ["type"] = "http", ["scheme"] = "bearer" } },
                ["schemas"] = schemas,
            },
            ["security"] = new JArray(new JObject { ["bearerAuth"] = new JArray() }),
        };
        if (source?.Diagnostics.Count > 0) document["x-xrmtools-diagnostics"] = new JArray(source.Diagnostics.Distinct(StringComparer.Ordinal));
        OpenApiSchemaVersion.Apply(document, version);
        return document;
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
