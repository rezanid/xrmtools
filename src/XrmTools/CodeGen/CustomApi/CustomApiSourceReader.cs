#nullable enable
namespace XrmTools.CodeGen.CustomApi;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Shell;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using XrmTools.Analyzers;
using XrmTools.Helpers;
using XrmTools.WebApi.Entities;
using XrmTools.WebApi.Types;
using Api = XrmTools.WebApi.Entities.CustomApi;

internal sealed class CustomApiSourceSchemas
{
    public Dictionary<string, JObject> Inputs { get; } = new(StringComparer.Ordinal);
    public Dictionary<string, JObject> Outputs { get; } = new(StringComparer.Ordinal);
    public JObject Components { get; } = new();
    public List<string> Diagnostics { get; } = [];
}

internal interface ICustomApiSourceReader
{
    Task<CustomApiSourceSchemas> ReadAsync(Api api, string? typeName, string? assemblyName, CancellationToken token);
}

[Export(typeof(ICustomApiSourceReader))]
[method: ImportingConstructor]
internal sealed class CustomApiSourceReader(ICSharpXrmMetaParser parser) : ICustomApiSourceReader
{
    public async Task<CustomApiSourceSchemas> ReadAsync(Api api, string? typeName, string? assemblyName, CancellationToken token)
    {
        var result = new CustomApiSourceSchemas();
        if (string.IsNullOrWhiteSpace(typeName) || string.IsNullOrWhiteSpace(assemblyName)) return result;
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(token);
        var components = await AsyncServiceProvider.GlobalProvider.GetServiceAsync(typeof(SComponentModel)) as IComponentModel;
        var workspace = components?.GetService<VisualStudioWorkspace>();
        if (workspace == null) return result;
        var solution = workspace.CurrentSolution;
        // Filter projects before compiling; Roslyn reuses compilations for the current snapshot.
        var candidates = await Task.Run(() => SourceTypeResolver.FindAsync(solution, typeName!, assemblyName, token), token).ConfigureAwait(false);
        if (candidates.Count == 0) return result;
        if (candidates.Count > 1)
        {
            result.Diagnostics.Add("Multiple source projects match the plugin assembly and type. Generated from deployed metadata only.");
            return result;
        }
        var candidate = candidates[0];
        var source = parser.ParsePluginConfig(candidate.Symbol, candidate.Compilation)?.CustomApi;
        if (source == null || source.UniqueName != api.UniqueName)
        {
            result.Diagnostics.Add("Source Custom API identity does not match the deployed API. Generated from deployed metadata only.");
            return result;
        }
        if (source.BindingType != api.BindingType || source.IsFunction != api.IsFunction || source.BoundEntityLogicalName != api.BoundEntityLogicalName &&
            !(string.IsNullOrEmpty(source.BoundEntityLogicalName) && string.IsNullOrEmpty(api.BoundEntityLogicalName)))
        {
            result.Diagnostics.Add("Source operation binding differs from deployment. Generated from deployed metadata only.");
            return result;
        }
        return Enrich(api, source, candidate.Symbol, candidate.Compilation, token);
    }

    internal static CustomApiSourceSchemas Enrich(Api deployed, Api source, INamedTypeSymbol plugin, Compilation compilation, CancellationToken token)
    {
        var result = new CustomApiSourceSchemas();
        var builder = new EntitySchemaBuilder(compilation, result, token);
        foreach (var request in new[] { true, false })
        {
            var marker = request ? "CustomApiRequestAttribute" : "CustomApiResponseAttribute";
            var containers = plugin.GetTypeMembers().Where(t => Attribute(t, marker) != null).ToArray();
            if (containers.Length > 1)
            {
                result.Diagnostics.Add("Multiple " + marker + " classes found. Skipped source enrichment for this direction.");
                continue;
            }
            var properties = containers.SelectMany(t => t.GetMembers().OfType<IPropertySymbol>())
                .Where(p => !p.IsStatic && !p.IsIndexer && p.DeclaredAccessibility is Accessibility.Public or Accessibility.Internal or Accessibility.ProtectedAndInternal)
                .GroupBy(p => NamedString(Attribute(p, request ? "CustomApiRequestParameterAttribute" : "CustomApiResponsePropertyAttribute"), "UniqueName") ?? p.Name, StringComparer.Ordinal)
                .ToDictionary(g => g.Key, g => g.ToArray(), StringComparer.Ordinal);
            var fields = (request
                ? deployed.RequestParameters.Select(f => (f.UniqueName, f.Type, f.LogicalEntityName, Optional: (bool?)f.IsOptional))
                : deployed.ResponseProperties.Select(f => (f.UniqueName, f.Type, f.LogicalEntityName, Optional: (bool?)null))).ToArray();
            var deployedNames = new HashSet<string?>(fields.Select(f => f.UniqueName), StringComparer.Ordinal);
            foreach (var field in fields)
            {
                token.ThrowIfCancellationRequested();
                if (field.UniqueName == null || !properties.TryGetValue(field.UniqueName, out var matches)) continue;
                var sourceField = request
                    ? source.RequestParameters.Where(f => f.UniqueName == field.UniqueName).Select(f => (f.Type, f.LogicalEntityName, Optional: (bool?)f.IsOptional)).ToArray()
                    : source.ResponseProperties.Where(f => f.UniqueName == field.UniqueName).Select(f => (f.Type, f.LogicalEntityName, Optional: (bool?)null)).ToArray();
                if (matches.Length != 1 || sourceField.Length != 1 || sourceField[0].Type != field.Type || sourceField[0].Optional != field.Optional ||
                    (sourceField[0].LogicalEntityName ?? "") != (field.LogicalEntityName ?? ""))
                {
                    result.Diagnostics.Add("Source field '" + field.UniqueName + "' differs from deployment. Retained deployed schema.");
                    continue;
                }
                var property = matches[0];
                var schema = builder.Schema(property.Type);
                if (schema == null) continue;
                var metadata = Attribute(property, request ? "CustomApiRequestParameterAttribute" : "CustomApiResponsePropertyAttribute");
                var description = NamedString(metadata, "Description") ?? Summary(property);
                if (!string.IsNullOrWhiteSpace(description)) schema["description"] = description;
                var title = NamedString(metadata, "DisplayName");
                if (!string.IsNullOrWhiteSpace(title)) schema["title"] = title;
                (request ? result.Inputs : result.Outputs)[field.UniqueName] = schema;
            }
            foreach (var name in properties.Keys.Where(n => !deployedNames.Contains(n)))
                result.Diagnostics.Add("Source field '" + name + "' is not deployed and was omitted.");
        }
        return result;
    }

    internal static AttributeData? Attribute(ISymbol symbol, string name) => symbol.GetAttributes().FirstOrDefault(a =>
        a.AttributeClass?.ToDisplayString() == "XrmTools.Meta.Attributes." + name);
    internal static string? NamedString(AttributeData? attribute, string name) => attribute?.NamedArguments
        .Where(a => a.Key == name).Select(a => a.Value.Value as string).FirstOrDefault() is string value && value.Length > 0 ? value : null;
    internal static string? Summary(ISymbol symbol)
    {
        var xml = symbol.GetDocumentationCommentXml();
        if (string.IsNullOrWhiteSpace(xml)) return null;
        try { return XElement.Parse(xml).Element("summary")?.Value.Trim(); }
        catch (System.Xml.XmlException) { return null; }
    }

    private sealed class EntitySchemaBuilder(Compilation compilation, CustomApiSourceSchemas result, CancellationToken token)
    {
        private readonly Dictionary<ITypeSymbol, string> names = new(SymbolEqualityComparer.Default);
        private readonly Dictionary<SyntaxTree, SemanticModel> semanticModels = new();
        private int depth;
        public JObject? Schema(ITypeSymbol original)
        {
            token.ThrowIfCancellationRequested();
            var type = original is INamedTypeSymbol nullable && nullable.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T ? nullable.TypeArguments[0] : original;
            if (type.TypeKind == TypeKind.Error) return null;
            if (type.TypeKind == TypeKind.Enum)
            {
                var members = type.GetMembers().OfType<IFieldSymbol>().Where(f => f.HasConstantValue).ToArray();
                var groups = members.GroupBy(f => Convert.ToString(f.ConstantValue, System.Globalization.CultureInfo.InvariantCulture)!).ToArray();
                var descriptions = new JObject();
                foreach (var group in groups)
                    descriptions[group.Key] = string.Join("; ", group.Select(f =>
                        Summary(f) is string summary && !string.IsNullOrWhiteSpace(summary) ? f.Name + ": " + summary : f.Name));
                return new JObject
                {
                    ["type"] = "integer", ["format"] = "int32",
                    ["enum"] = new JArray(groups.Select(g => JToken.FromObject(g.First().ConstantValue!))),
                    ["x-enumNames"] = new JArray(groups.Select(g => g.First().Name)),
                    ["x-enumDescriptions"] = descriptions,
                };
            }
            var primitive = type.SpecialType switch
            {
                SpecialType.System_Boolean => "boolean", SpecialType.System_String => "string",
                SpecialType.System_Int32 => "integer", SpecialType.System_Double or SpecialType.System_Decimal => "number", _ => null,
            };
            if (primitive != null)
            {
                var value = new JObject { ["type"] = primitive };
                if (type.SpecialType == SpecialType.System_Int32) value["format"] = "int32";
                if (type.SpecialType == SpecialType.System_Double) value["format"] = "double";
                return value;
            }
            var fullName = type.ToDisplayString();
            if (fullName is "Microsoft.Xrm.Sdk.Entity" or "Microsoft.Xrm.Sdk.EntityReference")
                return new JObject { ["type"] = "object", ["additionalProperties"] = true };
            if (fullName == "Microsoft.Xrm.Sdk.EntityCollection")
                return new JObject { ["type"] = "array", ["items"] = new JObject { ["type"] = "object", ["additionalProperties"] = true } };
            if (fullName is "System.Guid" or "System.DateTime") return new JObject { ["type"] = "string", ["format"] = fullName == "System.Guid" ? "uuid" : "date-time" };
            if (fullName is "Microsoft.Xrm.Sdk.Money" or "Microsoft.Xrm.Sdk.OptionSetValue") return new JObject { ["type"] = fullName.EndsWith("Money", StringComparison.Ordinal) ? "number" : "integer" };
            ITypeSymbol? element = type is IArrayTypeSymbol array ? array.ElementType :
                type is INamedTypeSymbol named ? named.AllInterfaces.Concat([named]).FirstOrDefault(i => i.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T)?.TypeArguments[0] : null;
            if (element != null)
            {
                var items = Schema(element);
                return items == null ? null : new JObject { ["type"] = "array", ["items"] = items };
            }
            if (type is not INamedTypeSymbol entity || fullName == "Microsoft.Xrm.Sdk.Entity" || !IsEntity(entity)) return null;
            if (names.TryGetValue(type, out var existing)) return Reference(existing);
            if (depth >= 32 || names.Count >= 256)
            {
                result.Diagnostics.Add("Source schema traversal limit reached for '" + fullName + "'. Retained an open object.");
                return new JObject { ["type"] = "object", ["additionalProperties"] = true };
            }
            var name = "Source_" + System.Text.RegularExpressions.Regex.Replace(fullName, @"[^A-Za-z0-9_.-]", "_");
            while (result.Components.Property(name) != null) name += "_";
            names[type] = name;
            var properties = new JObject { ["@odata.type"] = new JObject { ["type"] = "string" } };
            var schema = new JObject { ["type"] = "object", ["properties"] = properties, ["additionalProperties"] = true };
            var summary = Summary(type);
            if (!string.IsNullOrWhiteSpace(summary)) schema["description"] = summary;
            result.Components[name] = schema; // Reserve before visiting properties to handle cycles.
            depth++;
            try
            {
                for (var current = entity; current != null && current.ToDisplayString() != "Microsoft.Xrm.Sdk.Entity"; current = current.BaseType)
                foreach (var property in current.GetMembers().OfType<IPropertySymbol>().Where(p => !p.IsStatic && !p.IsIndexer && p.DeclaredAccessibility == Accessibility.Public))
                {
                    var wireName = WireName(property);
                    if (wireName == null)
                    {
                        result.Diagnostics.Add("Cannot resolve the entity attribute name for '" + property.ToDisplayString() + "'. Omitted this property.");
                        continue;
                    }
                    if (properties.Property(wireName) != null) continue;
                    var child = Schema(property.Type);
                    if (child == null)
                    {
                        result.Diagnostics.Add("Unsupported entity property type for '" + property.ToDisplayString() + "'. Omitted this property.");
                        continue;
                    }
                    var docs = Summary(property);
                    if (!string.IsNullOrWhiteSpace(docs)) child["description"] = docs;
                    properties[wireName] = child;
                }
            }
            finally { depth--; }
            return Reference(name);
        }

        private static JObject Reference(string name) => new() { ["allOf"] = new JArray(new JObject { ["$ref"] = "#/components/schemas/" + name }) };
        private static bool IsEntity(INamedTypeSymbol type)
        {
            for (var current = type.BaseType; current != null; current = current.BaseType)
                if (current.ToDisplayString() == "Microsoft.Xrm.Sdk.Entity") return true;
            return false;
        }
        private string? WireName(IPropertySymbol property)
        {
            var explicitName = property.GetAttributes().FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == "Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute")
                ?.ConstructorArguments.FirstOrDefault().Value as string;
            if (!string.IsNullOrWhiteSpace(explicitName)) return explicitName;
            var found = new HashSet<string>(StringComparer.Ordinal);
            foreach (var reference in property.DeclaringSyntaxReferences)
            {
                var syntax = reference.GetSyntax(token);
                if (!semanticModels.TryGetValue(syntax.SyntaxTree, out var model))
                    semanticModels[syntax.SyntaxTree] = model = compilation.GetSemanticModel(syntax.SyntaxTree);
                foreach (var invocation in syntax.DescendantNodes().OfType<InvocationExpressionSyntax>())
                {
                    var method = model.GetSymbolInfo(invocation, token).Symbol as IMethodSymbol;
                    if (method?.ContainingType.ToDisplayString() != "Microsoft.Xrm.Sdk.Entity" || method.Name is not ("GetAttributeValue" or "SetAttributeValue" or "TryGetAttributeValue")) continue;
                    var argument = invocation.ArgumentList.Arguments.FirstOrDefault();
                    if (argument != null && model.GetConstantValue(argument.Expression, token).Value is string key) found.Add(key);
                }
            }
            return found.Count == 1 ? found.Single() : null;
        }
    }
}
