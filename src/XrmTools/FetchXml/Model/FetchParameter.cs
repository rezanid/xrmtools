namespace XrmTools.FetchXml.Model;

/// <summary>
/// Represents a parameter in a parametric FetchXML query.
/// Can be defined as <param name="..." /> or <param name="...">defaultValue</param>
/// or inline as {{paramName}} or {{paramName:defaultValue}}
/// </summary>
public sealed class FetchParameter
{
    /// <summary>
    /// Parameter name (required)
    /// </summary>
    public required string Name { get; set; }
    
    /// <summary>
    /// Default value (optional). Can be XML string for <param> elements or simple value for inline parameters.
    /// </summary>
    public string? DefaultValue { get; set; }
    
    /// <summary>
    /// Whether the parameter is defined as an element (<param>) vs inline ({{name}})
    /// </summary>
    public bool IsElement { get; set; }
}
