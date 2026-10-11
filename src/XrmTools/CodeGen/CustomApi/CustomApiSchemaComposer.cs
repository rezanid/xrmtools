#nullable enable
namespace XrmTools.CodeGen.CustomApi;

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>Combines source schema graphs without conflating types with the same short name.</summary>
internal static class CustomApiSchemaComposer
{
    private const string Prefix = "#/components/schemas/";

    internal static Dictionary<string, CustomApiSourceSchemas?> Prepare(CustomApiOpenApiInput[] inputs, JObject destination)
    {
        var names = inputs.Select(i => CustomApiOpenApiWriter.RequireName(i.Api.UniqueName)).ToArray();
        if (names.Distinct(StringComparer.Ordinal).Count() != names.Length)
            throw new InvalidOperationException("Duplicate operation ID in the selected APIs.");
        var definitions = inputs.SelectMany(i => i.Source?.Components.Properties() ?? Enumerable.Empty<JProperty>()).ToArray();
        var conflicts = new HashSet<string>(definitions.GroupBy(p => p.Name, StringComparer.Ordinal)
            .Where(g => destination.Property(g.Key) != null || g.Any(p => !JToken.DeepEquals(p.Value, g.First().Value)))
            .Select(g => g.Key), StringComparer.Ordinal);
        // A seemingly identical parent schema can refer to a conflicting child. Namespace that parent too.
        bool changed;
        do
        {
            changed = false;
            foreach (var definition in definitions)
                if (References(definition.Value).Any(conflicts.Contains)) changed |= conflicts.Add(definition.Name);
        } while (changed);

        var reserved = new HashSet<string>(definitions.Select(p => p.Name).Concat(destination.Properties().Select(p => p.Name)), StringComparer.Ordinal);
        var result = new Dictionary<string, CustomApiSourceSchemas?>(StringComparer.Ordinal);
        foreach (var input in inputs)
        {
            var name = input.Api.UniqueName!;
            if (input.Source == null) { result.Add(name, null); continue; }
            var map = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var property in input.Source.Components.Properties().OrderBy(p => p.Name, StringComparer.Ordinal))
            {
                var target = property.Name;
                if (conflicts.Contains(target))
                {
                    target = name + "." + target;
                    while (!reserved.Add(target)) target += "_";
                }
                map.Add(property.Name, target);
            }
            var source = new CustomApiSourceSchemas();
            foreach (var pair in input.Source.Inputs) source.Inputs.Add(pair.Key, Rewrite(pair.Value, map));
            foreach (var pair in input.Source.Outputs) source.Outputs.Add(pair.Key, Rewrite(pair.Value, map));
            foreach (var property in input.Source.Components.Properties())
            {
                var value = Rewrite((JObject)property.Value, map);
                var target = map[property.Name];
                if (destination[target] == null) destination[target] = value;
                else if (!JToken.DeepEquals(destination[target], value)) throw new InvalidOperationException("Conflicting schema: " + target);
            }
            result.Add(name, source);
        }
        return result;
    }

    private static JObject Rewrite(JObject schema, IReadOnlyDictionary<string, string> map)
    {
        var copy = (JObject)schema.DeepClone();
        Visit(copy, reference =>
        {
            var value = (string?)reference.Value;
            if (value != null && value.StartsWith(Prefix, StringComparison.Ordinal))
            {
                var name = value.Substring(Prefix.Length).Replace("~1", "/").Replace("~0", "~");
                if (map.TryGetValue(name, out var replacement)) reference.Value = Prefix + replacement.Replace("~", "~0").Replace("/", "~1");
            }
        });
        return copy;
    }

    private static IEnumerable<string> References(JToken schema)
    {
        var result = new List<string>();
        Visit(schema, reference =>
        {
            var value = (string?)reference.Value;
            if (value != null && value.StartsWith(Prefix, StringComparison.Ordinal))
                result.Add(value.Substring(Prefix.Length).Replace("~1", "/").Replace("~0", "~"));
        });
        return result;
    }

    // Visit schema locations only; example payloads may legitimately contain a literal $ref property.
    private static void Visit(JToken? token, Action<JProperty> reference)
    {
        if (token is not JObject schema) return;
        if (schema.Property("$ref") is { } property) reference(property);
        foreach (var key in new[] { "properties", "$defs", "patternProperties" })
            if (schema[key] is JObject children)
                foreach (var child in children.Properties()) Visit(child.Value, reference);
        foreach (var key in new[] { "items", "additionalProperties", "propertyNames", "not", "if", "then", "else" }) Visit(schema[key], reference);
        foreach (var key in new[] { "allOf", "anyOf", "oneOf", "prefixItems" })
            if (schema[key] is JArray children)
                foreach (var child in children) Visit(child, reference);
    }
}
