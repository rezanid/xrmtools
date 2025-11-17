namespace XrmTools.FetchXml.Model;
using System;
using System.Collections.Generic;

/// <summary>
/// Represents <order .../>.
/// </summary>
public sealed class FetchOrder
{
    /// <summary>
    /// attribute="name"
    /// </summary>
    public string Attribute { get; set; }
    /// <summary>
    /// descending="true|false"
    /// </summary>
    public bool Descending { get; set; }
    /// <summary>
    /// alias-based ordering (rare)
    /// </summary>
    public string Alias { get; set; }

    public Dictionary<string, string> Extras { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}
