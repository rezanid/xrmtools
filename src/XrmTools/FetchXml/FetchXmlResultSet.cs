namespace XrmTools.FetchXml;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

internal sealed class FetchXmlResultSet
{
    private const string FormattedValueSuffix = "@OData.Community.Display.V1.FormattedValue";

    private FetchXmlResultSet(IReadOnlyList<FetchXmlResultColumn> columns, IReadOnlyList<FetchXmlResultRow> rows)
    {
        Columns = columns;
        Rows = rows;
    }

    public static FetchXmlResultSet Empty { get; } = new([], []);
    public IReadOnlyList<FetchXmlResultColumn> Columns { get; }
    public IReadOnlyList<FetchXmlResultRow> Rows { get; }

    public static FetchXmlResultSet Create(JArray records)
    {
        if (records is null || records.Count == 0) return Empty;

        var names = new List<string>();
        var knownNames = new HashSet<string>(StringComparer.Ordinal);
        foreach (var record in records.OfType<JObject>())
        {
            foreach (var property in record.Properties())
            {
                if (IsAnnotation(property.Name) || !knownNames.Add(property.Name)) continue;
                names.Add(property.Name);
            }
        }

        var columns = names.Select((name, index) => new FetchXmlResultColumn(name, index)).ToArray();
        var rows = new List<FetchXmlResultRow>(records.Count);
        foreach (var record in records.OfType<JObject>())
        {
            var rawValues = new JToken?[columns.Length];
            var displayValues = new string[columns.Length];
            for (var index = 0; index < columns.Length; index++)
            {
                var name = columns[index].Name;
                rawValues[index] = record[name];
                displayValues[index] = ToDisplayText(record[name + FormattedValueSuffix] ?? rawValues[index]);
            }
            rows.Add(new FetchXmlResultRow(rawValues, displayValues));
        }

        return new FetchXmlResultSet(columns, rows);
    }

    private static bool IsAnnotation(string name) => name.Contains("@");

    private static string ToDisplayText(JToken? value)
    {
        if (value is null || value.Type is JTokenType.Null or JTokenType.Undefined) return string.Empty;
        if (value is JValue scalar)
        {
            return scalar.Value switch
            {
                null => string.Empty,
                DateTime dateTime => dateTime.ToString(CultureInfo.CurrentCulture),
                DateTimeOffset dateTimeOffset => dateTimeOffset.ToString(CultureInfo.CurrentCulture),
                IFormattable formattable => formattable.ToString(null, CultureInfo.CurrentCulture),
                _ => scalar.Value?.ToString() ?? string.Empty,
            };
        }
        return value.ToString(Formatting.None);
    }
}

internal sealed class FetchXmlResultColumn(string name, int index)
{
    public string Name { get; } = name;
    public int Index { get; } = index;
}

internal sealed class FetchXmlResultRow
{
    private readonly JToken?[] rawValues;
    private readonly string[] displayValues;

    public FetchXmlResultRow(JToken?[] rawValues, string[] displayValues)
    {
        this.rawValues = rawValues;
        this.displayValues = displayValues;
    }

    public string this[int index] => displayValues[index];
    public JToken? GetRawValue(int index) => rawValues[index];
}

internal sealed class FetchXmlResultRowComparer(int columnIndex, ListSortDirection direction) : IComparer
{
    public int Compare(object? x, object? y)
    {
        var result = CompareTokens(
            (x as FetchXmlResultRow)?.GetRawValue(columnIndex),
            (y as FetchXmlResultRow)?.GetRawValue(columnIndex));
        return direction == ListSortDirection.Ascending ? result : -result;
    }

    private static int CompareTokens(JToken? left, JToken? right)
    {
        if (left is null || left.Type is JTokenType.Null or JTokenType.Undefined)
            return right is null || right.Type is JTokenType.Null or JTokenType.Undefined ? 0 : 1;
        if (right is null || right.Type is JTokenType.Null or JTokenType.Undefined) return -1;

        if (left is JValue leftValue && right is JValue rightValue)
        {
            if (IsNumeric(left.Type) && IsNumeric(right.Type))
            {
                return Convert.ToDecimal(leftValue.Value, CultureInfo.InvariantCulture)
                    .CompareTo(Convert.ToDecimal(rightValue.Value, CultureInfo.InvariantCulture));
            }

            var rightScalar = rightValue.Value;
            if (leftValue.Value is IComparable comparable && rightScalar is not null && leftValue.Value.GetType() == rightScalar.GetType())
            {
                return comparable.CompareTo(rightScalar);
            }
        }

        return StringComparer.CurrentCultureIgnoreCase.Compare(left.ToString(), right.ToString());
    }

    private static bool IsNumeric(JTokenType type) => type is JTokenType.Integer or JTokenType.Float;
}
