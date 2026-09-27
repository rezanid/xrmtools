namespace XrmTools.Core.Tests.PluginTrace;

using System;
using System.Linq;
using System.Text.Json;
using XrmTools.PluginTrace;
using Xunit;

public class TraceExplorerTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 22, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void RelativeWindowMovesWithEachRefreshAndEscapesTypeNames()
    {
        var filter = new TraceFilter { Minutes = 30, TypeName = "O'Brien.Account", ErrorsOnly = true, Expression = "depth eq 1 or depth eq 2" };
        string first = filter.Build(Now);
        Assert.Contains("createdon ge 2026-09-22T11:30:00.000Z", first);
        Assert.Contains("contains(typename,'O''Brien.Account')", first);
        Assert.Contains("and (depth eq 1 or depth eq 2)", first);
        Assert.Contains("createdon ge 2026-09-22T12:30:00.000Z", filter.Build(Now.AddHours(1)));
    }

    [Fact]
    public void FullControlReplacesAllQuickFiltersButCannotOverrideOrdering()
    {
        var filter = new TraceFilter { FullControl = true, Expression = "contains(typename,'A&B')", Minutes = -1, ErrorsOnly = true, TypeName = "Ignored" };
        Assert.Equal(filter.Expression, filter.Build(Now));
        Assert.Contains("$orderby=createdon desc,plugintracelogid desc", filter.Query(Now));
        Assert.Contains("A%26B", filter.Query(Now));
        Assert.DoesNotContain("Ignored", filter.Query(Now));
        Assert.DoesNotContain("&B", filter.Query(Now));
    }

    [Fact]
    public void CustomWindowNormalizesToUtcAndRejectsInvalidRanges()
    {
        var filter = new TraceFilter { Minutes = 0, From = Now.ToOffset(TimeSpan.FromHours(2)), To = Now.AddHours(1) };
        Assert.Contains("createdon ge 2026-09-22T12:00:00.000Z", filter.Build(Now));
        Assert.Throws<FormatException>(() => (filter with { To = filter.From }).Build(Now));
        Assert.Throws<FormatException>(() => (filter with { From = null }).Build(Now));
    }

    [Fact]
    public void SavedFilterRoundTripsWithoutFreezingRelativeTime()
    {
        var filter = new TraceFilter { Name = "Account failures", Minutes = 60, ErrorsOnly = true };
        var restored = JsonSerializer.Deserialize<TraceFilter>(JsonSerializer.Serialize(filter));
        Assert.Equal(filter, restored);
        Assert.NotEqual(restored.Build(Now), restored.Build(Now.AddMinutes(10)));
    }

    [Fact]
    public void SnapshotsAreNewestFirstAndDeduplicateRecords()
    {
        var earlier = Row(1, Now.AddSeconds(-1), "first");
        var later = Row(2, Now, "second");
        var updated = Row(2, Now, "updated");
        var result = TraceSnapshots.Order(new[] { later, earlier, updated });
        Assert.Equal(new[] { later.Id, earlier.Id }, result.Select(r => r.Id));
        Assert.Same(updated, result[0]);
    }

    [Fact]
    public void SelectedSnapshotSurvivesUpdatesAndAgingOutWithoutChangingItsText()
    {
        var selected = Row(1, Now, "reading this");
        var updated = Row(1, Now, "changed by server");
        var newer = Row(2, Now.AddSeconds(1), "new trace");
        var merged = TraceSnapshots.PreserveSelection(new[] { updated, newer }, selected);
        Assert.Equal(new[] { newer.Id, selected.Id }, merged.Select(r => r.Id));
        Assert.Same(selected, merged[1]);
        Assert.Equal("reading this", merged[1].Message);
        var expired = TraceSnapshots.PreserveSelection(new[] { newer }, selected);
        Assert.Equal(new[] { newer.Id, selected.Id }, expired.Select(r => r.Id));
        Assert.Same(selected, expired[1]);
        Assert.True(TraceSnapshots.Changed(new[] { selected }, new[] { updated }));
        Assert.False(TraceSnapshots.Changed(new[] { selected }, new[] { Row(1, Now, "reading this") }));
    }

    private static TraceRecord Row(int id, DateTimeOffset created, string message)
    {
        using var json = JsonDocument.Parse(JsonSerializer.Serialize(new
        {
            plugintracelogid = new Guid(id, 0, 0, new byte[8]), createdon = created, messageblock = message,
            typename = "Contoso.AccountPlugin", performanceexecutionduration = (int?)null
        }));
        return new TraceRecord(json.RootElement);
    }
}
