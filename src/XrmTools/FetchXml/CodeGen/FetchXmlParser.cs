namespace XrmTools.FetchXml.CodeGen;

using Microsoft.Language.Xml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

internal class FetchXmlParser
{
    private static readonly Regex FxSettingIncludingCommentChars = new(@"^\s*<!--\s*fx\.(.+?)\s*:\s*(.+?)\s*--!?>\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex InlineParameterPattern = new(@"\{\{(\w+)(?::([^}]*))?\}\}", RegexOptions.Compiled);

    public async Task<Model.FetchQuery> ParseAsync(XmlDocumentSyntax doc, CancellationToken cancellationToken = default)
    {
        if (doc == null) throw new ArgumentNullException(nameof(doc));

        cancellationToken.ThrowIfCancellationRequested();

        var fetchElement = doc.Root;
        if (fetchElement == null || !StringEquals(fetchElement.Name, "fetch"))
            throw new FormatException("Root element must be <fetch>.");

        var query = new Model.FetchQuery();

        // Parse fx.* settings from the input text before the <fetch> element
        CollectFxSettings(doc, query.Settings);

        // Parse root attributes and collect inline parameters
        foreach (var attr in fetchElement.Attributes)
        {
            var name = attr.Key ?? string.Empty;
            var val = attr.Value ?? string.Empty;
            
            // Collect inline parameters from attribute values
            CollectInlineParameters(val, query.Parameters);
            
            switch (name.ToLowerInvariant())
            {
                case "version": query.Version = val; break;
                case "distinct": query.Distinct = ParseBool(val); break;
                case "no-lock": query.NoLock = ParseBool(val); break;
                case "returntotalrecordcount": query.ReturnTotalRecordCount = ParseBool(val); break;
                case "aggregate": query.Aggregate = ParseBool(val); break;
                case "count": query.Count = ParseInt(val); break;
                case "page": query.Page = ParseInt(val); break;
                case "top": query.Top = ParseInt(val); break;
                case "paging-cookie": query.PagingCookie = val; break;
                case "output-format": query.OutputFormat = val; break;
                case "mapping": query.Mapping = val; break;
                case "min-active-row-version": query.MinActiveRowVersion = val; break;
                default:
                    query.Extras[name] = val;
                    break;
            }
        }

        // Find entity
        var entityElement = fetchElement.Elements.FirstOrDefault(e => StringEquals(e.Name, "entity"))
            ?? throw new FormatException("<entity> element is required under <fetch>.");

        query.Entity = ParseEntity(entityElement, cancellationToken);
        
        // Collect <param> elements from entity and its children
        CollectParamElements(entityElement, query.Parameters);

        return await Task.FromResult(query).ConfigureAwait(false);
    }

    public Task<Model.FetchQuery> ParseAsync(string fetchXml, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fetchXml)) throw new ArgumentNullException(nameof(fetchXml));
        cancellationToken.ThrowIfCancellationRequested();

        var doc = Parser.ParseText(fetchXml);
        var root = doc?.Root;
        if (root == null || !StringEquals(root.Name, "fetch"))
            throw new FormatException("Root element must be <fetch>.");

        var query = new Model.FetchQuery();

        // Parse fx.* settings from the input text before the <fetch> element
        CollectFxSettings(fetchXml, query.Settings);

        // Parse root attributes and collect inline parameters
        foreach (var attr in root.Attributes)
        {
            var name = attr.Key ?? string.Empty;
            var val = attr.Value ?? string.Empty;
            
            // Collect inline parameters from attribute values
            CollectInlineParameters(val, query.Parameters);
            
            switch (name.ToLowerInvariant())
            {
                case "version": query.Version = val; break;
                case "distinct": query.Distinct = ParseBool(val); break;
                case "no-lock": query.NoLock = ParseBool(val); break;
                case "returntotalrecordcount": query.ReturnTotalRecordCount = ParseBool(val); break;
                case "aggregate": query.Aggregate = ParseBool(val); break;
                case "count": query.Count = ParseInt(val); break;
                case "page": query.Page = ParseInt(val); break;
                case "top": query.Top = ParseInt(val); break;
                case "paging-cookie": query.PagingCookie = val; break;
                case "output-format": query.OutputFormat = val; break;
                case "mapping": query.Mapping = val; break;
                case "min-active-row-version": query.MinActiveRowVersion = val; break;
                default:
                    query.Extras[name] = val;
                    break;
            }
        }

        // Find entity
        var entityElement = root.Elements.FirstOrDefault(e => StringEquals(e.Name, "entity"));
        if (entityElement == null)
            throw new FormatException("<entity> element is required under <fetch>.");

        query.Entity = ParseEntity(entityElement, cancellationToken);
        
        // Collect <param> elements from entity and its children
        CollectParamElements(entityElement, query.Parameters);
        
        return Task.FromResult(query);
    }

    private static Model.FetchEntity ParseEntity(IXmlElement entityElement, CancellationToken ct)
    {
        var entity = new Model.FetchEntity();
        ParseFetchNodeCommon(entityElement, entity);

        foreach (var attr in entityElement.Attributes)
        {
            var name = attr.Key ?? string.Empty;
            var val = attr.Value ?? string.Empty;
            switch (name.ToLowerInvariant())
            {
                case "enableprefiltering": entity.EnablePrefiltering = ParseBool(val); break;
                case "prefilterparametername": entity.PrefilterParameterName = val; break;
            }
        }

        // Detect <all-attributes />
        foreach (var child in entityElement.Elements)
        {
            if (StringEquals(child.Name, "all-attributes"))
            {
                entity.AllAttributes = true;
            }
        }

        return entity;
    }

    private static void ParseFetchNodeCommon(IXmlElement el, Model.FetchNode node)
    {
        foreach (var attr in el.Attributes)
        {
            var name = attr.Key ?? string.Empty;
            var val = attr.Value ?? string.Empty;
            switch (name.ToLowerInvariant())
            {
                case "name": node.Name = val; break;
                case "alias": node.Alias = val; break;
                default:
                    node.Extras[name] = val;
                    break;
            }
        }

        foreach (var child in el.Elements)
        {
            if (StringEquals(child.Name, "attribute"))
            {
                node.Attributes.Add(ParseAttribute(child));
            }
            else if (StringEquals(child.Name, "order"))
            {
                node.Orders.Add(ParseOrder(child));
            }
            else if (StringEquals(child.Name, "filter"))
            {
                node.Filters.Add(ParseFilter(child));
            }
            else if (StringEquals(child.Name, "link-entity"))
            {
                node.Links.Add(ParseLinkEntity(child));
            }
        }
    }

    private static Model.FetchLinkEntity ParseLinkEntity(IXmlElement linkEl)
    {
        var link = new Model.FetchLinkEntity();
        ParseFetchNodeCommon(linkEl, link);

        foreach (var attr in linkEl.Attributes)
        {
            var name = attr.Key ?? string.Empty;
            var val = attr.Value ?? string.Empty;
            switch (name.ToLowerInvariant())
            {
                case "from": link.From = val; break;
                case "to": link.To = val; break;
                case "link-type": link.LinkType = ParseJoinType(val); break;
                case "intersect": link.Intersect = ParseBool(val); break;
            }
        }
        return link;
    }

    private static Model.FetchAttribute ParseAttribute(IXmlElement attrEl)
    {
        var fa = new Model.FetchAttribute();
        foreach (var attr in attrEl.Attributes)
        {
            var name = attr.Key ?? string.Empty;
            var val = attr.Value ?? string.Empty;
            switch (name.ToLowerInvariant())
            {
                case "name": fa.Name = val; break;
                case "alias": fa.Alias = val; break;
                case "aggregate": fa.Aggregate = val; break;
                case "groupby": fa.GroupBy = ParseBool(val); break;
                case "dategrouping": fa.DateGrouping = val; break;
                case "distinct": fa.Distinct = ParseBool(val); break;
                case "usertimezone": fa.UserTimeZone = ParseInt(val); break;
                default:
                    fa.Extras[name] = val;
                    break;
            }
        }
        return fa;
    }

    private static Model.FetchOrder ParseOrder(IXmlElement orderEl)
    {
        var fo = new Model.FetchOrder();
        foreach (var attr in orderEl.Attributes)
        {
            var name = attr.Key ?? string.Empty;
            var val = attr.Value ?? string.Empty;
            switch (name.ToLowerInvariant())
            {
                case "attribute": fo.Attribute = val; break;
                case "descending": fo.Descending = ParseBool(val) ?? false; break;
                case "alias": fo.Alias = val; break;
                default:
                    fo.Extras[name] = val; break;
            }
        }
        return fo;
    }

    private static Model.FetchFilter ParseFilter(IXmlElement filterEl)
    {
        var filter = new Model.FetchFilter();
        foreach (var attr in filterEl.Attributes)
        {
            var name = attr.Key ?? string.Empty;
            var val = attr.Value ?? string.Empty;
            switch (name.ToLowerInvariant())
            {
                case "type": filter.Type = StringEquals(val, "or") ? Model.LogicalOperator.Or : Model.LogicalOperator.And; break;
                default: filter.Extras[name] = val; break;
            }
        }

        foreach (var child in filterEl.Elements)
        {
            if (StringEquals(child.Name, "condition"))
            {
                filter.Conditions.Add(ParseCondition(child));
            }
            else if (StringEquals(child.Name, "filter"))
            {
                filter.Filters.Add(ParseFilter(child));
            }
        }
        return filter;
    }

    private static Model.FetchCondition ParseCondition(IXmlElement condEl)
    {
        var cond = new Model.FetchCondition();
        foreach (var attr in condEl.Attributes)
        {
            var name = attr.Key ?? string.Empty;
            var val = attr.Value ?? string.Empty;
            switch (name.ToLowerInvariant())
            {
                case "attribute":
                    cond.Attribute = val;
                    // Support alias.attribute (split into EntityAlias + Attribute)
                    var dot = val?.IndexOf('.') ?? -1;
                    if (dot > 0)
                    {
                        cond.EntityAlias = val.Substring(0, dot);
                        cond.Attribute = val.Substring(dot + 1);
                    }
                    break;
                case "operator": cond.Operator = val; break;
                case "value": // rare inline value without <value> child
                    cond.Values.Add(new Model.FetchValue { Text = val, Typed = TryAutoType(val, null, null), TypeHint = null });
                    break;
                default:
                    cond.Extras[name] = val; break;
            }
        }

        // Detect null/not-null operator (no values expected)
        if (string.Equals(cond.Operator, "null", StringComparison.OrdinalIgnoreCase) || string.Equals(cond.Operator, "not-null", StringComparison.OrdinalIgnoreCase))
        {
            cond.ValueIsNull = true;
        }

        foreach (var child in condEl.Elements)
        {
            if (StringEquals(child.Name, "value"))
            {
                cond.Values.Add(ParseValue(child));
            }
        }

        return cond;
    }

    private static Model.FetchValue ParseValue(IXmlElement valueEl)
    {
        var fv = new Model.FetchValue();

        // Attributes on <value>
        foreach (var attr in valueEl.Attributes)
        {
            var name = attr.Key ?? string.Empty;
            var val = attr.Value ?? string.Empty;
            switch (name.ToLowerInvariant())
            {
                case "uitype": fv.Uitype = val; break;
                case "uiname": fv.Uiname = val; break;
                case "type": fv.TypeHint = val; break; // explicit type hint if present
                default: fv.Extras[name] = val; break;
            }
        }

        // Best-effort inner text using a known heuristic: the element's text is often stored in a special pseudo-attribute value under Microsoft.Language.Xml
        // If not present, leave Text empty.
        var text = TryGetElementInnerText(valueEl);
        fv.Text = text;
        fv.Typed = TryAutoType(text, fv.TypeHint, fv.Uitype);
        return fv;
    }

    private static string TryGetElementInnerText(IXmlElement el)
    {
        // Microsoft.Language.Xml does not expose a direct InnerText API, but element.Value often contains the text content.
        // Use reflection fallback to avoid hard dependency on specific versions.
        try
        {
            var prop = el.GetType().GetProperty("Value");
            if (prop != null && prop.PropertyType == typeof(string))
            {
                return prop.GetValue(el) as string ?? string.Empty;
            }
        }
        catch { }
        return string.Empty;
    }

    private static object TryAutoType(string text, string typeHint, string uiType)
    {
        if (string.IsNullOrEmpty(text)) return text; // keep empty string as-is

        if (!string.IsNullOrEmpty(typeHint))
        {
            switch (typeHint.ToLowerInvariant())
            {
                case "guid": if (Guid.TryParse(text, out var g)) return g; break;
                case "int": if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i)) return i; break;
                case "decimal": if (decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var d)) return d; break;
                case "float": if (float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var f)) return f; break;
                case "double": if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var db)) return db; break;
                case "datetime": if (DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dt)) return dt; break;
                case "bool": if (bool.TryParse(text, out var b)) return b; break;
                case "string": return text;
            }
        }

        // Heuristics
        if (Guid.TryParse(text, out var g2)) return g2;
        if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i2)) return i2;
        if (decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var d2)) return d2;
        if (DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dt2)) return dt2;
        if (bool.TryParse(text, out var b2)) return b2;

        return text; // default to string
    }

    private static bool StringEquals(string a, string b) => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
    private static bool StringEquals(string a, IXmlElement el) => string.Equals(a, el?.Name, StringComparison.OrdinalIgnoreCase);
    private static bool StringEquals(IXmlElement el, string b) => string.Equals(el?.Name, b, StringComparison.OrdinalIgnoreCase);

    private static bool? ParseBool(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;
        if (bool.TryParse(s, out var b)) return b;
        if (int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i)) return i != 0;
        return string.Equals(s, "true", StringComparison.OrdinalIgnoreCase);
    }

    private static int? ParseInt(string s)
    {
        if (int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i)) return i;
        return null;
    }

    private static Model.JoinType? ParseJoinType(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;
        return string.Equals(s, "inner", StringComparison.OrdinalIgnoreCase) ? Model.JoinType.Inner : Model.JoinType.Outer;
    }

    private static void CollectFxSettings(string fetchXml, IDictionary<string, string> settings)
    {
        if (string.IsNullOrEmpty(fetchXml)) return;
        var idxFetch = fetchXml.IndexOf("<fetch", StringComparison.OrdinalIgnoreCase);
        if (idxFetch < 0) return;
        var header = fetchXml.Substring(0, idxFetch);
        var lines = header.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var raw in lines)
        {
            var line = raw.Trim();
            if (line.Length == 0) continue;
            // strip comment markers if present
            if (line.StartsWith("<!--")) line = line.Substring(4);
            if (line.EndsWith("-->")) line = line.Substring(0, line.Length - 3);
            if (line.EndsWith("--!>")) line = line.Substring(0, line.Length - 4);
            line = line.Trim();
            if (!line.StartsWith("fx.", StringComparison.OrdinalIgnoreCase)) continue;
            var colon = line.IndexOf(':');
            if (colon <= 3) continue;
            var key = line.Substring(3, colon - 3).Trim();
            var value = line.Substring(colon + 1).Trim();
            if (key.Length > 0) settings[key] = value;
        }
    }

    private static void CollectFxSettings(XmlDocumentSyntax doc, IDictionary<string, string> settings)
    {
        if (doc == null) return;
        foreach (var node in doc.ChildNodes)
        {
            if (node.Kind == SyntaxKind.XmlComment)
            {
                var commentText = node.ToFullString();
                var match = FxSettingIncludingCommentChars.Match(commentText);
                if (match.Success && match.Groups.Count == 3)
                {
                    var key = match.Groups[1].Value;
                    var value = match.Groups[2].Value;
                    if (key.Length > 0) settings[key] = value;
                    continue;
                }
            }
            else if (node.Kind == SyntaxKind.XmlElement)
            {
                // Stop at the first element (likely <fetch>)
                break;
            }
        }
    }

    /// <summary>
    /// Collects inline parameters from a string value (e.g., "{{paramName}}" or "{{paramName:defaultValue}}")
    /// </summary>
    private static void CollectInlineParameters(string value, IList<Model.FetchParameter> parameters)
    {
        if (string.IsNullOrEmpty(value)) return;
        
        var matches = InlineParameterPattern.Matches(value);
        foreach (Match match in matches)
        {
            if (match.Success && match.Groups.Count >= 2)
            {
                var paramName = match.Groups[1].Value;
                var defaultValue = match.Groups.Count >= 3 && match.Groups[2].Success ? match.Groups[2].Value : null;
                
                // Check if parameter already exists
                if (!parameters.Any(p => string.Equals(p.Name, paramName, StringComparison.OrdinalIgnoreCase)))
                {
                    parameters.Add(new Model.FetchParameter
                    {
                        Name = paramName,
                        DefaultValue = defaultValue,
                        IsElement = false
                    });
                }
            }
        }
    }

    /// <summary>
    /// Recursively collects <param> elements from an XML element and its children
    /// </summary>
    private static void CollectParamElements(IXmlElement element, IList<Model.FetchParameter> parameters)
    {
        if (element == null) return;
        
        foreach (var child in element.Elements)
        {
            if (StringEquals(child.Name, "param"))
            {
                // Parse <param> element
                var paramName = child.Attributes.FirstOrDefault(a => string.Equals(a.Key, "name", StringComparison.OrdinalIgnoreCase))?.Value;
                if (!string.IsNullOrEmpty(paramName))
                {
                    // Get inner XML as default value
                    var defaultValue = TryGetElementInnerXml(child);
                    
                    // Check if parameter already exists
                    if (!parameters.Any(p => string.Equals(p.Name, paramName, StringComparison.OrdinalIgnoreCase)))
                    {
                        parameters.Add(new Model.FetchParameter
                        {
                            Name = paramName,
                            DefaultValue = defaultValue,
                            IsElement = true
                        });
                    }
                }
            }
            else
            {
                // Also collect inline parameters from attributes of all elements
                foreach (var attr in child.Attributes)
                {
                    CollectInlineParameters(attr.Value, parameters);
                }
                
                // Recurse into child elements
                CollectParamElements(child, parameters);
            }
        }
    }

    /// <summary>
    /// Gets the inner XML content of an element (all child nodes as XML string)
    /// </summary>
    private static string TryGetElementInnerXml(IXmlElement element)
    {
        if (element == null) return string.Empty;
        
        try
        {
            // Get all child elements and convert to XML string
            var childElements = element.Elements.ToList();
            if (childElements.Count > 0)
            {
                // Use ToFullString() to get the XML representation
                return string.Join("", childElements.Select(e => e.AsNode?.ToFullString() ?? string.Empty)).Trim();
            }
            
            // If no child elements, try to get text content
            return TryGetElementInnerText(element);
        }
        catch
        {
            return string.Empty;
        }
    }
}