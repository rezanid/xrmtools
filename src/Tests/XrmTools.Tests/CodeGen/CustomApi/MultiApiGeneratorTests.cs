#nullable enable
namespace XrmTools.Tests.CodeGen.CustomApi;

using Moq;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using XrmTools.CodeGen.CustomApi;
using XrmTools.WebApi;
using XrmTools.WebApi.Entities;
using XrmTools.WebApi.Types;
using Api = XrmTools.WebApi.Entities.CustomApi;

public sealed class MultiApiGeneratorTests
{
    private static readonly Uri Environment = new("https://example.crm.dynamics.com/api/data/v9.2/");

    [Theory]
    [InlineData(OpenApiOutputFormat.Json)]
    [InlineData(OpenApiOutputFormat.Yaml)]
    public async Task CollectionReadsPagedParametersAndReusesTableMetadata(OpenApiOutputFormat format)
    {
        var webApi = new Mock<IWebApiService>(MockBehavior.Strict);
        webApi.Setup(w => w.GetBaseUrlAsync()).ReturnsAsync(Environment);
        var apiCount = 0;
        webApi.Setup(w => w.SendAsync(It.IsAny<WebApiRequest<ODataQueryResponse<Api>>>(), false, It.IsAny<CancellationToken>()))
            .Returns(() => Task.FromResult(new ODataQueryResponse<Api> { Value = [new Api { UniqueName = "new_" + ++apiCount }] }));
        webApi.Setup(w => w.SendAsync(It.IsAny<WebApiRequest<ODataQueryResponse<CustomApiRequestParameter>>>(), false, It.IsAny<CancellationToken>()))
            .Returns<WebApiRequest<ODataQueryResponse<CustomApiRequestParameter>>, bool, CancellationToken>((r, _, _) => Task.FromResult(
                r.RequestUri!.OriginalString.Contains("page=2")
                ? new ODataQueryResponse<CustomApiRequestParameter> { Value = [new() { UniqueName = "Second", Type = CustomApiFieldType.String }] }
                : new ODataQueryResponse<CustomApiRequestParameter> { Value = [new() { UniqueName = "First", Type = CustomApiFieldType.EntityReference, LogicalEntityName = "account" }], NextLink = Environment + "customapirequestparameters?page=2" }));
        webApi.Setup(w => w.SendAsync(It.IsAny<WebApiRequest<ODataQueryResponse<CustomApiResponseProperty>>>(), false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ODataQueryResponse<CustomApiResponseProperty>());
        webApi.Setup(w => w.SendAsync(It.IsAny<WebApiRequest<ODataQueryResponse<EntityMetadata>>>(), false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ODataQueryResponse<EntityMetadata> { Value = [new EntityMetadata("account", "accounts") { PrimaryIdAttribute = "accountid" }] });
        var source = new Mock<ICustomApiSourceReader>(MockBehavior.Strict);
        source.Setup(s => s.ReadAsync(It.IsAny<Api>(), "Contoso.Plugin", "Contoso", It.IsAny<CancellationToken>())).ReturnsAsync(new CustomApiSourceSchemas());
        var settings = Settings(format);
        var generator = new CustomApiOpenApiGenerator(webApi.Object, source.Object, settings.Object);
        var result = await generator.GenerateManyAsync([new(Guid.NewGuid(), "Contoso.Plugin", "Contoso"), new(Guid.NewGuid(), "Contoso.Plugin", "Contoso")], Environment, TestContext.Current.CancellationToken);
        Assert.Equal("custom-apis.openapi." + (format == OpenApiOutputFormat.Json ? "json" : "yaml"), result.FileName);
        Assert.Contains("new_1", result.Content); Assert.Contains("new_2", result.Content);
        Assert.Contains("First", result.Content); Assert.Contains("Second", result.Content);
        webApi.Verify(w => w.SendAsync(It.IsAny<WebApiRequest<ODataQueryResponse<EntityMetadata>>>(), false, It.IsAny<CancellationToken>()), Times.Once);
        settings.Verify(s => s.GetFormatAsync(), Times.Once); settings.Verify(s => s.GetVersionAsync(), Times.Once);
        if (format == OpenApiOutputFormat.Json) Assert.Equal(2, ((JObject)JObject.Parse(result.Content)["paths"]!).Count);
    }

    [Fact]
    public async Task MissingApiFailsWithItsNameAndDoesNotProducePartialOutput()
    {
        var webApi = new Mock<IWebApiService>(MockBehavior.Strict);
        webApi.Setup(w => w.GetBaseUrlAsync()).ReturnsAsync(Environment);
        webApi.Setup(w => w.SendAsync(It.IsAny<WebApiRequest<ODataQueryResponse<Api>>>(), false, It.IsAny<CancellationToken>())).ReturnsAsync(new ODataQueryResponse<Api>());
        var generator = new CustomApiOpenApiGenerator(webApi.Object, Mock.Of<ICustomApiSourceReader>(), Mock.Of<IOpenApiOutputSettings>());
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => generator.GenerateManyAsync([new(Guid.NewGuid(), null, null, "new_Missing")], Environment, TestContext.Current.CancellationToken));
        Assert.Contains("new_Missing", ex.Message);
    }

    [Fact]
    public async Task EmptyDuplicateAndCanceledSelectionsNeverIssueRequests()
    {
        var webApi = new Mock<IWebApiService>(MockBehavior.Strict);
        var generator = new CustomApiOpenApiGenerator(webApi.Object, Mock.Of<ICustomApiSourceReader>(), Mock.Of<IOpenApiOutputSettings>());
        var target = new CustomApiOpenApiTarget(Guid.NewGuid(), null, null);
        await Assert.ThrowsAsync<ArgumentException>(() => generator.GenerateManyAsync([], Environment, TestContext.Current.CancellationToken));
        await Assert.ThrowsAsync<ArgumentException>(() => generator.GenerateManyAsync([target, target], Environment, TestContext.Current.CancellationToken));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => generator.GenerateManyAsync([target], Environment, new CancellationToken(true)));
    }

    private static Mock<IOpenApiOutputSettings> Settings(OpenApiOutputFormat format)
    {
        var settings = new Mock<IOpenApiOutputSettings>(MockBehavior.Strict);
        settings.Setup(s => s.GetFormatAsync()).ReturnsAsync(format);
        settings.Setup(s => s.GetVersionAsync()).ReturnsAsync(OpenApiSpecificationVersion.V3_0);
        return settings;
    }
}
