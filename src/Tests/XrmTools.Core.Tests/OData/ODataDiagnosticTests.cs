namespace XrmTools.Core.Tests.OData;

using System.Linq;
using XrmTools.OData;
using Xunit;

public class ODataDiagnosticTests
{
    [Fact]
    public void ReportsSecondNameInTheSameRequest()
    {
        const string text = "# @name first\r\n  # @name second\r\nGET /contacts";
        var diagnostic = Assert.Single(ODataDiagnostics.Analyze(text));
        Assert.Equal("second", text.Substring(diagnostic.Start, diagnostic.Length));
    }

    [Fact]
    public void ReportsExactSymbolsAndValidatesUnsentRequests()
    {
        const string text = "@accountId = demo\r\n# {{ignored}}\r\n  # @name contacts\r\nGET /contacts\r\n###\r\n  // @name contacts\r\nGET /{{ accountd }}/{{contatcs.response.body.$.value}}\r\nX-Test: {{contacts.response.headers.ETag}}\r\nX-Invalid: {{contacts.response.body.$.value[-1]}}";
        var diagnostics = ODataDiagnostics.Analyze(text);
        Assert.Collection(diagnostics,
            item => Check(item, "contacts", null),
            item => Check(item, "accountd", "accountId"),
            item => Check(item, "contatcs", "contacts"),
            item => Check(item, "contacts.response.body.$.value[-1]", null));
        Assert.Equal(text.IndexOf("// @name contacts", System.StringComparison.Ordinal) + "// @name ".Length, diagnostics[0].Start);

        void Check(ODataDiagnostic diagnostic, string expected, string? replacement)
        {
            Assert.Equal(expected, text.Substring(diagnostic.Start, diagnostic.Length));
            Assert.Equal(replacement, diagnostic.Replacement);
        }
    }

    [Fact]
    public void LeavesAmbiguousSpellingAndIncompleteReferencesAlone()
    {
        const string text = "@date = one\n@data = two\nGET /{{dat}}/{{incomplete";
        var diagnostic = Assert.Single(ODataDiagnostics.Analyze(text));
        Assert.Equal("dat", text.Substring(diagnostic.Start, diagnostic.Length));
        Assert.Null(diagnostic.Replacement);
    }

    [Fact]
    public void ValidatesEnvironmentVariablesAndReportsConfigurationErrorsWithoutValues()
    {
        const string text = "GET /accounts({{account1}})/{{missing}}";
        var variables = new System.Collections.Generic.Dictionary<string, string> { ["account1"] = "sensitive-value" };
        var diagnostics = ODataDiagnostics.Analyze(text, variables, "The file contains malformed JSON.");

        Assert.Equal(2, diagnostics.Count);
        Assert.Contains("Invalid odata-client.env.json", diagnostics[0].Message);
        Assert.DoesNotContain("sensitive-value", string.Join(" ", diagnostics.Select(item => item.Message)));
        Assert.Equal("missing", text.Substring(diagnostics[1].Start, diagnostics[1].Length));
    }
}
