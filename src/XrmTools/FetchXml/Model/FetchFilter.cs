namespace XrmTools.FetchXml.Model;
using System;
using System.Collections.Generic;

public enum LogicalOperator
{
    And,
    Or
}

/// <summary>
/// Represents <filter type="and|or"> containing conditions and nested filters.
/// </summary>
public sealed class FetchFilter
{
    public LogicalOperator Type { get; set; } = LogicalOperator.And;
    public List<FetchCondition> Conditions { get; } = [];
    public List<FetchFilter> Filters { get; } = [];

    /// <summary>
    /// For preserving uncommon attributes
    /// </summary>
    public Dictionary<string, string> Extras { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}
