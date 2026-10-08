#nullable enable
namespace XrmTools.CodeGen.CustomApi;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using XrmTools.Options;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

public enum OpenApiOutputFormat
{
    [Description("OpenAPI v3 JSON")] Json,
    [Description("OpenAPI v3 YAML")] Yaml,
}

internal interface IOpenApiOutputSettings
{
    Task<OpenApiOutputFormat> GetFormatAsync();
}

[Export(typeof(IOpenApiOutputSettings))]
internal sealed class OpenApiOutputSettings : IOpenApiOutputSettings
{
    public async Task<OpenApiOutputFormat> GetFormatAsync()
        => (await GeneralOptions.GetLiveInstanceAsync().ConfigureAwait(false)).OpenApiOutputFormat;
}

/// <summary>Serializes the same document model for both output formats.</summary>
internal static class OpenApiDocumentFormatter
{
    internal static GeneratedClient Create(string apiName, JObject document, OpenApiOutputFormat format)
    {
        switch (format)
        {
            case OpenApiOutputFormat.Json:
                return new GeneratedClient(apiName + ".openapi.json", document.ToString(Formatting.Indented));
            case OpenApiOutputFormat.Yaml:
                using (var writer = new StringWriter(CultureInfo.InvariantCulture))
                {
                    new YamlStream(new YamlDocument(Node(document))).Save(writer, assignAnchors: false);
                    return new GeneratedClient(apiName + ".openapi.yaml", writer.ToString());
                }
            default: throw new ArgumentOutOfRangeException(nameof(format));
        }
    }

    private static YamlNode Node(JToken token)
    {
        if (token is JObject obj)
        {
            var mapping = new YamlMappingNode();
            foreach (var property in obj.Properties())
            {
                var key = new YamlScalarNode(property.Name) { Style = SafeKey(property.Name) ? ScalarStyle.Plain : ScalarStyle.DoubleQuoted };
                mapping.Add(key, Node(property.Value));
            }
            return mapping;
        }
        if (token is JArray array) return new YamlSequenceNode(array.Select(Node));
        // Quoting every string value preserves its JSON type across YAML readers,
        // including enum literals, timestamps, booleans, null and numeric-looking keys.
        return token.Type switch
        {
            JTokenType.String => new YamlScalarNode(token.Value<string>()) { Style = ScalarStyle.DoubleQuoted },
            JTokenType.Boolean => new YamlScalarNode(token.Value<bool>() ? "true" : "false") { Style = ScalarStyle.Plain },
            JTokenType.Null => new YamlScalarNode("null") { Style = ScalarStyle.Plain },
            JTokenType.Integer or JTokenType.Float => new YamlScalarNode(token.ToString(Formatting.None)) { Style = ScalarStyle.Plain },
            _ => throw new InvalidOperationException("Unsupported OpenAPI document value: " + token.Type),
        };
    }

    private static bool SafeKey(string value) => Regex.IsMatch(value, @"\A[A-Za-z_][A-Za-z0-9_.-]*\z") &&
        value.ToLowerInvariant() is not ("true" or "false" or "null" or "yes" or "no" or "on" or "off" or "y" or "n");
}
