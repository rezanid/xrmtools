namespace XrmTools.FetchXml.Model;
/// <summary>
/// Root of the FetchXML model (represents <fetch ...>).
/// </summary>
public sealed class FetchQuery
{
    /// <summary>
    /// e.g. "1.0"
    /// </summary>
    public string Version { get; set; }
    /// <summary>
    /// distinct="true|false"
    /// </summary>
    public bool? Distinct { get; set; }
    /// <summary>
    /// no-lock="true|false"
    /// </summary>
    public bool? NoLock { get; set; }
    /// <summary>
    /// returntotalrecordcount="true|false"
    /// </summary>
    public bool? ReturnTotalRecordCount { get; set; }
    /// <summary>
    /// aggregate="true|false"
    /// </summary>
    public bool? Aggregate { get; set; }
    /// <summary>
    /// count="5000"
    /// </summary>
    public int? Count { get; set; }
    /// <summary>
    /// page="1"
    /// </summary>
    public int? Page { get; set; }
    /// <summary>
    /// top="10"
    /// </summary>
    public int? Top { get; set; }
    /// <summary>
    /// paging-cookie="..."
    /// </summary>
    public string PagingCookie { get; set; }
    /// <summary>
    /// output-format="xml-platform"
    /// </summary>
    public string OutputFormat { get; set; }
    /// <summary>
    /// mapping="logical" (legacy)
    /// </summary>
    public string Mapping { get; set; }
    /// <summary>
    /// min-active-row-version="true|false"
    /// </summary>
    public string MinActiveRowVersion { get; set; }

    // The root entity
    public FetchEntity Entity { get; set; }

    /// <summary>
    /// Arbitrary settings parsed from comments immediately preceding the <fetch> element
    /// with the pattern <!-- fx.key: value -->
    /// </summary>
    public System.Collections.Generic.Dictionary<string, string> Settings { get; } = new System.Collections.Generic.Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// For preserving uncommon attributes on <fetch>
    /// </summary>
    public System.Collections.Generic.Dictionary<string, string> Extras { get; } = new System.Collections.Generic.Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Parameters detected in the FetchXML query
    /// </summary>
    public System.Collections.Generic.List<FetchParameter> Parameters { get; set; } = [];

    public string Raw { get; set; }
    public string Tokenized { get; set; }
    public string Defaulted { get; set; }
}