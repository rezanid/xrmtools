#nullable enable
namespace XrmTools.PluginTrace;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

/// <summary>Immutable snapshots keep the selected trace and its text stable during polling.</summary>
public sealed class TraceRecord
{
    public Guid Id { get; }
    public DateTimeOffset CreatedOn { get; }
    public string Time => CreatedOn.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss.fff");
    public string TypeName { get; }
    public string ShortTypeName => TypeName.Split(',')[0].Split('.').Last();
    public string MessageName { get; }
    public string Entity { get; }
    public int Duration { get; }
    public int Depth { get; }
    public Guid? CorrelationId { get; }
    public string Message { get; }
    public string Exception { get; }
    public string Status => string.IsNullOrWhiteSpace(Exception) ? "Trace" : "Exception";
    public string Raw { get; }

    public TraceRecord(JsonElement row)
    {
        Id = row.GetProperty("plugintracelogid").GetGuid();
        CreatedOn = row.GetProperty("createdon").GetDateTimeOffset();
        TypeName = Text(row, "typename");
        MessageName = Text(row, "messagename");
        Entity = Text(row, "primaryentity");
        Duration = Number(row, "performanceexecutionduration");
        Depth = Number(row, "depth");
        CorrelationId = Guid.TryParse(Text(row, "correlationid"), out var correlation) ? correlation : null;
        Message = Text(row, "messageblock");
        Exception = Text(row, "exceptiondetails");
        Raw = JsonSerializer.Serialize(row, new JsonSerializerOptions { WriteIndented = true });
    }

    private static string Text(JsonElement row, string property) => row.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() ?? "" : "";
    private static int Number(JsonElement row, string property) => row.TryGetProperty(property, out var value) && value.TryGetInt32Safe(out var number) ? number : 0;
}

internal static class TraceJsonExtensions
{
    public static bool TryGetInt32Safe(this JsonElement value, out int number)
    {
        number = 0;
        return value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out number);
    }
}

public static class TraceSnapshots
{
    public static IReadOnlyList<TraceRecord> Order(IEnumerable<TraceRecord> rows) => rows
        .GroupBy(r => r.Id).Select(g => g.Last()).OrderByDescending(r => r.CreatedOn).ThenByDescending(r => r.Id).ToList();

    public static bool Changed(IReadOnlyList<TraceRecord> current, IReadOnlyList<TraceRecord> incoming) =>
        current.Count != incoming.Count || !current.Select(r => r.Raw).SequenceEqual(incoming.Select(r => r.Raw));

    public static IReadOnlyList<TraceRecord> PreserveSelection(IEnumerable<TraceRecord> incoming, TraceRecord? selected)
    {
        if (selected == null) return Order(incoming);
        return Order(incoming.Where(r => r.Id != selected.Id).Concat(new[] { selected }));
    }
}
