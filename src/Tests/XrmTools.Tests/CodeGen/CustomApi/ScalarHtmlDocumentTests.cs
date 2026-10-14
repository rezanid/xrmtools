#nullable enable
namespace XrmTools.Tests.CodeGen.CustomApi;

using Newtonsoft.Json;
using System;
using System.Text.RegularExpressions;
using Xunit;
using XrmTools.CodeGen.CustomApi;

public sealed class ScalarHtmlDocumentTests
{
    [Theory]
    [InlineData("custom-apis.openapi.json")]
    [InlineData("custom-apis.openapi.yaml")]
    [InlineData("API & café #1.openapi.json")]
    [InlineData("API 'quoted' & names.openapi.json")]
    public void CompanionLoadsOriginalSpecificationUsingSafeRelativeUrl(string filename)
    {
        var html = ScalarHtmlDocument.Create(new GeneratedClient(filename, "{}"), OpenApiHtmlMode.Linked);
        Assert.EndsWith(".openapi.html", html.FileName);
        var match = Regex.Match(html.Content, @"url: (.+),");
        Assert.True(match.Success);
        var url = JsonConvert.DeserializeObject<string>(match.Groups[1].Value)!;
        Assert.StartsWith("./", url);
        Assert.Equal(filename, Uri.UnescapeDataString(url.Substring(2)));
        Assert.Contains("createApiReference('#app'", html.Content);
        Assert.Contains("https://cdn.jsdelivr.net/npm/@scalar/api-reference/esm.js", html.Content);
        Assert.DoesNotContain("proxy.scalar.com", html.Content);
        Assert.DoesNotContain("'<script>", html.Content);
    }

    [Theory]
    [InlineData("json", "{\"openapi\":\"3.0.4\",\"info\":{\"description\":\"</script><script>alert(1)</script> & café\"}}")]
    [InlineData("yaml", "openapi: 3.0.4\ninfo:\n  description: '</script> & café'\n")]
    public void EmbeddedModeDefaultsToSafelyEncodedContentWithoutFileFetch(string extension, string content)
    {
        var html = ScalarHtmlDocument.Create(new GeneratedClient("custom-apis.openapi." + extension, content));
        var match = Regex.Match(html.Content, @"content: (.+),");
        Assert.True(match.Success);
        Assert.Equal(content, JsonConvert.DeserializeObject<string>(match.Groups[1].Value));
        Assert.DoesNotContain("url:", html.Content);
        Assert.DoesNotContain("</script><script>alert", html.Content);
        Assert.Contains("Embedded specification snapshot", html.Content);
    }

    [Fact]
    public void UnsupportedModesAreRejected()
        => Assert.Throws<ArgumentOutOfRangeException>(() => ScalarHtmlDocument.Create(new GeneratedClient("api.json", "{}"), (OpenApiHtmlMode)999));

    [Theory]
    [InlineData("API '<script>.openapi.json")]
    [InlineData("")]
    [InlineData("../openapi.json")]
    [InlineData("C:\\specs\\openapi.json")]
    public void RejectsMissingFilenamesAndDirectoryPaths(string filename)
        => Assert.Throws<ArgumentException>(() => ScalarHtmlDocument.Create(new GeneratedClient(filename, "{}"), OpenApiHtmlMode.Linked));
}
