#nullable enable
namespace XrmGen.Xrm.Generators;

using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

public static class ScribanExtensions
{
    /// <summary>
    /// Removes all diacritics and spaces from a string.
    /// </summary>
    public static string RemoveDiacriticsAndSpace(this string text)
    {
        if (text == null) return string.Empty;

        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (var ch in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark && unicodeCategory != UnicodeCategory.SpaceSeparator)
            {
                stringBuilder.Append(ch);
            }
        }

        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }

    public static string Tokenize(string? input)
    {
        if (input == null) return string.Empty;

        var normalizedString = input.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalizedString.Length);
        UnicodeCategory? previousCategory = null;

        foreach (var c in normalizedString)
        {
            var uc = CharUnicodeInfo.GetUnicodeCategory(c);
            if (uc != UnicodeCategory.NonSpacingMark)
            {
                if (char.IsLetterOrDigit(c))
                {
                    sb.Append(previousCategory is UnicodeCategory.UppercaseLetter or UnicodeCategory.LowercaseLetter or UnicodeCategory.DecimalDigitNumber ? char.ToLowerInvariant(c) : char.ToUpperInvariant(c));
                }
                previousCategory = uc;
            }
        }
        if (char.IsDigit(sb[0]))
        {
            sb.Insert(0, '_');
        }
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    public static string LastSegment(string input)
    {
        if (input == null) return string.Empty;
        var index = input.LastIndexOf('.');
        return index == -1 ? input : input[(index + 1)..];
    }

    public static string? LocalizedDisplayName(AttributeMetadata attribute, AttributeMetadata[]? attributes)
    {
        var label = attribute.DisplayName?.LocalizedLabels?.FirstOrDefault()?.Label;
        if (!string.IsNullOrEmpty(label)) return label;
        if (string.IsNullOrEmpty(label)
            && !string.IsNullOrEmpty(attribute.AttributeOf)
            && attributes != null)
        {
            return string.Concat(attributes!.FirstOrDefault(a => a.LogicalName == attribute.AttributeOf)?.DisplayName?.LocalizedLabels?[0].Label, "_", attribute.LogicalName);
        }
        return null;
    }

    public static string? SafeDisplayName(AttributeMetadata attribute, AttributeMetadata[]? attributes)
    {
        var label = attribute.DisplayName?.LocalizedLabels?.FirstOrDefault()?.Label;
        if (!string.IsNullOrEmpty(label)) return Tokenize(label);
        if (string.IsNullOrEmpty(label)
            && !string.IsNullOrEmpty(attribute.AttributeOf)
            && attributes != null)
        {
            return string.Concat(Tokenize(attributes!.FirstOrDefault(a => a.LogicalName == attribute.AttributeOf)?.DisplayName?.LocalizedLabels?[0].Label), "_", attribute.LogicalName);
        }
        return null;
    }
}
#nullable restore