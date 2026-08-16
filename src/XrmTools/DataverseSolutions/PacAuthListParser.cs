#nullable enable
namespace XrmTools.DataverseSolutions;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

internal static class PacAuthListParser
{
    public static IReadOnlyList<PacAuthProfile> Parse(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return [];
        }

        var lines = text
            .Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.TrimEnd())
            .ToList();

        var headerIndex = lines.FindIndex(line => line.StartsWith("Index", StringComparison.OrdinalIgnoreCase));
        if (headerIndex < 0)
        {
            return [];
        }

        var header = lines[headerIndex];
        var activeStart = header.IndexOf("Active", StringComparison.Ordinal);
        var kindStart = header.IndexOf("Kind", StringComparison.Ordinal);
        var nameStart = header.IndexOf("Name", StringComparison.Ordinal);
        var userStart = header.IndexOf("User", StringComparison.Ordinal);
        var cloudStart = header.IndexOf("Cloud", StringComparison.Ordinal);
        var typeStart = header.IndexOf("Type", StringComparison.Ordinal);
        var environmentStart = header.IndexOf("Environment", typeStart + 1, StringComparison.Ordinal);
        var environmentUrlStart = header.IndexOf("Environment Url", StringComparison.Ordinal);

        var result = new List<PacAuthProfile>();
        foreach (var line in lines.Skip(headerIndex + 1))
        {
            if (!line.StartsWith("[", StringComparison.Ordinal))
            {
                continue;
            }

            var indexText = ReadColumn(line, 0, activeStart).Trim('[', ']');
            if (!int.TryParse(indexText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var index))
            {
                continue;
            }

            result.Add(new PacAuthProfile
            {
                Index = index,
                IsActive = ReadColumn(line, activeStart, kindStart).Contains('*'),
                Kind = NullIfWhiteSpace(ReadColumn(line, kindStart, nameStart)),
                Name = NullIfWhiteSpace(ReadColumn(line, nameStart, userStart)),
                User = NullIfWhiteSpace(ReadColumn(line, userStart, cloudStart)),
                Cloud = NullIfWhiteSpace(ReadColumn(line, cloudStart, typeStart)),
                Type = NullIfWhiteSpace(ReadColumn(line, typeStart, environmentStart)),
                EnvironmentName = NullIfWhiteSpace(ReadColumn(line, environmentStart, environmentUrlStart)),
                EnvironmentUrl = NullIfWhiteSpace(ReadColumn(line, environmentUrlStart, line.Length))
            });
        }

        return result;
    }

    private static string ReadColumn(string line, int start, int endExclusive)
    {
        if (start < 0 || start >= line.Length)
        {
            return string.Empty;
        }

        var safeEnd = Math.Min(Math.Max(endExclusive, start), line.Length);
        return line[start..safeEnd].Trim();
    }

    private static string? NullIfWhiteSpace(string value) => string.IsNullOrWhiteSpace(value) ? null : value;
}
#nullable restore
