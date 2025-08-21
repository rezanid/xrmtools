#nullable enable
namespace XrmTools.Xrm.Generators;

using Nito.Disposables.Internals;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using XrmTools.WebApi.Entities;
using XrmTools.WebApi.Types;

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

    public static AttributeMetadata? GetAttributeByName(IEnumerable<AttributeMetadata>? attributes, string name) => attributes?.FirstOrDefault(a => a.LogicalName == name);

    public static string? Tokenize(string? input)
    {
        if (input == null) return null;

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
        if (attribute == null) return null;
        var label = attribute.DisplayName?.LocalizedLabels?.FirstOrDefault()?.Label;
        if (!string.IsNullOrEmpty(label)) return Tokenize(label);
        if (string.IsNullOrEmpty(attribute.AttributeOf) || attributes == null)
        {
            return attribute.SchemaName;
        }
        var originalAttribute = attributes.FirstOrDefault(a => a.LogicalName == attribute.AttributeOf);
        if (originalAttribute == null) { return attribute.SchemaName; }
        if ((attribute.AttributeType is AttributeTypeCode.String or AttributeTypeCode.Virtual) && attribute.LogicalName.EndsWith("name"))
        {
            label = originalAttribute.DisplayName?.LocalizedLabels?.FirstOrDefault()?.Label;
            return string.Concat(Tokenize(label), "Name");
        }
        if (attribute.AttributeType == AttributeTypeCode.EntityName && attribute.LogicalName.EndsWith("idtype"))
        {
            label = originalAttribute.DisplayName?.LocalizedLabels?.FirstOrDefault()?.Label;
            return string.Concat(Tokenize(label), "IdType");
        }
        if (attribute.AttributeType == AttributeTypeCode.BigInt && attribute.LogicalName.EndsWith("timestamp"))
        {
            // e.g. logicalname: "crb4d_image_timestamp" attributeof: "crb4d_imageid"
            label = originalAttribute.DisplayName?.LocalizedLabels?.FirstOrDefault()?.Label;
            if (label is null) { return attribute.SchemaName; }
            return string.Concat(Tokenize(label), "Timestamp");
        }
        if (attribute.AttributeType == AttributeTypeCode.String && attribute.LogicalName.EndsWith("url"))
        {
            // e.g. logicalname: "crb4d_image_url" attributeof: "crb4d_imageid"
            label = originalAttribute.DisplayName?.LocalizedLabels?.FirstOrDefault()?.Label;
            if (label is null) { return attribute.SchemaName; }
            return string.Concat(Tokenize(label), "Timestamp");
        }
        return attribute.SchemaName;
    }

    public static string? RemovePrefixes(string text, string[] prefixes)
    {
        if (string.IsNullOrEmpty(text)) return null;
        foreach (var prefix in prefixes.WhereNotNull())
        {
            if (text.StartsWith(prefix, StringComparison.Ordinal))
            {
                return text[prefix!.Length..];
            }
        }
        return text;
    }

    public static string ToString(object? value) => value?.ToString() ?? string.Empty;

    //TODO: Maybe it's faster to rely on IsEnmuAttribute instead of casting?
    public static IEnumerable<EnumAttributeMetadata> FilterEnumAttributes(this IEnumerable<AttributeMetadata>? attributes)
        => attributes?.OfType<EnumAttributeMetadata>().Where(
            a => a.IsValidForRead && a.OptionSet is not null && a.OptionSet.OptionSetType != OptionSetType.Boolean) ?? [];
    public static IEnumerable<EnumAttributeMetadata> FilterGlobalEnumAttributes(this IEnumerable<AttributeMetadata>? attributes)
        => attributes.FilterEnumAttributes().Where(a => a.OptionSet?.IsGlobal == true) ?? [];
    public static IEnumerable<EnumAttributeMetadata> FilterLocalEnumAttributes(this IEnumerable<AttributeMetadata>? attributes)
        => attributes.FilterEnumAttributes().Where(a => a.OptionSet?.IsGlobal != true) ?? [];

    public static bool IsEnumAttribute(AttributeMetadata a) => (a.AttributeType is AttributeTypeCode.Picklist or AttributeTypeCode.Virtual or AttributeTypeCode.State or AttributeTypeCode.Status or AttributeTypeCode.EntityName) && a.IsLogical == false;

    public static string? GetLabel(Label? label, int language)
       => label?.LocalizedLabels?.FirstOrDefault(l => l.LanguageCode == language)?.Label;
}
#nullable restore