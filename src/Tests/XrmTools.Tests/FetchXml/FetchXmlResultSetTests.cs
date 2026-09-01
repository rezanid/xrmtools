namespace XrmTools.Tests.FetchXml;

using Newtonsoft.Json.Linq;
using System.ComponentModel;
using System.Linq;
using XrmTools.FetchXml;
using Xunit;

public class FetchXmlResultSetTests
{
    [Fact]
    public void Create_UsesUnionOfColumnsAndFormattedValues()
    {
        var records = JArray.Parse("""
        [
          {
            "name": "Contoso",
            "name@OData.Community.Display.V1.FormattedValue": "Contoso (formatted)",
            "revenue": 12.5
          },
          {
            "name": "Fabrikam",
            "address1_city": "Paris"
          }
        ]
        """);

        var result = FetchXmlResultSet.Create(records);

        Assert.Equal(new[] { "name", "revenue", "address1_city" }, result.Columns.Select(column => column.Name));
        Assert.Equal("Contoso (formatted)", result.Rows[0][0]);
        Assert.Equal("Paris", result.Rows[1][2]);
        Assert.Equal(string.Empty, result.Rows[0][2]);
    }

    [Fact]
    public void RowComparer_SortsNumbersByRawValueAndNullsLast()
    {
        var result = FetchXmlResultSet.Create(JArray.Parse("""
        [
          { "value": 10 },
          { "value": 2 },
          { "value": null }
        ]
        """));
        var comparer = new FetchXmlResultRowComparer(0, ListSortDirection.Ascending);

        Assert.True(comparer.Compare(result.Rows[0], result.Rows[1]) > 0);
        Assert.True(comparer.Compare(result.Rows[2], result.Rows[0]) > 0);
    }
}
