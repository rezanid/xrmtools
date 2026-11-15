#nullable enable
namespace XrmTools.Pcf.Setup;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

internal readonly struct NodeVersion : IComparable<NodeVersion>, IEquatable<NodeVersion>
{
    public NodeVersion(int major, int minor, int patch) { Major = major; Minor = minor; Patch = patch; }
    public int Major { get; }
    public int Minor { get; }
    public int Patch { get; }

    public static bool TryParse(string? text, out NodeVersion version)
    {
        version = default;
        var match = Regex.Match(text?.Trim() ?? string.Empty, @"\Av?(\d{1,9})\.(\d{1,9})\.(\d{1,9})(?:[-+][0-9A-Za-z.+-]*)?\z");
        if (!match.Success) return false;
        version = new NodeVersion(Number(match.Groups[1].Value), Number(match.Groups[2].Value), Number(match.Groups[3].Value));
        return true;
    }

    public int CompareTo(NodeVersion other)
        => Major != other.Major ? Major.CompareTo(other.Major) : Minor != other.Minor ? Minor.CompareTo(other.Minor) : Patch.CompareTo(other.Patch);
    public bool Equals(NodeVersion other) => CompareTo(other) == 0;
    public override bool Equals(object? obj) => obj is NodeVersion other && Equals(other);
    public override int GetHashCode() => (Major * 397 ^ Minor) * 397 ^ Patch;
    public override string ToString() => $"v{Major}.{Minor}.{Patch}";
    public static bool operator <(NodeVersion left, NodeVersion right) => left.CompareTo(right) < 0;
    public static bool operator >(NodeVersion left, NodeVersion right) => left.CompareTo(right) > 0;
    public static bool operator <=(NodeVersion left, NodeVersion right) => left.CompareTo(right) <= 0;
    public static bool operator >=(NodeVersion left, NodeVersion right) => left.CompareTo(right) >= 0;

    private static int Number(string digits) => int.Parse(digits, CultureInfo.InvariantCulture);
}

/// <summary>Evaluates the npm range syntax used by <c>engines.node</c> and the partial versions used by version files.</summary>
internal static class NodeVersionRange
{
    private static readonly NodeVersion Highest = new(int.MaxValue, 0, 0);

    /// <returns><c>null</c> when the range is not understood; callers must not treat that as a mismatch.</returns>
    public static bool? Satisfies(NodeVersion version, string? range)
    {
        if (string.IsNullOrWhiteSpace(range)) return true;
        bool? result = false;
        foreach (var alternative in range!.Split(["||"], StringSplitOptions.None))
        {
            var matched = SatisfiesAll(version, alternative.Trim());
            if (matched == true) return true;
            if (matched is null) result = null;
        }
        return result;
    }

    /// <summary>Matches <c>22</c>, <c>22.15</c> and <c>v22.15.0</c>; returns <c>null</c> for aliases such as <c>lts/*</c>.</summary>
    public static bool? MatchesPartial(NodeVersion version, string? spec)
    {
        var text = spec?.Trim() ?? string.Empty;
        if (text.StartsWith("v", StringComparison.OrdinalIgnoreCase)) text = text.Substring(1);
        return TryPartial(text, out var partial) && partial.Major is not null ? Bounds(partial).Contains(version) : null;
    }

    private static bool? SatisfiesAll(NodeVersion version, string alternative)
    {
        if (alternative.Length == 0) return true;
        var hyphen = Regex.Match(alternative, @"\A(\S+)\s+-\s+(\S+)\z");
        if (hyphen.Success)
        {
            if (!TryPartial(Strip(hyphen.Groups[1].Value), out var low) || !TryPartial(Strip(hyphen.Groups[2].Value), out var high)) return null;
            return version >= Bounds(low).Minimum && version < Bounds(high).Maximum;
        }
        foreach (var comparator in Regex.Replace(alternative, @"([<>=~^]+)\s+", "$1").Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries))
        {
            var range = Comparator(comparator);
            if (range is null) return null;
            if (!range.Value.Contains(version)) return false;
        }
        return true;
    }

    private static Interval? Comparator(string comparator)
    {
        var match = Regex.Match(comparator, @"\A(\^|~>?|>=|<=|>|<|=)?(.*)\z");
        var op = match.Groups[1].Value;
        if (!TryPartial(Strip(match.Groups[2].Value), out var partial)) return null;
        var bounds = Bounds(partial);
        var floor = bounds.Minimum;
        switch (op)
        {
            case "": case "=": return bounds;
            case ">=": return new Interval(floor, Highest);
            case ">": return new Interval(partial.IsFull ? Next(floor) : bounds.Maximum, Highest);
            case "<": return new Interval(default, floor);
            case "<=": return new Interval(default, partial.IsFull ? Next(floor) : bounds.Maximum);
            case "^":
                if (partial.Major is null) return bounds;
                if (floor.Major > 0 || partial.Minor is null) return new Interval(floor, new NodeVersion(floor.Major + 1, 0, 0));
                if (floor.Minor > 0 || partial.Patch is null) return new Interval(floor, new NodeVersion(0, floor.Minor + 1, 0));
                return new Interval(floor, Next(floor));
            default:
                if (partial.Major is null) return bounds;
                return partial.Minor is null
                    ? new Interval(floor, new NodeVersion(floor.Major + 1, 0, 0))
                    : new Interval(floor, new NodeVersion(floor.Major, floor.Minor + 1, 0));
        }
    }

    private static Interval Bounds(Partial partial)
    {
        if (partial.Major is null) return new Interval(default, Highest);
        if (partial.Minor is null) return new Interval(new NodeVersion(partial.Major.Value, 0, 0), new NodeVersion(partial.Major.Value + 1, 0, 0));
        if (partial.Patch is null) return new Interval(new NodeVersion(partial.Major.Value, partial.Minor.Value, 0), new NodeVersion(partial.Major.Value, partial.Minor.Value + 1, 0));
        var exact = new NodeVersion(partial.Major.Value, partial.Minor.Value, partial.Patch.Value);
        return new Interval(exact, Next(exact));
    }

    private static NodeVersion Next(NodeVersion version) => new(version.Major, version.Minor, version.Patch + 1);
    private static string Strip(string text) => text.StartsWith("v", StringComparison.OrdinalIgnoreCase) ? text.Substring(1) : text;

    private static bool TryPartial(string text, out Partial partial)
    {
        partial = default;
        var core = text.Split('-', '+')[0];
        if (core.Length == 0) return true;
        var parts = core.Split('.');
        if (parts.Length > 3) return false;
        var numbers = new List<int?>();
        foreach (var part in parts)
        {
            if (part is "x" or "X" or "*") { numbers.Add(null); continue; }
            if (part.Length is 0 or > 9 || !part.All(char.IsDigit)) return false;
            numbers.Add(int.Parse(part, CultureInfo.InvariantCulture));
        }
        // A wildcard makes every following part a wildcard: 1.x.3 means 1.x.
        var wildcard = numbers.FindIndex(number => number is null);
        if (wildcard >= 0) numbers = numbers.Take(wildcard).ToList();
        partial = new Partial(numbers.ElementAtOrDefault(0), numbers.ElementAtOrDefault(1), numbers.ElementAtOrDefault(2));
        return true;
    }

    private readonly struct Partial(int? major, int? minor, int? patch)
    {
        public int? Major { get; } = major;
        public int? Minor { get; } = minor;
        public int? Patch { get; } = patch;
        public bool IsFull => Patch is not null;
    }

    private readonly struct Interval(NodeVersion minimum, NodeVersion maximum)
    {
        public NodeVersion Minimum { get; } = minimum;
        /// <summary>Exclusive upper bound.</summary>
        public NodeVersion Maximum { get; } = maximum;
        public bool Contains(NodeVersion version) => version >= Minimum && version < Maximum;
    }
}
#nullable restore
