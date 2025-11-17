namespace XrmTools.FetchXml.Model;
using System;
using System.Collections.Generic;

/// <summary>
/// Represents <condition attribute="..." operator="..."> with optional one or more <value> children.
/// </summary>
public sealed class FetchCondition
{
    /// <summary>
    /// attribute being tested (can be aliased / link-entity alias qualified in XML)
    /// </summary>
    public string Attribute { get; set; }
    /// <summary>
    /// resolved entity alias (if attribute was qualified like alias.attribute)
    /// </summary>
    public string EntityAlias { get; set; }
    /// <summary>
    /// keep raw operator string: "eq", "ne", "in", "between", "eq-userid", "last-x-days", etc.
    /// </summary>
    public string Operator { get; set; }
    /// <summary>
    /// true if operator is 'null' / 'not-null' (so values list is empty)
    /// </summary>
    public bool? ValueIsNull { get; set; }
    public List<FetchValue> Values { get; } = new List<FetchValue>();

    /// <summary>
    /// Common operator extra flags/attrs (preserved raw)
    /// </summary>
    public Dictionary<string, string> Extras { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}
