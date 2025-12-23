namespace XrmTools.FetchXml;

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using XrmTools.FetchXml.Model;

/// <summary>
/// Helper class to replace parameters in FetchXML with provided values
/// </summary>
internal static class FetchXmlParameterReplacer
{
    private static readonly Regex ValueParameterRegex = new(@"\{\{(\w+)(?::([^}]*))?\}\}", RegexOptions.Compiled);
    private static readonly Regex ParamElementRegex = new(@"<param\s+name\s*=\s*['""](\w+)['""](?:\s*/)?>(?:.*?</param>|)", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

    /// <summary>
    /// Replaces parameters in FetchXML with their values and optionally updates defaults
    /// </summary>
    /// <param name="fetchXml">The FetchXML content</param>
    /// <param name="parameterValues">Dictionary of parameter names to values</param>
    /// <param name="updateDefaults">If true, updates the FetchXML to include default values</param>
    /// <returns>Updated FetchXML string</returns>
    public static string ReplaceParameters(string fetchXml, Dictionary<string, string> parameterValues, bool updateDefaults = false)
    {
        if (string.IsNullOrEmpty(fetchXml) || parameterValues == null || parameterValues.Count == 0)
        {
            return fetchXml;
        }

        var result = fetchXml;

        // Replace value-based parameters {{paramName}} or {{paramName:defaultValue}}
        foreach (var kvp in parameterValues)
        {
            var paramName = kvp.Key;
            var paramValue = kvp.Value ?? string.Empty;

            // Replace {{paramName:defaultValue}} with value (or with {{paramName:newDefault}} if updating defaults)
            var patternWithDefault = $@"\{{\{{{paramName}:([^}}]*)\}}}}";
            if (updateDefaults)
            {
                result = Regex.Replace(result, patternWithDefault, $"{{{{{paramName}:{EscapeXmlValue(paramValue)}}}}}");
            }
            else
            {
                result = Regex.Replace(result, patternWithDefault, EscapeXmlValue(paramValue));
            }

            // Replace {{paramName}} with value (or with {{paramName:value}} if updating defaults)
            var patternWithoutDefault = $@"\{{\{{{paramName}\}}}}";
            if (updateDefaults)
            {
                result = Regex.Replace(result, patternWithoutDefault, $"{{{{{paramName}:{EscapeXmlValue(paramValue)}}}}}");
            }
            else
            {
                result = Regex.Replace(result, patternWithoutDefault, EscapeXmlValue(paramValue));
            }
        }

        // Replace element-based parameters <param name='paramName' /> or <param name='paramName'>...</param>
        foreach (var kvp in parameterValues)
        {
            var paramName = kvp.Key;
            var paramValue = kvp.Value ?? string.Empty;

            if (updateDefaults)
            {
                // Update to include default: <param name='paramName'>value</param>
                var pattern = $@"<param\s+name\s*=\s*['\""{paramName}['\""]\s*(?:/)?>\s*(?:.*?</param>)?";
                result = Regex.Replace(result, pattern, 
                    $"<param name='{paramName}'>{paramValue}</param>", 
                    RegexOptions.IgnoreCase | RegexOptions.Singleline);
            }
            else
            {
                // Replace with value directly
                var pattern = $@"<param\s+name\s*=\s*['\""{paramName}['\""]\s*(?:/)?>\s*(?:.*?</param>)?";
                result = Regex.Replace(result, pattern, paramValue, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            }
        }

        return result;
    }

    /// <summary>
    /// Updates FetchXML to include default values for parameters
    /// </summary>
    public static string UpdateDefaultValues(string fetchXml, List<FetchParameter> parameters, Dictionary<string, string> parameterValues)
    {
        if (string.IsNullOrEmpty(fetchXml) || parameters == null || parameters.Count == 0 || parameterValues == null)
        {
            return fetchXml;
        }

        return ReplaceParameters(fetchXml, parameterValues, updateDefaults: true);
    }

    private static string EscapeXmlValue(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        return value
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&apos;");
    }
}
