namespace XrmTools.Core.Tests.OData;
using System;
using System.Threading.Tasks;
using XrmTools.OData;
using Xunit;

public class ODataDocumentTests
{
    [Fact]
    public void ParsesMultipleRequestsAndPreservesJsonBody()
    {
        var document = ODataDocument.Parse("@count = 5\r\n### Find\r\nGET /accounts?$top={{count}}\r\nAccept: application/json\r\n\r\n### Create\r\nPOST /accounts\r\nContent-Type: application/json\r\n\r\n{\r\n  \"name\": \"hello\"\r\n}\r\n");
        Assert.Equal(2, document.Requests.Count);
        Assert.Equal(2, document.Requests[0].Line);
        Assert.Equal("Find", document.Requests[0].Name);
        Assert.Equal("/accounts?$top=5", document.Resolve(document.Requests[0]).Target);
        Assert.Equal("{\n  \"name\": \"hello\"\n}", document.Requests[1].Body);
    }

    [Theory]
    [InlineData("GET /{{missing}}")]
    [InlineData("@a = {{b}}\n@b = {{a}}\nGET /{{a}}")]
    [InlineData("@bad name = value\nGET /WhoAmI")]
    [InlineData("GET /WhoAmI HTTP/2")]
    [InlineData("GET /WhoAmI\nGET /accounts")]
    [InlineData("TRACE /WhoAmI")]
    public void RejectsInvalidOrUnsupportedRequests(string text)
    {
        var document = ODataDocument.Parse(text);
        Assert.Throws<FormatException>(() => document.Resolve(document.Requests[0]));
    }

    [Fact]
    public void ExpandsNestedVariablesWithoutTreatingJsonBracesAsVariables()
    {
        var document = ODataDocument.Parse("@name = test\n@value = {{name}}\nPOST /accounts\n\n{\"name\":\"{{value}}\"}");
        Assert.Equal("{\"name\":\"test\"}", document.Resolve(document.Requests[0]).Body);
    }

    [Theory]
    [InlineData("/WhoAmI")]
    [InlineData("WhoAmI")]
    [InlineData("/api/data/v9.2/WhoAmI")]
    [InlineData("https://contoso.crm.dynamics.com/api/data/v9.2/WhoAmI")]
    public void ResolvesOnlyWithinSelectedWebApiRoot(string target)
    {
        using var message = ODataRequestBuilder.Build(new ODataRequest { Target = target }, Root, "test-token");
        Assert.Equal(new Uri(Root, "WhoAmI"), message.RequestUri);
        Assert.Equal("test-token", message.Headers.Authorization.Parameter);
    }

    [Theory]
    [InlineData("https://other.crm.dynamics.com/api/data/v9.2/WhoAmI")]
    [InlineData("http://contoso.crm.dynamics.com/api/data/v9.2/WhoAmI")]
    [InlineData("https://user@contoso.crm.dynamics.com/api/data/v9.2/WhoAmI")]
    [InlineData("../WhoAmI")]
    [InlineData("//other.crm.dynamics.com/WhoAmI")]
    [InlineData("/WhoAmI#fragment")]
    [InlineData("/api/other")]
    public void DoesNotLeakCredentialsOutsideWebApi(string target)
        => Assert.ThrowsAny<Exception>(() => ODataRequestBuilder.Build(new ODataRequest { Target = target }, Root, "secret"));

    [Theory]
    [InlineData("Authorization")]
    [InlineData("Host")]
    [InlineData("Content-Length")]
    [InlineData("Proxy-Authorization")]
    public void RejectsManagedHeaderOverrides(string name)
    {
        var request = new ODataRequest { Target = "/WhoAmI" };
        request.Headers.Add(new(name, "bad"));
        Assert.Throws<FormatException>(() => ODataRequestBuilder.Build(request, Root, "secret"));
    }

    [Fact]
    public async Task BuildsJsonAndCustomHeaders()
    {
        var document = ODataDocument.Parse("POST /accounts\nContent-Type: application/json; charset=utf-8\nPrefer: return=representation\n\n{\"name\":\"test\"}");
        using var message = ODataRequestBuilder.Build(document.Resolve(document.Requests[0]), Root, "token");
        Assert.Equal("application/json", message.Content.Headers.ContentType.MediaType);
        Assert.Equal("{\"name\":\"test\"}", await message.Content.ReadAsStringAsync());
        Assert.True(message.Headers.Contains("Prefer"));
    }

    private static readonly Uri Root = new("https://contoso.crm.dynamics.com/api/data/v9.2/");

    [Fact]
    public void CaretOnSectionHeadingSelectsThatRequestNotThePreviousOne()
    {
        var document = ODataDocument.Parse("GET /WhoAmI\n### New section\nGET /accounts\n### Empty section\n# Nothing here");
        Assert.Same(document.Requests[1], document.FindRequestAtLine(1));
        Assert.Null(document.FindRequestAtLine(3));
    }
}
