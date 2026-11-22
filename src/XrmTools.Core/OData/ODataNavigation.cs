#nullable enable
namespace XrmTools.OData;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

internal static class ODataNavigation
{
    public static (int Line, int Column)? FindDefinition(string text, int line, int column)
    {
        var variables = new Dictionary<string, (int Line, int Column)>(StringComparer.Ordinal);
        var requests = new Dictionary<string, (int Line, int Column)>(StringComparer.Ordinal);
        string? expression = null;
        var document = ODataDocument.Parse(text, (referenceLine, start, value) =>
        {
            if (referenceLine != line) return;
            foreach (Match match in ODataDocument.Variable.Matches(value))
            {
                int from = start + match.Index;
                if (column >= from && column <= from + match.Length &&
                    (expression == null || column < from + match.Length))
                    expression = match.Groups[1].Value.Trim();
            }
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
}
