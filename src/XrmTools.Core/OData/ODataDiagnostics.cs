#nullable enable
namespace XrmTools.OData;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

internal readonly record struct ODataDiagnostic(int Start, int Length, string Message, string? Replacement = null);

internal static class ODataDiagnostics
{
    public static IReadOnlyList<ODataDiagnostic> Analyze(string text,
        IReadOnlyDictionary<string, string>? environmentVariables = null, string? environmentError = null)
    {
        var lineStarts = new List<int> { 0 };
        foreach (Match newline in Regex.Matches(text, "\r\n|\r|\n"))
            lineStarts.Add(newline.Index + newline.Length);

        var diagnostics = new List<ODataDiagnostic>();
        var references = new List<(int Start, string Expression)>();
        var document = ODataDocument.Parse(text, (line, column, value) =>
        {
            foreach (Match match in ODataDocument.Variable.Matches(value))
            {
                var group = match.Groups[1];
                string expression = group.Value.Trim();
                if (expression.Length != 0)
                    references.Add((lineStarts[line] + column + group.Index + group.Value.Length - group.Value.TrimStart().Length, expression));
            }
        }, (line, column, name) => diagnostics.Add(new(lineStarts[line] + column, name.Length, $"Duplicate request name '{name}'. Choose a unique name.")));

        if (environmentVariables != null) document.SetEnvironmentVariables(environmentVariables);
        if (environmentError != null)
            diagnostics.Add(new(0, Math.Min(1, text.Length), $"Invalid odata-client.env.json: {environmentError}"));

        var names = new HashSet<string>(document.Requests.Select(request => request.RequestName).Where(name => name.Length != 0), StringComparer.Ordinal);
        var variables = document.EffectiveVariables.Keys.Concat(ODataBuiltInVariables.All.Keys).ToArray();
        foreach (var (start, expression) in references)
        {
            if (expression.Contains(".response"))
            {
                string name;
                try { name = ODataResponseReferences.ValidateSelector(expression); }
                catch (FormatException ex)
                {
                    diagnostics.Add(new(start, expression.Length, ex.Message));
                    continue;
                }
                if (!names.Contains(name))
                    diagnostics.Add(Unknown(start, name, "named request", names));
            }
            else if (!document.EffectiveVariables.ContainsKey(expression) && !ODataBuiltInVariables.All.ContainsKey(expression))
                diagnostics.Add(Unknown(start, expression, "variable", variables));
        }
        return diagnostics.OrderBy(diagnostic => diagnostic.Start).ToArray();
    }

    private static ODataDiagnostic Unknown(int start, string name, string kind, IEnumerable<string> candidates)
    {
        var matches = candidates.Where(candidate => IsClose(name, candidate)).Take(2).ToArray();
        return new(start, name.Length, $"Unknown {kind} '{name}'.", matches.Length == 1 ? matches[0] : null);
    }

    private static bool IsClose(string left, string right)
    {
        if (string.Equals(left, right, StringComparison.OrdinalIgnoreCase)) return true;
        if (Math.Abs(left.Length - right.Length) > 1) return false;
        if (left.Length > right.Length) (left, right) = (right, left);
        int i = 0;
        while (i < left.Length && left[i] == right[i]) i++;
        if (left.Length != right.Length)
            return string.CompareOrdinal(left, i, right, i + 1, left.Length - i) == 0;
        if (i == left.Length) return true;
        if (string.CompareOrdinal(left, i + 1, right, i + 1, left.Length - i - 1) == 0) return true;
        return i + 1 < left.Length && left[i] == right[i + 1] && left[i + 1] == right[i] &&
            string.CompareOrdinal(left, i + 2, right, i + 2, left.Length - i - 2) == 0;
    }
}
