namespace XrmTools.FetchXml.Model;
using System.Collections.Generic;

/// <summary>
/// Common base for nodes that can have attributes/orders/filters and nested link-entities.
/// Both <entity> and <link-entity> share these.
/// </summary>
public abstract class FetchNode
{
    /// <summary>
    /// logical entity name (required)
    /// </summary>
    public string Name { get; set; }   // logical table name
    /// <summary>
    /// alias="e" etc.
    /// </summary>
    public string Alias { get; set; }

    public List<FetchAttribute> Attributes { get; } = new();
    public List<FetchOrder> Orders { get; } = new();
    public List<FetchFilter> Filters { get; } = new();

    // Nested <link-entity> only (children are always links)
    public List<FetchLinkEntity> Links { get; } = new();
}
