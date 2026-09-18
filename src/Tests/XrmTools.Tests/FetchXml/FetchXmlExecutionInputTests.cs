namespace XrmTools.Tests.FetchXml;

using System;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.FetchXml;
using Xunit;

public class FetchXmlExecutionInputTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("<other/>")]
    [InlineData("<fetch/>")]
    [InlineData("<fetch><entity/></fetch>")]
    public async Task RejectsMissingQueryOrEntity(string text)
    {
        await Assert.ThrowsAsync<FormatException>(() => FetchXmlExecutionInput.ParseAsync(text, CancellationToken.None));
    }

    [Fact]
    public async Task CapturesEntityAndXmlFromTheSameInput()
    {
        var first = await FetchXmlExecutionInput.ParseAsync("<fetch><entity name='account'/></fetch>", CancellationToken.None);
        var second = await FetchXmlExecutionInput.ParseAsync("<fetch><entity name='contact'/></fetch>", CancellationToken.None);
        Assert.Equal("account", first.EntityName);
        Assert.Equal("contact", second.EntityName);
        Assert.Contains("contact", second.Xml);
        Assert.DoesNotContain("account", second.Xml);
    }

    [Fact]
    public async Task ResolvesEntityParameterDefault()
    {
        var result = await FetchXmlExecutionInput.ParseAsync("<fetch><entity name='{{entity:account}}'/></fetch>", CancellationToken.None);
        Assert.Equal("account", result.EntityName);
        Assert.DoesNotContain("{{", result.Xml);
    }

    [Fact]
    public async Task HonorsCancellationBeforeParsing()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            FetchXmlExecutionInput.ParseAsync("<fetch><entity name='account'/></fetch>", cts.Token));
    }

    [Theory]
    [InlineData("<fetch><entity name='account'/></fetch>")]
    [InlineData("<?xml version='1.0'?>\n<!-- <fetch>example</fetch> -->\n<fetch><entity name='account'/></fetch>")]
    [InlineData("<!-- <fetch>example</fetch> -->\n<fetch><entity name='account'/></fetch>")]
    [InlineData("\n  <fetch>\n<entity name='account'/>\n</fetch>")]
    public void ActionTargetsRootAndIgnoresCommentExamples(string text)
    {
        Assert.Equal(text.LastIndexOf("<fetch>", StringComparison.Ordinal), FetchXmlExecutionInput.FindActionPosition(text));
    }

    [Theory]
    [InlineData("")]
    [InlineData("<!-- <fetch/> -->")]
    [InlineData("<other><fetch/></other>")]
    public void DoesNotAddAnActionWithoutFetchRoot(string text) =>
        Assert.Null(FetchXmlExecutionInput.FindActionPosition(text));
}
