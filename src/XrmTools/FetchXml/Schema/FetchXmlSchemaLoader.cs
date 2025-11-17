#nullable enable
namespace XrmTools.FetchXml.Schema;

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Xml;
using System.Xml.Schema;
using XrmTools.Logging;

internal sealed class FetchXmlSchemaLoader
{
    private const string ResourceNameSuffix = ".Schemas.Fetch.xsd";
    private static XmlSchemaSet? _schemaSet;
    private static int _loaded; // 0 = not loaded, 1 = loaded
    private static readonly ConcurrentDictionary<string, bool> _elements = new(StringComparer.OrdinalIgnoreCase);
    private static readonly ConcurrentDictionary<string, bool> _attributes = new(StringComparer.OrdinalIgnoreCase);

    public static XmlSchemaSet? SchemaSet { get { EnsureLoaded(null); return _schemaSet; } }

    public static void EnsureLoaded(IOutputLoggerService? logger) => EnsureLoaded(logger, false);

    private static XmlSchemaSet? EnsureLoaded(IOutputLoggerService? logger, bool force)
    {
        if (!force && Interlocked.CompareExchange(ref _loaded, 1, 0) != 0)
            return _schemaSet; // already loaded

        try
        {
            var asm = typeof(FetchXmlSchemaLoader).Assembly;
            string? resourceName = null;
            foreach (var r in asm.GetManifestResourceNames())
            {
                if (r.EndsWith(ResourceNameSuffix, StringComparison.OrdinalIgnoreCase)) { resourceName = r; break; }
            }
            if (resourceName == null)
            {
                logger?.LogError($"FetchXml schema resource not found ending with {ResourceNameSuffix}.");
                return null;
            }

            using var stream = asm.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                logger?.LogError("Failed to open embedded FetchXml schema stream.");
                return null;
            }
            using var reader = XmlReader.Create(stream, new XmlReaderSettings { IgnoreComments = true, IgnoreWhitespace = true });
            var schema = XmlSchema.Read(reader, (s, e) => logger?.LogError($"FetchXml XSD load error: {e.Message}"));
            var set = new XmlSchemaSet { XmlResolver = null };
            set.Add(schema);
            set.Compile();
            _schemaSet = set;

            foreach (XmlSchemaElement el in schema.Elements.Values)
            {
                if (!string.IsNullOrEmpty(el.Name)) _elements.TryAdd(el.Name, true);
            }
            foreach (XmlSchemaObject o in schema.Items)
            {
                if (o is XmlSchemaComplexType ctype && ctype.Attributes != null)
                {
                    foreach (var a in ctype.Attributes)
                    {
                        if (a is XmlSchemaAttribute attr && !string.IsNullOrEmpty(attr.Name)) _attributes.TryAdd(attr.Name, true);
                    }
                }
            }
            logger?.LogInformation("FetchXml schema loaded.");
        }
        catch (Exception ex)
        {
            logger?.LogError("Unexpected error loading FetchXml schema. " + ex.Message);
        }
        return _schemaSet;
    }

    public static bool IsElement(string name) => _elements.ContainsKey(name);
    public static bool IsAttribute(string name) => _attributes.ContainsKey(name);
}
#nullable restore
