#nullable enable
namespace XrmTools.PluginTrace;

using System;
using System.Collections.Generic;
using System.Globalization;

/// <summary>Saved relative windows are resolved at query time, never at save time.</summary>
public sealed record TraceFilter
{
    public string Name { get; set; } = "";
    public int Minutes { get; set; } = 30;
    public DateTimeOffset? From { get; set; }
    public DateTimeOffset? To { get; set; }
    public string TypeName { get; set; } = "";
    public bool ErrorsOnly { get; set; }
    public bool FullControl { get; set; }
    public string Expression { get; set; } = "";

    public string Build(DateTimeOffset now)
    {
        if (FullControl) return Expression.Trim();
        var clauses = new List<string>();
        if (Minutes < 0) throw new FormatException("Choose a valid duration.");
        var from = Minutes == 0 ? From : now.AddMinutes(-Minutes);
        var to = Minutes == 0 ? To : now;
        if (from == null || to == null || from >= to)
            throw new FormatException("Enter a start and end time, with the end after the start.");
        clauses.Add($"createdon ge {Format(from.Value)} and createdon le {Format(to.Value)}");
        if (!string.IsNullOrWhiteSpace(TypeName)) clauses.Add($"contains(typename,'{TypeName.Trim().Replace("'", "''")}')");
        if (ErrorsOnly) clauses.Add("exceptiondetails ne null and exceptiondetails ne ''");
        if (!string.IsNullOrWhiteSpace(Expression)) clauses.Add(Expression.Trim());
        return "(" + string.Join(") and (", clauses) + ")";
    }

    public string Query(DateTimeOffset now)
    {
        var filter = Build(now);
        return "plugintracelogs?$select=plugintracelogid,createdon,performanceexecutionstarttime,typename,messagename,primaryentity,performanceexecutionduration,depth,mode,correlationid,exceptiondetails,messageblock"
            + "&$orderby=createdon desc,plugintracelogid desc"
            + (filter.Length == 0 ? "" : "&$filter=" + Uri.EscapeDataString(filter));
    }

    private static string Format(DateTimeOffset value) => value.UtcDateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture);
}
