namespace XrmTools.FetchXml.Model;
using System;
using System.Collections.Generic;

/// <summary>
/// Represents a <value ...> element under <condition>.
/// </summary>
public sealed class FetchValue
{
    /// <summary>
    /// inner text of <value>
    /// </summary>
    public string Text { get; set; }
    /// <summary>
    /// uitype="account" (if present)
    /// </summary>
    public string Uitype { get; set; }
    /// <summary>
    /// uiname="Contoso" (if present)
    /// </summary>
    public string Uiname { get; set; }

    /// <summary>
    /// e.g., Guid, int, decimal, DateTime, bool, string
    /// </summary>
    public object Typed { get; set; }
    /// <summary>
    /// optional hint like "guid", "int", "decimal", "datetime", "bool", "string"
    /// </summary>
    public string TypeHint { get; set; }

    public Dictionary<string, string> Extras { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}
