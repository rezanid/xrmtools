#nullable enable
namespace XrmTools.OData;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

internal readonly record struct ODataRenameOccurrence(int Line, int Column, string Name, bool IsRequest, bool IsDeclaration)
{
    public bool Contains(int line, int column) => Line == line && Column <= column && column <= Column + Name.Length;
}

internal sealed class ODataRename(
    ODataRenameOccurrence active, IReadOnlyList<ODataRenameOccurrence> occurrences, HashSet<string> declaredNames)
{
    private static readonly Regex Identifier = new(@"\A[A-Za-z_][A-Za-z0-9_]*\z", RegexOptions.Compiled);
    public ODataRenameOccurrence Active { get; } = active;
    public IReadOnlyList<ODataRenameOccurrence> Occurrences { get; } = occurrences;

    public string? Validate(string name)
    {
        if (!Identifier.IsMatch(name)) return "Use letters, digits, or underscores. Start with a letter or underscore.";
        if (name != Active.Name && declaredNames.Contains(name))
            return $"'{name}' is already used by another {(Active.IsRequest ? "named request" : "variable")}.";
        return null;
    }

    public static ODataRename? Find(string text, int line, int column)
    {
        var symbols = new List<ODataRenameOccurrence>();
        var document = ODataDocument.Parse(text, (referenceLine, start, value) =>
        {
            foreach (Match match in ODataDocument.Variable.Matches(value))
            {
                var group = match.Groups[1];
                string expression = group.Value.TrimStart();
                int response = expression.IndexOf(".response.", StringComparison.Ordinal);
                string name = response < 0 ? expression.TrimEnd() : expression.Substring(0, response);
                if (Identifier.IsMatch(name))
                    symbols.Add(new(referenceLine, start + group.Index + group.Length - expression.Length, name, response >= 0, false));
            }
        }, declaration: (declarationLine, start, name, isRequest) =>
            symbols.Add(new(declarationLine, start, name, isRequest, true)));

        int index = symbols.FindIndex(symbol => symbol.Contains(line, column));
        if (index < 0) return null;
        var active = symbols[index];
        var sameKind = symbols.Where(symbol => symbol.IsRequest == active.IsRequest).ToArray();
        var occurrences = sameKind.Where(symbol => symbol.Name == active.Name).ToArray();
        if (occurrences.Count(symbol => symbol.IsDeclaration) != 1 ||
            (active.IsRequest && document.Requests.Count(request => request.RequestName == active.Name) != 1))
            return null;
        var names = new HashSet<string>(sameKind.Where(symbol => symbol.IsDeclaration).Select(symbol => symbol.Name), StringComparer.Ordinal);
        return new(active, occurrences, names);
    }
}
