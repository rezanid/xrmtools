namespace XrmTools.FetchXml;

using Microsoft.Language.Xml;
using System;
using System.Collections.Generic;
using System.Text;
using XrmTools.FetchXml.Model;

/// <summary>
/// Helper class to replace parameters in FetchXML with provided values using XmlDocumentSyntax
/// </summary>
internal static class FetchXmlParameterParser
{
    public static (string defaultDocument, string tokenziedDocument, List<FetchParameter> parameters)
        ParseParameters(XmlDocumentSyntax document, string rawDocument)
    {
        if (document == null)
        {
            throw new ArgumentNullException(nameof(document));
        }

        rawDocument ??= document.ToFullString();

        if (TryFindParams(document.Root as XmlElementSyntax, out var paramNodes, out var parameters))
        {
            var tokenizedDocument = new StringBuilder(rawDocument);
            var defaultDocument = new StringBuilder(rawDocument);

            for (var i = paramNodes.Count - 1; i >= 0; i--)
            {
                var node = paramNodes[i];
                var param = parameters[i];
                if (node is IXmlElement element)
                {
                    defaultDocument.Remove(element.Start, element.FullWidth);
                    tokenizedDocument.Remove(element.Start, element.FullWidth);

                    defaultDocument.Insert(element.Start, param.DefaultValue);
                    tokenizedDocument.Insert(element.Start, "{" + param.Name + "}");
                }
                else if (node is XmlAttributeSyntax attribute)
                {
                    defaultDocument.Remove(attribute.ValueNode.Start + 1, attribute.ValueNode.Width - 2);
                    tokenizedDocument.Remove(attribute.ValueNode.Start + 1, attribute.ValueNode.Width - 2);

                    defaultDocument.Insert(attribute.ValueNode.Start + 1, param.DefaultValue);
                    tokenizedDocument.Insert(attribute.ValueNode.Start + 1, "{" + param.Name + "}");
                }
            }

            return (defaultDocument.ToString(), tokenizedDocument.ToString(), parameters);
        }
        return (string.Empty, string.Empty, []);
    }

    private static bool TryFindParams(
        XmlElementSyntax element, 
        out List<XmlNodeSyntax> parameterNodes,
        out List<FetchParameter> parameters)
    {
        parameterNodes = [];
        parameters = [];
        if (element == null)
        {
            return false;
        }

        int counter = 0;
        FindParamsRecursive(element, parameterNodes, parameters, ref counter);

        return parameterNodes.Count > 0;
    }

    private static void FindParamsRecursive(
        IXmlElementSyntax element,
        List<XmlNodeSyntax> parameterNodes,
        List<FetchParameter> parameters,
        ref int counter)
    {
        if (element == null)
        {
            return;
        }
        // Check if this is a param element
        if (string.Equals(element.Name.ToString(), "param", StringComparison.OrdinalIgnoreCase))
        {
            counter++;
            parameterNodes.Add(element as XmlNodeSyntax);
            var paramName = element.GetAttributeValue("name");
            if (string.IsNullOrEmpty(paramName)) paramName = "p" + counter;
            parameters.Add(new FetchParameter
            {
                Name = paramName,
                IsElementParameter = true,
                DefaultValue = element is XmlElementSyntax elementWithClosing ? elementWithClosing.Content.ToFullString() : string.Empty
            });
            return;
        }
        // Check attributes for parameters
        foreach (var attr in element.Attributes)
        {
            if (TryParseParamAttribute(attr) is FetchParameter parameter)
            {
                counter++;
                if (string.IsNullOrEmpty(parameter.Name)) parameter.Name = "p" + counter;
                parameterNodes.Add(attr);
                parameters.Add(parameter);
            }
        }
        // Recurse into children
        foreach (var child in element.Elements)
        {
            FindParamsRecursive(child, parameterNodes, parameters, ref counter);
        }
    }

    public static FetchParameter TryParseParamAttribute(XmlAttributeSyntax attribute)
    {
        var attrValue = attribute.Value;
        if (string.IsNullOrEmpty(attrValue)) return null;
        var startIndex = attrValue.IndexOf("{{", StringComparison.Ordinal);
        var endIndex = attrValue.IndexOf("}}", StringComparison.Ordinal);
        if (startIndex >= 0 && endIndex > startIndex)
        {
            var paramContent = attrValue.Substring(startIndex + 2, endIndex - startIndex - 2);
            var colonIndex = paramContent.IndexOf(':');
            if (colonIndex >= 0)
            {
                return new FetchParameter
                {
                    Name = paramContent[..colonIndex],
                    DefaultValue = paramContent[(colonIndex + 1)..]?.Trim(),
                    IsElementParameter = false
                };
            }
            else
            {
                return new FetchParameter
                {
                    Name = paramContent,
                    DefaultValue = string.Empty,
                    IsElementParameter = false
                };
            }
        }
        return null;
    }
}
