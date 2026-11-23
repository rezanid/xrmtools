#nullable enable
namespace XrmTools.OData;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

internal readonly record struct ODataReference(int Column, int Length, string Expression);

internal static class ODataNavigation
{
    public static ODataReference? FindReference(string text, int line, int column)
    {
        ODataReference? reference = null;
        ODataDocument.Parse(text, (referenceLine, start, value) =>
        {
            if (referenceLine == line)
                reference = FindInValue(value, start, column, includeEnd: false) ?? reference;
        });
        return reference;
    }

    public static (int Line, int Column)? FindDefinition(string text, int line, int column)
    {
        var variables = new Dictionary<string, (int Line, int Column)>(StringComparer.Ordinal);
        var requests = new Dictionary<string, (int Line, int Column)>(StringComparer.Ordinal);
        string? expression = null;
        var document = ODataDocument.Parse(text, (referenceLine, start, value) =>
        {
            if (referenceLine == line)
                expression = FindInValue(value, start, column, includeEnd: true)?.Expression ?? expression;
        }, declaration: (declarationLine, declarationColumn, name, isRequest) =>
            (isRequest ? requests : variables)[name] = (declarationLine, declarationColumn));

        if (expression == null) return null;
        int response = expression.IndexOf(".response.", StringComparison.Ordinal);
        if (response < 0)
            return variables.TryGetValue(expression, out var variable) ? variable : null;

        string requestName = expression.Substring(0, response);
        return document.Requests.Count(request => request.RequestName == requestName) == 1 &&
            requests.TryGetValue(requestName, out var requestDefinition) ? requestDefinition : null;
    }

    private static ODataReference? FindInValue(string value, int start, int column, bool includeEnd)
    {
        ODataReference? reference = null;
        foreach (Match match in ODataDocument.Variable.Matches(value))
        {
            int from = start + match.Index;
            if (column >= from && (column < from + match.Length || (includeEnd && column == from + match.Length)) &&
                (reference == null || column < from + match.Length))
                reference = new(from, match.Length, match.Groups[1].Value.Trim());
        }
        return reference;
    }
}
