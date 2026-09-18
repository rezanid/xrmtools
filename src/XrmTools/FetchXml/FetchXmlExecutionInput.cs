#nullable enable
namespace XrmTools.FetchXml;

using Microsoft.Language.Xml;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.FetchXml.CodeGen;

/// <summary>Parses one captured editor snapshot, independent of background completion parsing.</summary>
internal sealed record FetchXmlExecutionInput(string Xml, string EntityName)
{
    internal static async Task<FetchXmlExecutionInput> ParseAsync(string text, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if (string.IsNullOrWhiteSpace(text)) throw new FormatException("Enter a FetchXML query before executing.");
        var query = await new FetchXmlParser().ParseAsync(Parser.ParseText(text), text, token).ConfigureAwait(false);
        var xml = string.IsNullOrEmpty(query.Defaulted) ? text : query.Defaulted;
        var entity = Parser.ParseText(xml).Root.Elements.FirstOrDefault(e => e.Name == "entity");
        var name = entity?.Attributes.FirstOrDefault(a => a.Key == "name").Value;
        if (string.IsNullOrWhiteSpace(name)) throw new FormatException("The query must specify an entity name.");
        return new FetchXmlExecutionInput(xml, name);
    }

    internal static int? FindActionPosition(string text)
    {
        var root = Parser.ParseText(text).Root;
        if (root?.Name != "fetch") return null;
        // Syntax positions include leading whitespace, which may belong to the preceding line.
        int position = root.Start;
        while (position < text.Length && char.IsWhiteSpace(text[position])) position++;
        return position;
    }
}
