namespace XrmTools.FetchXml.Model;

/// <summary>
/// Represents <entity ...> content.
/// </summary>
public sealed class FetchEntity : FetchNode
{

    /// <summary>
    /// enableprefiltering="true|false"
    /// </summary>
    public bool? EnablePrefiltering { get; set; }
    /// <summary>
    /// prefilterparametername="CRM_..."
    /// </summary>
    public string PrefilterParameterName { get; set; }
    /// <summary>
    /// present if <all-attributes /> under this entity
    /// </summary>
    public bool? AllAttributes { get; set; }
 }
