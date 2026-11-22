namespace XrmTools.Core.Tests.OData;

using System.Linq;
using System.Threading;
using XrmTools.OData;
using Xunit;

public class ODataCompletionTests
{
    [Fact]
    public void FindsCapturedPathAndSuggestsKeysAcrossSampledArrayItems()
    {
        const string line = "GET /{{list.response.body.$.value[*].";
        Assert.True(ODataCompletion.TryGetCapturedSpan(line, line.Length, out int start, out int length, out string name, out string path));
        Assert.Equal(".", line.Substring(start, length));
        Assert.Equal("list", name);
        Assert.Equal("$.value[*]", path);
        using var capture = new ODataCapturedResponse { Body = "{\"value\":[{\"accountid\":1,\"nested\":[1,2]},{\"@odata.etag\":\"tag\"}]}" };
        Assert.Equal(new[] { "['@odata.etag']", ".accountid", ".nested" }, capture.GetBodyCompletion(path, CancellationToken.None));
        Assert.Equal(new[] { "[0]", "[*]" }, capture.GetBodyCompletion("$.value", CancellationToken.None));
    }

    [Theory]
    [InlineData("GET /{{|}}", "", "")]
    [InlineData("GET /{{  acc|ountId}}", "", "accountId")]
    [InlineData("X-ID: {{list.res|ponse.body.$}}", "list.", "response")]
    [InlineData("X-ID: {{list.response.body.|}}", "list.response.body.", "")]
    public void ReplacesOnlyTheCurrentSegment(string markedLine, string expectedQualifier, string expectedText)
    {
        int caret = markedLine.IndexOf('|');
        string line = markedLine.Remove(caret, 1);
        Assert.True(ODataCompletion.TryGetSpan(line, caret, out int start, out int length, out var qualifier));
        Assert.Equal(expectedQualifier, qualifier);
        Assert.Equal(expectedText, line.Substring(start, length));
    }

    [Theory]
    [InlineData("# Use {{")]
    [InlineData("GET /{{name}}")]
    [InlineData("GET /{{list.response.body.$.value")]
    public void DoesNotCompleteOutsideSupportedPlaceholderPositions(string line)
        => Assert.False(ODataCompletion.TryGetSpan(line, line.Length, out _, out _, out _));

    [Theory]
    [InlineData("", "accountId,$guid,list")]
    [InlineData("list.", "response")]
    [InlineData("list.response.", "body,headers")]
    [InlineData("list.response.body.", "$")]
    [InlineData("List.", "")]
    public void OffersDocumentSymbolsAndResponseSyntax(string qualifier, string expected)
    {
        var document = ODataDocument.Parse("@accountId = example\n# @name list\nGET /accounts");
        Assert.Equal(expected, string.Join(",", ODataCompletion.GetItems(document, qualifier).Select(item => item.Text)));
    }
}
