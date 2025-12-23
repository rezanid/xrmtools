namespace XrmTools.FetchXml;

using Microsoft.Language.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XrmTools.FetchXml.Model;

/// <summary>
/// Helper class to replace parameters in FetchXML with provided values using XmlDocumentSyntax
/// </summary>
internal static class FetchXmlParameterReplacer
{
    /// <summary>
    /// Replaces parameters in FetchXML with their values and optionally updates defaults
    /// </summary>
    /// <param name="document">The XmlDocumentSyntax to process</param>
    /// <param name="parameterValues">Dictionary of parameter names to values</param>
    /// <param name="updateDefaults">If true, updates the FetchXML to include default values</param>
    /// <returns>Updated XmlDocumentSyntax</returns>
    public static XmlDocumentSyntax ReplaceParameters(XmlDocumentSyntax document, Dictionary<string, string> parameterValues, bool updateDefaults = false)
    {
        if (document == null || parameterValues == null || parameterValues.Count == 0)
        {
            return document;
        }

        // Convert to string, do replacement, then parse back
        var xmlString = document.ToFullString();
        var updatedString = ReplaceParametersInString(xmlString, document.Root, parameterValues, updateDefaults);
        
        if (updatedString == xmlString)
        {
            return document;
        }

        return Parser.ParseText(updatedString);
    }

    /// <summary>
    /// Updates FetchXML to include default values for parameters
    /// </summary>
    public static XmlDocumentSyntax UpdateDefaultValues(XmlDocumentSyntax document, List<FetchParameter> parameters, Dictionary<string, string> parameterValues)
    {
        if (document == null || parameters == null || parameters.Count == 0 || parameterValues == null)
        {
            return document;
        }

        return ReplaceParameters(document, parameterValues, updateDefaults: true);
    }

    private static string ReplaceParametersInString(string xmlString, IXmlElement root, Dictionary<string, string> parameterValues, bool updateDefaults)
    {
        var result = xmlString;
        
        // First pass: Replace <param> elements
        result = ReplaceParamElements(result, root, parameterValues, updateDefaults);
        
        // Second pass: Replace value parameters {{paramName}} in the result
        result = ReplaceValueParameters(result, parameterValues, updateDefaults);
        
        return result;
    }

    private static string ReplaceParamElements(string xmlString, IXmlElement root, Dictionary<string, string> parameterValues, bool updateDefaults)
    {
        var result = xmlString;
        var paramElements = FindParamElements(root);
        
        // Process in reverse order to maintain string positions
        foreach (var (element, paramName) in paramElements.OrderByDescending(p => GetElementPosition(xmlString, p.element)))
        {
            if (!parameterValues.TryGetValue(paramName, out var paramValue))
            {
                continue;
            }

            var elementText = element.AsSyntaxElement.ToFullString();
            var elementPos = GetElementPosition(xmlString, element);
            
            if (elementPos < 0)
            {
                continue;
            }

            string replacement;
            if (updateDefaults)
            {
                // Keep as <param> but update content
                replacement = $"<param name='{EscapeXmlAttribute(paramName)}'>{EscapeXmlContent(paramValue)}</param>";
            }
            else
            {
                // Replace with value only
                replacement = EscapeXmlContent(paramValue);
            }

            result = result.Substring(0, elementPos) + replacement + result.Substring(elementPos + elementText.Length);
        }
        
        return result;
    }

    private static List<(IXmlElement element, string paramName)> FindParamElements(IXmlElement root)
    {
        var result = new List<(IXmlElement, string)>();
        FindParamElementsRecursive(root, result);
        return result;
    }

    private static void FindParamElementsRecursive(IXmlElement element, List<(IXmlElement, string)> result)
    {
        if (element == null)
        {
            return;
        }

        // Check if this is a param element
        if (string.Equals(element.Name, "param", StringComparison.OrdinalIgnoreCase))
        {
            var nameAttr = element.Attributes.FirstOrDefault(a => 
                string.Equals(a.Key, "name", StringComparison.OrdinalIgnoreCase));
            
            if (nameAttr != null && !string.IsNullOrEmpty(nameAttr.Value))
            {
                result.Add((element, nameAttr.Value));
            }
        }

        // Recurse into children
        foreach (var child in element.Elements)
        {
            FindParamElementsRecursive(child, result);
        }
    }

    private static int GetElementPosition(string xmlString, IXmlElement element)
    {
        var elementText = element.AsSyntaxElement.ToFullString();
        return xmlString.IndexOf(elementText, StringComparison.Ordinal);
    }

    private static string ReplaceValueParameters(string value, Dictionary<string, string> parameterValues, bool updateDefaults)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        var result = value;

        foreach (var kvp in parameterValues)
        {
            var paramName = kvp.Key;
            var paramValue = kvp.Value ?? string.Empty;

            // Replace {{paramName:defaultValue}}
            var searchIndex = 0;
            while (true)
            {
                var patternStart = $"{{{{{paramName}:";
                var index = result.IndexOf(patternStart, searchIndex, StringComparison.Ordinal);
                if (index < 0)
                {
                    break;
                }

                var endIndex = result.IndexOf("}", index + patternStart.Length, StringComparison.Ordinal);
                if (endIndex > index && endIndex + 1 < result.Length && result[endIndex + 1] == '}')
                {
                    var fullPattern = result.Substring(index, endIndex - index + 2);
                    var replacement = updateDefaults ? $"{{{{{paramName}:{paramValue}}}}}" : paramValue;
                    result = result.Substring(0, index) + replacement + result.Substring(index + fullPattern.Length);
                    searchIndex = index + replacement.Length;
                }
                else
                {
                    searchIndex = index + patternStart.Length;
                }
            }

            // Replace {{paramName}}
            var patternWithoutDefault = $"{{{{{paramName}}}}}";
            if (result.Contains(patternWithoutDefault))
            {
                var replacement = updateDefaults ? $"{{{{{paramName}:{paramValue}}}}}" : paramValue;
                result = result.Replace(patternWithoutDefault, replacement);
            }
        }

        return result;
    }

    private static string EscapeXmlContent(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value ?? string.Empty;
        }

        return value
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;");
    }

    private static string EscapeXmlAttribute(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value ?? string.Empty;
        }

        return value
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&apos;");
    }
}
