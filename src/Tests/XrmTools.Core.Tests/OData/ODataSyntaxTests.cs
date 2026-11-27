namespace XrmTools.Core.Tests.OData;

using System;
using System.Collections.Generic;
using System.Linq;
using XrmTools.OData;
using Xunit;

public class ODataSyntaxTests
{
    [Fact]
    public void ClassifiesRequestWithReadableTargetVersionAndHeaders()
    {
        const string text = "  GET /accounts?$filter=name eq 'A B' HTTP/1.1  \r\n Accept : application/json";
        Assert.Equal(new[]
        {
            (ODataSyntaxKind.HttpMethod, "GET"),
            (ODataSyntaxKind.RequestTarget, "/accounts?$filter=name eq 'A B'"),
            (ODataSyntaxKind.HttpVersion, "HTTP/1.1"),
            (ODataSyntaxKind.HeaderName, "Accept"),
            (ODataSyntaxKind.HeaderValue, "application/json")
        }, Tokens(text));
    }

    [Theory]
    [InlineData("GET")]
    [InlineData("POST")]
    [InlineData("PUT")]
    [InlineData("PATCH")]
    [InlineData("DELETE")]
    [InlineData("HEAD")]
    [InlineData("OPTIONS")]
    [InlineData("GE")]
    [InlineData("P")]
    public void RecognizesSupportedMethodsAndIncompletePrefixes(string method)
        => Assert.Equal(new[] { (ODataSyntaxKind.HttpMethod, method) }, Tokens(method));

    [Theory]
    [InlineData("GET /accounts HTTP/", "HTTP/")]
    [InlineData("GET /accounts HTTP/1.", "HTTP/1.")]
    [InlineData("GET /accounts HTTP/2", "HTTP/2")]
    public void RecognizesIncompleteAndUnsupportedVersionsWithoutValidation(string text, string version)
        => Assert.Equal(new[]
        {
            (ODataSyntaxKind.HttpMethod, "GET"),
            (ODataSyntaxKind.RequestTarget, "/accounts"),
            (ODataSyntaxKind.HttpVersion, version)
        }, Tokens(text));

    [Fact]
    public void CommentsAndBlankLinesBeforeRequestDoNotStartBody()
    {
        const string text = "\n# comment\n// other\n@root = /accounts\n\nGET {{root}}\n# comment\n// other\nAccept";
        Assert.Equal(new[]
        {
            (ODataSyntaxKind.Comment, "# comment"),
            (ODataSyntaxKind.Comment, "// other"),
            (ODataSyntaxKind.VariableDeclaration, "@root"),
            (ODataSyntaxKind.HttpMethod, "GET"),
            (ODataSyntaxKind.VariableReference, "{{root}}"),
            (ODataSyntaxKind.Comment, "# comment"),
            (ODataSyntaxKind.Comment, "// other"),
            (ODataSyntaxKind.HeaderName, "Accept")
        }, Tokens(text));
    }

    [Theory]
    [InlineData("# @name accounts")]
    [InlineData("// @name accounts")]
    [InlineData("#\t@name")]
    [InlineData("// @name=accounts")]
    public void RecognizesNameDirectiveInsideComment(string text)
        => Assert.Equal("@name", Assert.Single(Scan(text), s => s.Kind == ODataSyntaxKind.NameDirective).Text(text));

    [Fact]
    public void DoesNotTreatOtherCommentsAsNameDirectives()
    {
        Assert.DoesNotContain(Scan("# @names request\n// text @name request\n### @name request"), s => s.Kind == ODataSyntaxKind.NameDirective);
        Assert.DoesNotContain(Scan("GET /accounts\n# @name ignored"), s => s.Kind == ODataSyntaxKind.NameDirective);
    }

    [Fact]
    public void BodyOnlyHighlightsReferencesUntilDelimiterEvenInBody()
    {
        const string text = "POST /accounts\n \t\n{\"name\":\"{{name}}\"}\nAccept: body text\n# @name body\n// body\n@body = text\nGET /body\n ### Next\nGET /next\nAccept: json";
        Assert.Equal(new[]
        {
            (ODataSyntaxKind.HttpMethod, "POST"),
            (ODataSyntaxKind.RequestTarget, "/accounts"),
            (ODataSyntaxKind.VariableReference, "{{name}}"),
            (ODataSyntaxKind.Heading, "### Next"),
            (ODataSyntaxKind.HttpMethod, "GET"),
            (ODataSyntaxKind.RequestTarget, "/next"),
            (ODataSyntaxKind.HeaderName, "Accept"),
            (ODataSyntaxKind.HeaderValue, "json")
        }, Tokens(text));
    }

    [Fact]
    public void MalformedRequestStillStartsHeadersAndDeclarationsDoNotOverrideState()
    {
        const string text = "TRACE /accounts\n@notDeclaration = value\nGET /other\nAccept: json\n\nContent-Type: body";
        Assert.Equal(new[] { (ODataSyntaxKind.HeaderName, "Accept"), (ODataSyntaxKind.HeaderValue, "json") }, Tokens(text));
        var document = ODataDocument.Parse(text);
        Assert.Single(document.Requests);
        Assert.Equal("Content-Type: body", document.Requests[0].Body);
    }

    [Theory]
    [InlineData("@")]
    [InlineData("@root")]
    [InlineData("@root =")]
    public void RecognizesIncompleteDeclarations(string text)
        => Assert.Equal(text.Split(' ')[0], Assert.Single(Scan(text)).Text(text));

    [Fact]
    public void ReferencesOverrideEnclosingSyntaxWithoutOverlap()
    {
        const string text = "@root = {{ other }}\n### {{heading}}\n# {{comment}}\nGET /{{root}}/items\n{{header}}: {{value}}\n\n{{unfinished";
        var tokens = Tokens(text);
        Assert.Equal(new[] { "{{ other }}", "{{heading}}", "{{comment}}", "{{root}}", "{{header}}", "{{value}}", "{{unfinished" },
            tokens.Where(t => t.Kind == ODataSyntaxKind.VariableReference).Select(t => t.Text));
        Assert.Contains((ODataSyntaxKind.RequestTarget, "/"), tokens);
        Assert.Contains((ODataSyntaxKind.RequestTarget, "/items"), tokens);
        Assert.DoesNotContain(tokens, t => t.Kind == ODataSyntaxKind.HeaderName);
    }

    [Theory]
    [InlineData("\r\n")]
    [InlineData("\r")]
    [InlineData("\n")]
    public void PreservesOriginalUtf16OffsetsAndLineEndings(string newline)
    {
        string text = "### 😀" + newline + "GET /😀/{{id}}" + newline + "Accept: json";
        var spans = Scan(text);
        Assert.Equal(text.IndexOf("GET", StringComparison.Ordinal), spans.Single(s => s.Kind == ODataSyntaxKind.HttpMethod).Start);
        Assert.Equal(text.IndexOf("{{id}}", StringComparison.Ordinal), spans.Single(s => s.Kind == ODataSyntaxKind.VariableReference).Start);
        Assert.Equal(4, spans.Single(s => s.Kind == ODataSyntaxKind.RequestTarget).Length);
        Assert.Equal(text.IndexOf("Accept", StringComparison.Ordinal), spans.Single(s => s.Kind == ODataSyntaxKind.HeaderName).Start);
    }

    [Fact]
    public void EmptyInputAndJsonBracesDoNotCreateSpans()
    {
        Assert.Empty(Scan(""));
        Assert.Empty(Scan(" \r\n\t"));
        Assert.DoesNotContain(Scan("POST /accounts\n\n{\"name\": {\"value\": 1}}"), s => s.Kind == ODataSyntaxKind.VariableReference);
        Assert.Throws<ArgumentNullException>(() => ODataSyntax.Scan(null));
    }

    [Fact]
    public void HeaderValuesTrimWhitespaceAndSplitAroundReferencesWithoutColoringBodies()
    {
        const string text = "GET /a\r\nX-Test: before {{id}} after  \r\nEmpty:  \r\n\r\nX-Body: uncolored";
        Assert.Equal(new[]
        {
            (ODataSyntaxKind.HttpMethod, "GET"),
            (ODataSyntaxKind.RequestTarget, "/a"),
            (ODataSyntaxKind.HeaderName, "X-Test"),
            (ODataSyntaxKind.HeaderValue, "before "),
            (ODataSyntaxKind.VariableReference, "{{id}}"),
            (ODataSyntaxKind.HeaderValue, " after"),
            (ODataSyntaxKind.HeaderName, "Empty")
        }, Tokens(text));
    }

    private static IReadOnlyList<ODataSyntaxSpan> Scan(string text)
    {
        var spans = ODataSyntax.Scan(text);
        int previousEnd = 0;
        foreach (var span in spans)
        {
            Assert.True(span.Length > 0);
            Assert.InRange(span.Start, previousEnd, text.Length);
            Assert.InRange(span.End, span.Start + 1, text.Length);
            previousEnd = span.End;
        }
        return spans;
    }

    private static (ODataSyntaxKind Kind, string Text)[] Tokens(string text)
        => Scan(text).Select(s => (s.Kind, s.Text(text))).ToArray();
}

internal static class ODataSyntaxTestExtensions
{
    public static string Text(this ODataSyntaxSpan span, string text) => text.Substring(span.Start, span.Length);
}
