namespace XrmTools.Core.Tests.OData;

using System;
using System.Collections.Generic;
using XrmTools.OData;
using Xunit;

public class ODataResponseReferenceTests
{
    private static ODataCapturedResponse Capture() => new()
    {
        Body = "{\"items\":[{\"name\":\"A \\\"quote\\\"\\n{{literal}}\",\"n\":42,\"ok\":true,\"empty\":null}],\"@odata.etag\":\"W/\\\"123\\\"\"}",
        Headers = new Dictionary<string, string> { ["ETag"] = "W/\"123\"" },
        Principal = "user"
    };

    [Fact]
    public void NamesRequestsWithoutChangingHeadingsAndEnforcesUniqueness()
    {
        var doc = ODataDocument.Parse("### Heading\n# @name First\nGET /a\n### Next\n// @name first\nGET /b");
        Assert.Empty(doc.Errors);
        Assert.Equal("Heading", doc.Requests[0].Name);
        Assert.Equal("First", doc.Requests[0].RequestName);
        Assert.Equal("first", doc.Requests[1].RequestName);
        var beforeHeading = ODataDocument.Parse("# @name x\n### Title\nGET /a");
        Assert.Empty(beforeHeading.Errors);
        Assert.Equal("x", beforeHeading.Requests[0].RequestName);
        var duplicate = ODataDocument.Parse("# @name x\nGET /a\n###\n// @name x\nGET /b");
        Assert.Contains("duplicate", Assert.Throws<FormatException>(() => duplicate.Resolve(duplicate.Requests[0])).Message);
    }

    [Fact]
    public void CommentedNamedRequestsDoNotBlockOrNameOtherRequests()
    {
        var doc = ODataDocument.Parse("GET /first\n### disabled\n# @name x\n#GET /disabled\n### next\n# @name x\nGET /next\n### disabled at end\n// @name y\n// GET /disabled");
        Assert.Empty(doc.Errors);
        Assert.Equal(2, doc.Requests.Count);
        Assert.Equal("/first", doc.Resolve(doc.Requests[0]).Target);
        Assert.Equal("x", doc.Requests[1].RequestName);
        Assert.Equal("/next", doc.Resolve(doc.Requests[1]).Target);
    }

    [Theory]
    [InlineData("# @name 1bad\nGET /a")]
    [InlineData("// @name\nGET /a")]
    [InlineData("# @name=bad\nGET /a")]
    [InlineData("# @name x\n# @name y\nGET /a")]
    public void RejectsMalformedNames(string source)
    {
        var doc = ODataDocument.Parse(source);
        Assert.NotEmpty(doc.Errors);
    }

    [Fact]
    public void SubstitutesTypedJsonAndEscapesStringsWithoutExpandingCapturedContent()
    {
        var doc = ODataDocument.Parse("@alias = {{first.response.body.$.items[0].name}}\nPOST /a\n\n{\"string\":\"prefix {{alias}}\",\"number\":{{first.response.body.$.items[0].n}},\"bool\":{{first.response.body.$.items[0].ok}},\"nil\":{{first.response.body.$.items[0].empty}},\"object\":{{first.response.body.$.items[0]}},\"etag\":{{first.response.body.$['@odata.etag']}}}");
        var capture = Capture();
        int calls = 0;
        var result = doc.Resolve(doc.Requests[0], name => name == "first" ? capture : null, used => { Assert.Same(capture, used); calls++; });
        var json = Newtonsoft.Json.Linq.JObject.Parse(result.Body);
        Assert.Equal("prefix A \"quote\"\n{{literal}}", (string)json["string"]);
        Assert.Equal(42, (int)json["number"]);
        Assert.True((bool)json["bool"]);
        Assert.Equal(Newtonsoft.Json.Linq.JTokenType.Null, json["nil"].Type);
        Assert.Equal(42, (int)json["object"]["n"]);
        Assert.Equal("W/\"123\"", (string)json["etag"]);
        Assert.Equal(6, calls);
    }

    [Fact]
    public void ResolvesDataverseAccountAndDottedKeyExamples()
    {
        var created = new ODataCapturedResponse { Body = "{\"accountid\":\"one\",\"@odata.etag\":\"v1\"}", Headers = new Dictionary<string, string> { ["OData-EntityId"] = "entity", ["X.Example"] = "dot" } };
        var found = new ODataCapturedResponse { Body = "{\"value\":[{\"accountid\":\"two\"}]}" };
        var doc = ODataDocument.Parse("GET /accounts({{createAccount.response.body.$.accountid}})/{{findAccounts.response.body.$.value[0].accountid}}\nEntity: {{createAccount.response.headers.odata-entityid}}\nTag: {{createAccount.response.body.$['@odata.etag']}}\nDot: {{createAccount.response.headers.X.Example}}");
        var result = doc.Resolve(doc.Requests[0], name => name == "createAccount" ? created : found, _ => { });
        Assert.Equal("/accounts(one)/two", result.Target);
        Assert.Equal("entity", result.Headers[0].Value);
        Assert.Equal("v1", result.Headers[1].Value);
        Assert.Equal("dot", result.Headers[2].Value);
    }

    [Fact]
    public void ResolvesHeadersAndUrlScalarsCaseInsensitively()
    {
        var doc = ODataDocument.Parse("GET /{{r.response.body.$.items[0].n}}\nIf-Match: {{r.response.headers.etag}}\nOther: {{r.response.headers['ETAG']}}");
        var result = doc.Resolve(doc.Requests[0], _ => Capture(), _ => { });
        Assert.Equal("/42", result.Target);
        Assert.Equal("W/\"123\"", result.Headers[0].Value);
        Assert.Equal(result.Headers[0].Value, result.Headers[1].Value);
    }

    [Theory]
    [InlineData("{\"{{r.response.body.$.items[0].name}}\":1}")]
    [InlineData("{\"a\":1, {{r.response.body.$.items[0]}}}")]
    [InlineData("{\"x\":prefix{{r.response.body.$.items[0].n}}}")]
    [InlineData("{\"x\":\"{{r.response.body.$.items}}\"}")]
    [InlineData("{\"x\":{{r.response.body.$.missing}}}")]
    [InlineData("{\"x\":{{r.response.body.$.items[-1]}}}")]
    [InlineData("{\"x\":{{r.response.body.$.items[*].missing}}}")]
    [InlineData("{\"x\":{{r.response.body.$.items[*][*]}}}")]
    [InlineData("{\"x\":{{r.response.body.$[*]}}}")]
    [InlineData("{\"x\":{{r.response.body.items}}}")]
    [InlineData("{\"x\":{{r.response.status}}}")]
    public void RejectsUnsupportedOrMissingReferences(string body)
    {
        var doc = ODataDocument.Parse("POST /a\n\n" + body);
        Assert.Throws<FormatException>(() => doc.Resolve(doc.Requests[0], _ => Capture(), _ => { }));
    }

    [Theory]
    [InlineData("[]", "[]")]
    [InlineData("[{\"id\":7}]", "[7]")]
    [InlineData("[{\"id\":true},{\"id\":\"7\"},{\"id\":null}]", "[true,\"7\",null]")]
    public void ProjectsArrayPropertiesInOrderAndPreservesJsonTypes(string items, string expected)
    {
        var doc = ODataDocument.Parse("POST /a\n\n{\"ids\":{{r.response.body.$.value[*].id}}}");
        var capture = new ODataCapturedResponse { Body = "{\"value\":" + items + "}" };
        Assert.Equal("{\"ids\":" + expected + "}", doc.Resolve(doc.Requests[0], _ => capture, _ => { }).Body);
    }

    [Theory]
    [InlineData("GET /{{r.response.body.$[*].id}}")]
    [InlineData("GET /a\nX-Ids: {{r.response.body.$[*].id}}")]
    [InlineData("POST /a\n\n{\"ids\":\"{{r.response.body.$[*].id}}\"}")]
    public void RejectsProjectionsInScalarPositions(string source)
    {
        var doc = ODataDocument.Parse(source);
        var capture = new ODataCapturedResponse { Body = "[{\"id\":7}]" };
        Assert.Contains("scalar is required", Assert.Throws<FormatException>(() => doc.Resolve(doc.Requests[0], _ => capture, _ => { })).Message);
    }

    [Fact]
    public void EncodesProjectedArrayAsOneQueryAliasValue()
    {
        var doc = ODataDocument.Parse("GET /contacts?@ids={{r.response.body.$[*].id}}&$top=2");
        var capture = new ODataCapturedResponse { Body = "[{\"id\":\"a&$top=9#\"},{\"id\":\"b\"}]" };
        var request = doc.Resolve(doc.Requests[0], _ => capture, _ => { });
        using var message = ODataRequestBuilder.Build(request, new Uri("https://example.test/api/data/v9.2/"), "token");
        Assert.Equal("?@ids=%5B%22a%26%24top%3D9%23%22%2C%22b%22%5D&$top=2", message.RequestUri.Query);
    }

    [Theory]
    [InlineData("GET /contacts?$filter=name eq 'a&@ids={{r.response.body.$[*].id}}&b'")]
    [InlineData("GET /contacts?@ids={{r.response.body.$[*].id}}suffix")]
    [InlineData("GET /contacts?ids={{r.response.body.$[*].id}}")]
    [InlineData("GET /contacts?@ids={{r.response.body.$[*]}}")]
    public void RejectsArraysOutsideWholeScalarArrayAliases(string source)
    {
        var doc = ODataDocument.Parse(source);
        var capture = new ODataCapturedResponse { Body = "[{\"id\":\"a\"}]" };
        Assert.Throws<FormatException>(() => doc.Resolve(doc.Requests[0], _ => capture, _ => { }));
    }

    [Fact]
    public void RejectsMissingCaptureAndCredentialHeaders()
    {
        var doc = ODataDocument.Parse("GET /{{r.response.headers.ETag}}");
        Assert.Contains("captured response", Assert.Throws<FormatException>(() => doc.Resolve(doc.Requests[0])).Message);
        Assert.Throws<ArgumentException>(() => new ODataCapturedResponse { Headers = new Dictionary<string, string> { ["authorization"] = "secret" } });
        Assert.Throws<FormatException>(() => doc.Resolve(doc.Requests[0], _ => new ODataCapturedResponse(), _ => { }));
    }

    [Fact]
    public void RejectsInvalidJsonAndNonScalarUrlValues()
    {
        var doc = ODataDocument.Parse("GET /{{r.response.body.$.items}}");
        Assert.Throws<FormatException>(() => doc.Resolve(doc.Requests[0], _ => Capture(), _ => { }));
        Assert.Throws<FormatException>(() => doc.Resolve(doc.Requests[0], _ => new ODataCapturedResponse { Body = "not json" }, _ => { }));
    }

    [Fact]
    public void FingerprintsOnlyOwnTextAndReferencedRecursiveVariables()
    {
        ODataDocument Parse(string a, string unused, string heading, string other) => ODataDocument.Parse($"@a = {a}\n@b = {{{{a}}}}\n@unused = {unused}\n### {heading}\n# @name r\nGET /{{{{b}}}}/{{{{other.response.body.$.id}}}}\n### Other\nGET /{other}");
        var first = Parse("one", "x", "old", "a");
        var moved = Parse("one", "y", "new", "b");
        var changed = Parse("two", "x", "old", "a");
        Assert.Equal(first.Fingerprint(first.Requests[0]), moved.Fingerprint(moved.Requests[0]));
        Assert.NotEqual(first.Fingerprint(first.Requests[0]), changed.Fingerprint(changed.Requests[0]));
        first.Requests[0].Headers.Add(new("Accept", "application/json"));
        Assert.NotEqual(first.Fingerprint(first.Requests[0]), moved.Fingerprint(moved.Requests[0]));
    }
}
