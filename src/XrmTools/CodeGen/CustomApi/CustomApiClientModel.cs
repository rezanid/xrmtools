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

internal enum CustomApiClientLanguage { PluginCSharp, TypeScript, Http, OData }

internal sealed class ClientField
{
    public string Name { get; init; } = "";
    public string Literal { get; init; } = "";
    public string Identifier { get; init; } = "";
    public string CsType { get; init; } = "";
    public string TsType { get; init; } = "";
    public string EdmTypeLiteral { get; init; } = "";
    public int StructuralProperty { get; init; }
    public bool Optional { get; init; }
    public bool ReferenceType { get; init; }
    public bool BoundTarget { get; init; }
    public CustomApiFieldType Type { get; init; }
    public string? LogicalName { get; init; }
}

internal sealed class CustomApiClientModel
{
    public string ClassName { get; init; } = "";
    public string OperationLiteral { get; init; } = "";
    public bool IsFunction { get; init; }
    public bool EntityBound { get; init; }
    public string BoundLogicalNameLiteral { get; init; } = "";
    public string BoundEdmTypeLiteral { get; init; } = "";
    public IReadOnlyList<ClientField> Inputs { get; init; } = [];
    public IReadOnlyList<ClientField> Outputs { get; init; } = [];
    public bool SingleEntityResponse => Outputs.Count == 1 && Outputs[0].Type == CustomApiFieldType.Entity;
    public bool SingleCollectionResponse => Outputs.Count == 1 && Outputs[0].Type == CustomApiFieldType.EntityCollection;
    public string SingleResponseLiteral => Outputs.Count == 1 ? Outputs[0].Literal : "";
    public IReadOnlyList<ClientField> WebInputs => Inputs.Where(field => !field.BoundTarget).ToArray();
    public string HttpMethod => IsFunction ? "GET" : "POST";
    public string HttpUrl { get; init; } = "";
    public string HttpBody { get; init; } = "";
    public string BaseUrl { get; init; } = "";
}

internal static class CustomApiClientModelBuilder
{
    public static CustomApiClientModel Build(Api api, CustomApiClientLanguage language, Uri serviceUrl,
        IReadOnlyDictionary<string, EntityMetadata> tables)
    {
        var operation = RequireName(api.UniqueName, "Custom API unique name");
        if (!Enum.IsDefined(typeof(Api.BindingTypes), api.BindingType)) throw new InvalidOperationException("Unsupported Custom API binding type.");
        if (language == CustomApiClientLanguage.TypeScript && api.BindingType == Api.BindingTypes.EntityCollection)
            throw new InvalidOperationException("TypeScript generation with Xrm.WebApi.online.execute does not support collection-bound APIs. Use the HTTP generator for this API.");
        if (api.BindingType != Api.BindingTypes.Global) RequireName(api.BoundEntityLogicalName, "Bound table logical name");
        if (api.IsFunction && api.ResponseProperties.Count == 0) throw new InvalidOperationException("A Custom API function must have a response property.");

        var inputs = api.RequestParameters.OrderBy(field => field.UniqueName, StringComparer.Ordinal)
            .Select(field => Field(field.UniqueName, field.Type, field.LogicalEntityName, field.IsOptional, false)).ToList();
        var outputs = api.ResponseProperties.OrderBy(field => field.UniqueName, StringComparer.Ordinal)
            .Select(field => Field(field.UniqueName, field.Type, field.LogicalEntityName, false, false)).ToList();
        if (api.BindingType == Api.BindingTypes.Entity)
        {
            var existing = inputs.SingleOrDefault(field => field.Name == "Target");
            if (existing != null && existing.Type != CustomApiFieldType.EntityReference)
                throw new InvalidOperationException("The bound Target parameter must be an EntityReference.");
            inputs.RemoveAll(field => field.Name == "Target");
            inputs.Insert(0, Field("Target", CustomApiFieldType.EntityReference, api.BoundEntityLogicalName, false, true));
        }
        ValidateNames(inputs);
        ValidateNames(outputs);
        if (language == CustomApiClientLanguage.TypeScript && inputs.Any(field => field.Name is "getMetadata" or "__proto__" ||
                api.BindingType == Api.BindingTypes.Entity && !field.BoundTarget && field.Name == "entity"))
            throw new InvalidOperationException("A parameter name conflicts with the Xrm.WebApi request structure. Use C# or HTTP for this API.");

        var path = operation;
        if (api.BindingType != Api.BindingTypes.Global && language is CustomApiClientLanguage.Http or CustomApiClientLanguage.OData)
        {
            if (!tables.TryGetValue(api.BoundEntityLogicalName!, out var table)) throw new InvalidOperationException("Bound table metadata is missing.");
            path = RequireName(table.EntitySetName, "Bound table entity set name") +
                (api.BindingType == Api.BindingTypes.Entity ? "({{recordId}})" : "") + "/Microsoft.Dynamics.CRM." + operation;
        }
        var webInputs = inputs.Where(field => !field.BoundTarget).ToList();
        var body = new JObject();
        foreach (var field in webInputs) body[field.Name] = Sample(field, tables);
        if (api.IsFunction)
        {
            path += "(" + string.Join(",", webInputs.Select((field, index) => field.Name + "=@p" + index)) + ")";
            if (webInputs.Count > 0)
                path += "?" + string.Join("&", webInputs.Select((field, index) => "@p" + index + "=" + FunctionSample(field, tables)));
        }
        return new CustomApiClientModel
        {
            ClassName = operation + "Client", OperationLiteral = JsonConvert.ToString(operation),
            IsFunction = api.IsFunction, EntityBound = api.BindingType == Api.BindingTypes.Entity,
            BoundLogicalNameLiteral = JsonConvert.ToString(api.BoundEntityLogicalName ?? ""),
            BoundEdmTypeLiteral = JsonConvert.ToString("mscrm." + api.BoundEntityLogicalName),
            Inputs = inputs, Outputs = outputs, HttpUrl = (language == CustomApiClientLanguage.OData ? "/" : "{{baseUrl}}/") + path,
            HttpBody = api.IsFunction ? "" : body.ToString(Formatting.Indented), BaseUrl = serviceUrl.AbsoluteUri.TrimEnd('/'),
        };
    }

    private static ClientField Field(string? name, CustomApiFieldType type, string? logicalName, bool optional, bool boundTarget)
    {
        name = RequireName(name, "Parameter unique name");
        if (!string.IsNullOrEmpty(logicalName)) RequireName(logicalName, "Parameter table logical name");
        var (cs, ts, edm, structural, reference) = type switch
        {
            CustomApiFieldType.Boolean => ("bool", "boolean", "Edm.Boolean", 1, false),
            CustomApiFieldType.DateTime => ("DateTime", "string", "Edm.DateTimeOffset", 1, false),
            CustomApiFieldType.Decimal => ("decimal", "number", "Edm.Decimal", 1, false),
            CustomApiFieldType.Float => ("double", "number", "Edm.Double", 1, false),
            CustomApiFieldType.Integer => ("int", "number", "Edm.Int32", 1, false),
            CustomApiFieldType.Money => ("Money", "number", "Edm.Decimal", 1, true),
            CustomApiFieldType.Picklist => ("OptionSetValue", "number", "Edm.Int32", 1, true),
            CustomApiFieldType.String => ("string", "string", "Edm.String", 1, true),
            CustomApiFieldType.Guid => ("Guid", "string", "Edm.Guid", 1, false),
            CustomApiFieldType.StringArray => ("string[]", "string[]", "Collection(Edm.String)", 4, true),
            CustomApiFieldType.Entity => ("Entity", "Record<string, unknown>", "mscrm." + (logicalName ?? "crmbaseentity"), 5, true),
            CustomApiFieldType.EntityReference => ("EntityReference", "Record<string, unknown>", "mscrm." + (logicalName ?? "crmbaseentity"), 5, true),
            CustomApiFieldType.EntityCollection => ("EntityCollection", "Array<Record<string, unknown>>", "Collection(mscrm.crmbaseentity)", 4, true),
            _ => throw new InvalidOperationException("Unsupported Custom API parameter type: " + type),
        };
        // Prefixing properties also avoids collisions with class names and System.Object members.
        var identifier = "Parameter_" + name;
        return new ClientField
        {
            Name = name, Literal = JsonConvert.ToString(name), Identifier = identifier,
            CsType = cs, TsType = ts, EdmTypeLiteral = JsonConvert.ToString(edm),
            StructuralProperty = structural, Optional = optional, ReferenceType = reference,
            BoundTarget = boundTarget, Type = type, LogicalName = logicalName,
        };
    }

    private static string RequireName(string? name, string description)
    {
        if (name == null || !Regex.IsMatch(name, @"\A[A-Za-z_][A-Za-z0-9_]*\z"))
            throw new InvalidOperationException(description + " is missing or is not a supported identifier.");
        return name;
    }

    private static void ValidateNames(IEnumerable<ClientField> fields)
    {
        if (fields.GroupBy(field => field.Name, StringComparer.Ordinal).Any(group => group.Count() != 1))
            throw new InvalidOperationException("Custom API metadata contains duplicate parameter unique names.");
    }

    private static JToken Sample(ClientField field, IReadOnlyDictionary<string, EntityMetadata> tables) => field.Type switch
    {
        CustomApiFieldType.Boolean => new JValue(false),
        CustomApiFieldType.DateTime => new JValue("2000-01-01T00:00:00Z"),
        CustomApiFieldType.Decimal or CustomApiFieldType.Float or CustomApiFieldType.Integer or CustomApiFieldType.Money or CustomApiFieldType.Picklist => new JValue(0),
        CustomApiFieldType.Guid => new JValue("00000000-0000-0000-0000-000000000000"),
        CustomApiFieldType.String => new JValue("REPLACE_WITH_VALUE"),
        CustomApiFieldType.StringArray => new JArray("REPLACE_WITH_VALUE"),
        CustomApiFieldType.EntityCollection => new JArray(EntitySample(field, tables)),
        _ => EntitySample(field, tables),
    };

    private static JObject EntitySample(ClientField field, IReadOnlyDictionary<string, EntityMetadata> tables)
    {
        var logicalName = field.LogicalName ?? "REPLACE_WITH_TABLE_LOGICAL_NAME";
        var primaryId = tables.TryGetValue(logicalName, out var table) ? table.PrimaryIdAttribute : "REPLACE_WITH_PRIMARY_ID_ATTRIBUTE";
        return new JObject
        {
            ["@odata.type"] = "Microsoft.Dynamics.CRM." + logicalName,
            [primaryId ?? "REPLACE_WITH_PRIMARY_ID_ATTRIBUTE"] = "00000000-0000-0000-0000-000000000000",
        };
    }

    private static string FunctionSample(ClientField field, IReadOnlyDictionary<string, EntityMetadata> tables)
    {
        var sample = Sample(field, tables);
        var literal = field.Type switch
        {
            CustomApiFieldType.String => "'" + sample.Value<string>()!.Replace("'", "''") + "'",
            CustomApiFieldType.Guid or CustomApiFieldType.DateTime => sample.Value<string>()!,
            _ => sample.ToString(Formatting.None),
        };
        return Uri.EscapeDataString(literal);
    }
}
