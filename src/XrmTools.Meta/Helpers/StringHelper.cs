#nullable enable
namespace XrmTools.Meta.Helpers;
using System;
using System.Collections.Generic;

internal static class StringHelper
{
    /// <summary>
    /// Satisfy NRT checks by ensuring a null string is never propagated
    /// </summary>
    /// <remarks>
    /// Various legacy APIs still return nullable strings (even if, in practice they
    /// never will actually be null) so we can use this extension to keep the NRT
    /// checks quiet</remarks>
    public static string EmptyWhenNull(this string? str) => str ?? string.Empty;

    public static List<string> SplitAndTrim(this string input, char separator, string ignore)
    {
        if (string.IsNullOrWhiteSpace(input)) return [];

        var result = new List<string>();
        int start = 0, length = input.Length;

        for (int i = 0; i <= length; i++)
        {
            if (i == length || input[i] == separator)
            {
                if (i > start)
                {
                    string word = input[start..i].Trim();
                    if (!string.IsNullOrEmpty(word) && !string.Equals(word, ignore, StringComparison.OrdinalIgnoreCase))
                    {
                        result.Add(word);
                    }
                }
                // Skip the separator
                start = i + 1;
            }
        }

        return result;
    }

    public static List<string> SplitAndTrim(this string input, char separator)
    {
        if (string.IsNullOrWhiteSpace(input)) return [];

        var result = new List<string>();
        int start = 0, length = input.Length;

        for (int i = 0; i <= length; i++)
        {
            if (i == length || input[i] == separator)
            {
                if (i > start)
                {
                    string word = input[start..i].Trim();
                    if (!string.IsNullOrEmpty(word))
                    {
                        result.Add(word);
                    }
                }
                // Skip the comma
                start = i + 1;
            }
        }

        return result;
    }
}
#nullable restore