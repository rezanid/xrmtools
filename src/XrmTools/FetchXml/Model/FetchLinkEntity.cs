namespace XrmTools.FetchXml.Model;

public enum JoinType
{
    Inner,
    Outer
}

/// <summary>
/// Represents <link-entity ...>.
/// </summary>
public sealed class FetchLinkEntity : FetchNode
{
    /// <summary>
    /// from="accountid"
    /// </summary>
    public string From { get; set; }
    /// <summary>
    /// to="accountid"
    /// </summary>
    public string To { get; set; }
    /// <summary>
    /// link-type="inner|outer"
    /// </summary>
    public JoinType? LinkType { get; set; }
    /// <summary>
    /// intersect="true|false"
    /// </summary>
    public bool? Intersect { get; set; }
}
