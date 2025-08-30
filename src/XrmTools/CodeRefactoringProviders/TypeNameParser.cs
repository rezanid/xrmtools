namespace XrmTools.CodeRefactoringProviders;
using System.Collections.Generic;

public record ParsedType(string Namespace, string Name, List<ParsedType> GenericArguments);

public static class TypeNameParser
{
    public static ParsedType Parse(string input)
    {
        // Strip assembly if present
        var typePart = input.Split(',')[0].Trim();
        return ParseType(typePart);
    }

    private static ParsedType ParseType(string type)
    {
        int genericStart = type.IndexOf('[');
        if (genericStart == -1)
        {
            // No generics
            var lastDot = type.LastIndexOf('.');
            return new ParsedType(
                Namespace: type[..lastDot],
                Name: type[(lastDot + 1)..],
                GenericArguments: []);
        }

        // Generic type
        var baseType = type[..genericStart];
        var lastDotInBase = baseType.LastIndexOf('.');
        var baseNamespace = baseType[..lastDotInBase];
        var baseName = baseType[(lastDotInBase + 1)..];

        // remove [ ]
        var inner = type.Substring(genericStart + 1, type.Length - genericStart - 2);
        var args = ParseGenericArguments(inner);

        return new ParsedType(baseNamespace, baseName, args);
    }

    private static List<ParsedType> ParseGenericArguments(string input)
    {
        var results = new List<ParsedType>();
        int depth = 0;
        var current = "";

        for (int i = 0; i < input.Length; i++)
        {
            var ch = input[i];
            if (ch == '[') depth++;
            if (ch == ']') depth--;
            if (ch == ',' && depth == 0)
            {
                results.Add(Parse(current.Trim('[', ']')));
                current = "";
            }
            else
            {
                current += ch;
            }
        }

        if (!string.IsNullOrWhiteSpace(current))
            results.Add(Parse(current.Trim('[', ']')));

        return results;
    }
}
