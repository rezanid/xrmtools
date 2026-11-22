#nullable enable
namespace XrmTools.OData;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

internal enum ODataCompletionKind
{
    DocumentVariable,
    BuiltInVariable,
    NamedRequest,
    ResponseMember
}

internal readonly record struct ODataCompletionItem(string Text, string Description, ODataCompletionKind Kind);

internal static class ODataCompletion
{
    private static readonly Regex Prefix = new(@"\{\{[ \t]*((?:[A-Za-z_][A-Za-z0-9_]*\.){0,3})(\$?[A-Za-z0-9_]*)\z", RegexOptions.Compiled);
    private static readonly Regex CapturedPrefix = new(@"\{\{[ \t]*([A-Za-z_][A-Za-z0-9_]*)\.response\.(?:headers(\.[A-Za-z0-9_-]*)|body\.(\$(?:\.[A-Za-z_][A-Za-z0-9_]*|\[(?:[0-9]+|\*|'(?:[^'\\{}]|\\.)*'|""(?:[^""\\{}]|\\.)*"")\])*)(\.[A-Za-z0-9_]*|\[[0-9*]*))\z", RegexOptions.Compiled);

    public static bool TryGetCapturedSpan(string line, int column, out int start, out int length, out string name, out string path)
    {
        start = length = 0;
        name = path = "";
        if (ODataSyntax.IsComment(line.TrimStart())) return false;
        var match = CapturedPrefix.Match(line.Substring(0, column));
        if (!match.Success) return false;
        name = match.Groups[1].Value;
        var segment = match.Groups[2].Success ? match.Groups[2] : match.Groups[4];
        path = match.Groups[3].Value;
        start = segment.Index;
        int end = column;
        while (end < line.Length && (char.IsLetterOrDigit(line[end]) || line[end] is '_' or '-' or '*')) end++;
        if (line[start] == '[' && end < line.Length && line[end] == ']') end++;
        length = end - start;
        return true;
    }

    public static bool TryGetSpan(string line, int column, out int start, out int length, out string qualifier)
    {
        start = length = 0;
        qualifier = "";
        if (ODataSyntax.IsComment(line.TrimStart())) return false;
        var match = Prefix.Match(line.Substring(0, column));
        if (!match.Success) return false;
        qualifier = match.Groups[1].Value;
        if (qualifier.Count(c => c == '.') == 3 && !qualifier.EndsWith(".response.body.", StringComparison.Ordinal))
            return false;
        start = match.Groups[2].Index;
        int end = column;
        if (end == start && end < line.Length && line[end] == '$') end++;
        while (end < line.Length && (char.IsLetterOrDigit(line[end]) || line[end] == '_')) end++;
        length = end - start;
        return true;
    }

    public static IEnumerable<ODataCompletionItem> GetItems(ODataDocument document, string qualifier)
    {
        var names = document.Requests.Where(r => r.RequestName.Length != 0)
            .GroupBy(r => r.RequestName, StringComparer.Ordinal)
            .Where(group => group.Count() == 1)
            .Select(group => group.Key)
            .ToList();

        if (qualifier.Length == 0)
        {
            foreach (var name in document.Variables.Keys)
                yield return new(name, "Document variable.", ODataCompletionKind.DocumentVariable);
            foreach (var entry in ODataBuiltInVariables.All)
                yield return new(entry.Key, entry.Value.Description, ODataCompletionKind.BuiltInVariable);
            foreach (var name in names.Where(name => !document.Variables.ContainsKey(name)))
                yield return new(name, "Named request. Continue with .response to reference its captured response.", ODataCompletionKind.NamedRequest);
            yield break;
        }

        int dot = qualifier.IndexOf('.');
        if (!names.Contains(qualifier.Substring(0, dot))) yield break;
        switch (qualifier.Substring(dot))
        {
            case ".":
                yield return new("response", "The named request's captured response.", ODataCompletionKind.ResponseMember);
                break;
            case ".response.":
                yield return new("body", "The response JSON body. Continue with .$ to select values.", ODataCompletionKind.ResponseMember);
                yield return new("headers", "The response headers. Continue with a dot and a header name.", ODataCompletionKind.ResponseMember);
                break;
            case ".response.body.":
                yield return new("$", "The root of the response JSON body.", ODataCompletionKind.ResponseMember);
                break;
        }
    }
}
