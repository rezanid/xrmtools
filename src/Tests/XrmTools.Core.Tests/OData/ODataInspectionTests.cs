namespace XrmTools.Core.Tests.OData;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.OData;
using Xunit;

public class ODataInspectionTests
{
    private static Task<ODataInspection> Inspect(ODataDocument document, string expression, ODataCapturedResponse capture = null)
        => ODataInspector.InspectAsync(document, expression, "Development", _ => capture, _ => { }, CancellationToken.None);

    [Fact]
    public async Task DocumentDeclarationsOverrideProfilesAndResolveRecursiveEffectiveValues()
    {
        var document = ODataDocument.Parse("@id = document\n@alias = {{id}}/{{other}}\nGET /{{alias}}");
        document.SetEnvironmentVariables(new Dictionary<string, string> { ["id"] = "environment", ["other"] = "profile" });
        var id = await Inspect(document, "id");
        Assert.Contains("Document variable", id.Source);
        Assert.Contains("overrides environment variable in Development", id.Source);
        Assert.Equal("document", id.Preview);
        Assert.Equal("document/profile", (await Inspect(document, "alias")).Preview);
        var other = await Inspect(document, "other");
        Assert.Equal("Environment variable other (Development)", other.Source);
        Assert.Equal("profile", other.Preview);
    }

    [Fact]
    public async Task DynamicAndRecursiveDynamicDependenciesRemainUnevaluated()
    {
        var document = ODataDocument.Parse("@generated = prefix-{{$guid}}\n@alias = {{generated}}\nGET /{{alias}}");
        var direct = await Inspect(document, "$guid");
        Assert.Contains("when the request is sent", direct.Preview);
        Assert.Contains("does not evaluate", direct.Preview);
        var first = await Inspect(document, "alias");
        var second = await Inspect(document, "alias");
        Assert.Equal(first, second);
        Assert.Contains("prefix-{{$guid}}", first.Preview);
        Assert.Contains("Depends on $guid", first.Description);
        Assert.True(first.IsValue);
        Assert.False(direct.IsValue);
        Assert.Equal("prefix-{{$guid}}", document.Variables["generated"]);
    }

    [Fact]
    public async Task DynamicDependencyDoesNotTouchResponseCaptures()
    {
        var document = ODataDocument.Parse("@mixed = {{$guid}}/{{r.response.body.$}}\n# @name r\nGET /a");
        var result = await ODataInspector.InspectAsync(document, "mixed", null,
            _ => throw new InvalidOperationException("Must not resolve captures for an unevaluated dynamic dependency."),
            _ => throw new InvalidOperationException(), CancellationToken.None);
        Assert.Contains("Depends on $guid", result.Description);
    }

    [Fact]
    public async Task MissingCaptureAndUnresolvableSelectorHaveDifferentActionableMessages()
    {
        var document = ODataDocument.Parse("# @name r\nGET /a");
        var missing = await Inspect(document, "r.response.body.$.id");
        Assert.Contains("No valid captured response", missing.Preview);
        Assert.Contains("completed 2xx", missing.Preview);
        Assert.False(missing.IsValue);
        var capture = new ODataCapturedResponse { Body = "{\"id\":1}" };
        var unresolved = await Inspect(document, "r.response.body.$.missing", capture);
        Assert.Contains("capture is available", unresolved.Preview);
        Assert.Contains("missing or has the wrong type", unresolved.Preview);
        Assert.DoesNotContain("No valid captured", unresolved.Preview);
        Assert.False(unresolved.IsValue);
        var resolved = await Inspect(document, "r.response.body.$.id", capture);
        Assert.Equal("1", resolved.Preview);
        Assert.True(resolved.IsValue);
    }

    [Fact]
    public async Task ResponseAliasesUseStoredCapturesAndCaseInsensitiveHeadersWithoutExpandingTheirContent()
    {
        var document = ODataDocument.Parse("@alias = {{r.response.body.$['@odata.etag']}}\n# @name r\nGET /a");
        var capture = new ODataCapturedResponse
        {
            Body = "{\"@odata.etag\":\"{{$guid}}\"}",
            Headers = new Dictionary<string, string> { ["ETag"] = "header" }
        };
        Assert.Equal("{{$guid}}", (await Inspect(document, "alias", capture)).Preview);
        Assert.Equal("header", (await Inspect(document, "r.response.headers.etag", capture)).Preview);
        var redacted = await Inspect(document, "r.response.headers.Authorization", capture);
        Assert.Contains("redacted", redacted.Preview);
        Assert.False(redacted.IsValue);
        Assert.Contains("capture is available", (await Inspect(document, "r.response.headers.Missing", capture)).Preview);
    }

    [Theory]
    [InlineData("string")]
    [InlineData("array")]
    [InlineData("object")]
    public async Task ResponsePreviewsAreBoundedAndClearlyTruncated(string kind)
    {
        string body = kind switch
        {
            "string" => "\"" + new string('x', 12000) + "\"",
            "array" => "[" + string.Join(",", Enumerable.Repeat("123", 10000)) + "]",
            _ => "{\"long\":\"" + new string('x', 12000) + "\"}"
        };
        var result = await Inspect(ODataDocument.Parse("# @name r\nGET /a"), "r.response.body.$", new ODataCapturedResponse { Body = body });
        AssertTruncated(result.Preview, ODataInspector.ResponseTruncationNotice);
        if (kind != "string") Assert.Contains(Environment.NewLine, result.Preview);
    }

    [Fact]
    public async Task LiteralPreviewsPreserveWhitespaceAndBoundLargeValues()
    {
        var document = ODataDocument.Parse("@long = " + new string('x', 100000));
        AssertTruncated((await Inspect(document, "long")).Preview, ODataInspector.TruncationNotice);
        Assert.Equal("a\n\r\t?", ODataInspector.Bound("a\n\r\t\0"));
        var dynamic = ODataDocument.Parse("@long = " + new string('x', 2000) + "{{$guid}}");
        var result = await Inspect(dynamic, "long");
        Assert.Equal(new string('x', 2000) + "{{$guid}}", result.Preview);
        Assert.Contains("Depends on $guid", result.Description);
        var oversized = ODataDocument.Parse("@large = " + new string('x', 100000) + "{{$guid}}\n@alias = {{large}}");
        var oversizedResult = await Inspect(oversized, "alias");
        Assert.True(oversizedResult.Preview.Length <= 2048);
        Assert.Contains("Depends on $guid", oversizedResult.Description);
        Assert.False(oversizedResult.IsValue);
        Assert.Contains("Expansion unavailable", oversizedResult.Preview);
    }

    [Theory]
    [InlineData(512)]
    [InlineData(2048)]
    [InlineData(2049)]
    public void LiteralPreviewUsesTheFullNativeCharacterBudget(int length)
    {
        string value = new('x', length);
        Assert.Equal(length <= 2048 ? value : new string('x', 2048) + Environment.NewLine + ODataInspector.TruncationNotice,
            ODataInspector.Bound(value));
    }

    [Theory]
    [InlineData("\n")]
    [InlineData("\r\n")]
    [InlineData("\r")]
    public void LiteralPreviewPreservesTwentyLinesAndTruncatesBeforeTheTwentyFirst(string newline)
    {
        string twentyLines = string.Join(newline, Enumerable.Repeat("line", 20));
        Assert.Equal(twentyLines, ODataInspector.Bound(twentyLines));
        Assert.Equal(twentyLines + Environment.NewLine + ODataInspector.TruncationNotice,
            ODataInspector.Bound(twentyLines + newline + "hidden"));
    }

    [Theory]
    [InlineData(2046, true)]
    [InlineData(2047, false)]
    public void LiteralTruncationNeverSplitsASurrogatePair(int prefixLength, bool includesPair)
    {
        string prefix = new('x', prefixLength);
        string expected = prefix + (includesPair ? "\uD83D\uDE00" : "") + Environment.NewLine + ODataInspector.TruncationNotice;
        Assert.Equal(expected, ODataInspector.Bound(prefix + "\uD83D\uDE00tail"));
    }

    [Fact]
    public void CharacterLimitNeverSplitsACarriageReturnLineFeedPair()
    {
        string prefix = new('x', 2047);
        Assert.Equal(prefix + Environment.NewLine + ODataInspector.TruncationNotice,
            ODataInspector.Bound(prefix + "\r\ntail"));
    }

    [Fact]
    public async Task ProfileHeaderAndResponseAliasPreviewsPreserveLineBreaks()
    {
        const string value = "first\r\nsecond\n\tthird";
        var document = ODataDocument.Parse("@alias = {{r.response.body.$.text}}\n# @name r\nGET /a");
        document.SetEnvironmentVariables(new Dictionary<string, string> { ["multiline"] = value });
        var capture = new ODataCapturedResponse
        {
            Body = "{\"text\":\"first\\r\\nsecond\\n\\tthird\"}",
            Headers = new Dictionary<string, string> { ["X-Test"] = value }
        };
        Assert.Equal(value, (await Inspect(document, "multiline")).Preview);
        Assert.Equal(value, (await Inspect(document, "alias", capture)).Preview);
        Assert.Equal(value, (await Inspect(document, "r.response.headers.X-Test", capture)).Preview);
    }

    [Theory]
    [InlineData(2045, true)]
    [InlineData(2046, false)]
    public async Task StreamingStringTruncationNeverSplitsASurrogatePair(int prefixLength, bool includesPair)
    {
        string prefix = "\"" + new string('x', prefixLength);
        var result = await Inspect(ODataDocument.Parse("# @name r\nGET /a"), "r.response.body.$",
            new ODataCapturedResponse { Body = prefix + "\uD83D\uDE00tail\"" });
        Assert.Equal(prefix + (includesPair ? "\uD83D\uDE00" : "") + Environment.NewLine + ODataInspector.ResponseTruncationNotice, result.Preview);
    }

    [Theory]
    [InlineData(2048)]
    [InlineData(2049)]
    public async Task StreamingStringUsesTheFullNativeCharacterBudget(int length)
    {
        string body = "\"" + new string('x', length - 2) + "\"";
        var result = await Inspect(ODataDocument.Parse("# @name r\nGET /a"), "r.response.body.$", new ODataCapturedResponse { Body = body });
        Assert.Equal(length == 2048 ? body : body.Substring(0, 2048) + Environment.NewLine + ODataInspector.ResponseTruncationNotice, result.Preview);
    }

    [Fact]
    public async Task StreamingArrayStopsAtTwentyLinesWithoutExhaustingTheCharacterBudget()
    {
        var result = await Inspect(ODataDocument.Parse("# @name r\nGET /a"), "r.response.body.$",
            new ODataCapturedResponse { Body = "[" + string.Join(",", Enumerable.Range(0, 100)) + "]" });
        string expected = "[" + Environment.NewLine + string.Join(Environment.NewLine,
            Enumerable.Range(0, 19).Select(i => "  " + i + ","));
        Assert.Equal(expected + Environment.NewLine + ODataInspector.ResponseTruncationNotice, result.Preview);
        AssertTruncated(result.Preview, ODataInspector.ResponseTruncationNotice);
    }

    private static void AssertTruncated(string preview, string notice)
    {
        Assert.EndsWith(Environment.NewLine + notice, preview);
        string content = preview.Substring(0, preview.Length - Environment.NewLine.Length - notice.Length);
        Assert.True(content.Length <= 2048);
        Assert.True(content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None).Length <= 20);
        Assert.False(content.Length > 0 && char.IsHighSurrogate(content[content.Length - 1]));
    }

    [Theory]
    [InlineData("[]", "[]")]
    [InlineData("[{\"id\":7},{\"id\":null}]", "[\n  7,\n  null\n]")]
    public async Task ProjectionPreviewsPreserveTypesAndEmptyArrays(string body, string expected)
    {
        var result = await Inspect(ODataDocument.Parse("# @name r\nGET /a"), "r.response.body.$[*].id", new ODataCapturedResponse { Body = body });
        Assert.Equal(expected.Replace("\n", Environment.NewLine), result.Preview);
    }

    [Fact]
    public async Task WrongTypesInvalidJsonAndCyclesAreReportedWithoutCaptureFallbacks()
    {
        var document = ODataDocument.Parse("@a = {{b}}\n@b = {{a}}\n# @name r\nGET /a");
        Assert.Contains("Cyclic", (await Inspect(document, "a")).Preview);
        Assert.Contains("wrong type", (await Inspect(document, "r.response.body.$.id", new ODataCapturedResponse { Body = "[]" })).Preview);
        Assert.Contains("not valid JSON", (await Inspect(document, "r.response.body.$", new ODataCapturedResponse { Body = "not json" })).Preview);
    }

    [Fact]
    public async Task StoredBodiesSelectEarlyValuesWithoutReadingLargeUnrelatedTail()
    {
        string path = Path.GetTempFileName();
        try
        {
            File.WriteAllText(path, "{\"id\":42,\"tail\":\"" + new string('x', 1024 * 1024) + "\"}", Encoding.UTF8);
            using var capture = new ODataCapturedResponse { Body = "not the stored body", StoredBody = new ODataStoredBody(path, Encoding.UTF8) };
            var result = await Inspect(ODataDocument.Parse("# @name r\nGET /a"), "r.response.body.$.id", capture);
            Assert.Equal("42", result.Preview);
            var tooFar = await Inspect(ODataDocument.Parse("# @name r\nGET /a"), "r.response.body.$.missing", capture);
            Assert.Contains("64 Ki character", tooFar.Preview);
            Assert.Contains("capture is available", tooFar.Preview);
            Assert.False(tooFar.IsValue);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public async Task CancellationIsPropagatedDuringCaptureResolution()
    {
        using var cancellation = new CancellationTokenSource();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => ODataInspector.InspectAsync(
            ODataDocument.Parse("# @name r\nGET /a"), "r.response.body.$", null,
            _ => { cancellation.Cancel(); return new ODataCapturedResponse { Body = "{}" }; },
            _ => { }, cancellation.Token));
    }

    [Fact]
    public void SymbolLookupCoversEntirePlaceholderAndIgnoresComments()
    {
        const string source = "@id = one\n# {{id}}\nGET /{{ id }}/{{r.response.body.$.value[0]}}";
        var reference = ODataNavigation.FindReference(source, 2, 9);
        Assert.True(reference.HasValue);
        Assert.Equal(new ODataReference(5, 8, "id"), reference.Value);
        Assert.Equal("r.response.body.$.value[0]", ODataNavigation.FindReference(source, 2, 30).Value.Expression);
        Assert.Null(ODataNavigation.FindReference(source, 1, 5));
        Assert.Null(ODataNavigation.FindReference(source, 2, 13));
    }
}
