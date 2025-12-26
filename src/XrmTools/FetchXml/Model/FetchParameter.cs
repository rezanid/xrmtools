#nullable enable
namespace XrmTools.FetchXml.Model;

using System;

/// <summary>
/// Represents a parameter in a FetchXML query.
/// Parameters can be either element-based (&lt;param name='...'&gt;...&lt;/param&gt;)
/// or value-based ({{paramName}} or {{paramName:defaultValue}}).
/// </summary>
public class FetchParameter : IComparable<FetchParameter>
{
    /// <summary>
    /// Name of the parameter (e.g., "filterXml" or "entityName")
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Default value for the parameter (can be XML content or simple text)
    /// </summary>
    public string? DefaultValue { get; set; }

    /// <summary>
    /// Indicates if this is an element-based parameter (true) or value-based (false)
    /// </summary>
    public bool IsElementParameter { get; set; }

    /// <summary>
    /// Default sorting will put optional parameters at the end of 
    /// the list to make code-generation more fluent.
    /// </summary>
    public int CompareTo(FetchParameter other)
    {
        if (other == null) return 1;
        if (DefaultValue == null && other.DefaultValue == null) return 0;
        if (DefaultValue == null) return -1;
        if (other.DefaultValue == null) return 1;
        if (DefaultValue.Length != other.DefaultValue.Length)
        {
            return DefaultValue.Length.CompareTo(other.DefaultValue.Length);
        }
        return 0;
    }
}
#nullable restore