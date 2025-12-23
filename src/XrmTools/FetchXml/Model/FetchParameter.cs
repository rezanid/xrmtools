namespace XrmTools.FetchXml.Model;

/// <summary>
/// Represents a parameter in a FetchXML query.
/// Parameters can be either element-based (&lt;param name='...'&gt;...&lt;/param&gt;)
/// or value-based ({{paramName}} or {{paramName:defaultValue}}).
/// </summary>
public class FetchParameter
{
    /// <summary>
    /// Name of the parameter (e.g., "filterXml" or "entityName")
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Default value for the parameter (can be XML content or simple text)
    /// </summary>
    public string DefaultValue { get; set; }

    /// <summary>
    /// Indicates if this is an element-based parameter (true) or value-based (false)
    /// </summary>
    public bool IsElementParameter { get; set; }

    /// <summary>
    /// For element parameters, this stores the inner XML content as the default
    /// </summary>
    public string InnerXml { get; set; }
}
