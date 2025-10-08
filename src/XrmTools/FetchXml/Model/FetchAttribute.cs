namespace XrmTools.FetchXml.Model;
using System;
using System.Collections.Generic;

/// <summary>
/// Represents <attribute .../> and aggregate/group-by scenarios.
/// </summary>
public sealed class FetchAttribute
{
    /// <summary>
    /// attribute logical name
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// alias for projection
    /// </summary>
    public string Alias { get; set; }

    // Aggregates / grouping (only meaningful when fetch aggregate="true")

    /// <summary>
    /// raw aggregate name: "count", "countcolumn", "sum", "min", "max", "avg"
    /// </summary>
    public string Aggregate { get; set; }
    /// <summary>
    /// groupby="true|false"
    /// </summary>
    public bool? GroupBy { get; set; }
    /// <summary>
    /// dategrouping="day|week|month|quarter|year"
    /// </summary>
    public string DateGrouping { get; set; }
    /// <summary>
    /// distinct="true|false" (for count)
    /// </summary>
    public bool? Distinct { get; set; }
    /// <summary>
    /// usetimezone="1" (rare / legacy)
    /// </summary>
    public int? UserTimeZone { get; set; }

    // For preserving uncommon attributes (e.g., "ui*")
    public Dictionary<string, string> Extras { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}
