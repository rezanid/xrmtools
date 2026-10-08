#nullable enable
namespace XrmTools.Tests.CodeGen.CustomApi;

using Newtonsoft.Json.Linq;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using XrmTools.CodeGen.CustomApi;
using XrmTools.Options;
using XrmTools.WebApi.Entities;
using XrmTools.WebApi.Types;
using XrmTools.WebApi;
using YamlDotNet.Serialization;

public sealed class OpenApiDocumentFormatterTests
{
    [Theory]
    [InlineData(OpenApiOutputFormat.Json, OpenApiSpecificationVersion.V3_0)]
    [InlineData(OpenApiOutputFormat.Yaml, OpenApiSpecificationVersion.V3_0)]
    [InlineData(OpenApiOutputFormat.Json, OpenApiSpecificationVersion.V3_2)]
    [InlineData(OpenApiOutputFormat.Yaml, OpenApiSpecificationVersion.V3_2)]
    public async Task ExplorerGeneratorUsesSelectedOutputFormat(OpenApiOutputFormat format, OpenApiSpecificationVersion version)
    {
        var webApi = new Mock<IWebApiService>(MockBehavior.Strict);
        webApi.Setup(w => w.GetBaseUrlAsync()).ReturnsAsync(new Uri("https://example.crm.dynamics.com/api/data/v9.2/"));
        webApi.Setup(w => w.SendAsync(It.IsAny<WebApiRequest<ODataQueryResponse<CustomApi>>>(), false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ODataQueryResponse<CustomApi> { Value = [new CustomApi { UniqueName = "new_Test" }] });
        webApi.Setup(w => w.SendAsync(It.IsAny<WebApiRequest<ODataQueryResponse<CustomApiRequestParameter>>>(), false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ODataQueryResponse<CustomApiRequestParameter>());
        webApi.Setup(w => w.SendAsync(It.IsAny<WebApiRequest<ODataQueryResponse<CustomApiResponseProperty>>>(), false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ODataQueryResponse<CustomApiResponseProperty>());
        var source = new Mock<ICustomApiSourceReader>(MockBehavior.Strict);
        source.Setup(s => s.ReadAsync(It.IsAny<CustomApi>(), null, null, It.IsAny<CancellationToken>())).ReturnsAsync(new CustomApiSourceSchemas());
        var settings = new Mock<IOpenApiOutputSettings>(MockBehavior.Strict);
        settings.Setup(s => s.GetFormatAsync()).ReturnsAsync(format);
        settings.Setup(s => s.GetVersionAsync()).ReturnsAsync(version);
        var output = await new CustomApiOpenApiGenerator(webApi.Object, source.Object, settings.Object)
            .GenerateAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);
        Assert.Equal("new_Test.openapi." + (format == OpenApiOutputFormat.Json ? "json" : "yaml"), output.FileName);
        var doc = format == OpenApiOutputFormat.Json ? JObject.Parse(output.Content) : ParseYaml(output.Content);
        Assert.NotNull(doc["paths"]!["/new_Test"]!["post"]);
        Assert.Equal(version == OpenApiSpecificationVersion.V3_2 ? "3.2.1" : "3.0.4", (string?)doc["openapi"]);
        settings.Verify(s => s.GetFormatAsync(), Times.Once);
        settings.Verify(s => s.GetVersionAsync(), Times.Once);
    }

    [Theory]
    [InlineData(false, OpenApiSpecificationVersion.V3_0)]
    [InlineData(true, OpenApiSpecificationVersion.V3_0)]
    [InlineData(false, OpenApiSpecificationVersion.V3_2)]
    [InlineData(true, OpenApiSpecificationVersion.V3_2)]
    public void JsonAndYamlDescribeIdenticalActionAndFunctionContracts(bool function, OpenApiSpecificationVersion version)
    {
        var api = new CustomApi { UniqueName = "new_Test", IsFunction = function,
            RequestParameters = [new CustomApiRequestParameter { UniqueName = "Name", Type = CustomApiFieldType.String, IsOptional = true }],
            ResponseProperties = [new CustomApiResponseProperty { UniqueName = "Result", Type = CustomApiFieldType.String }] };
        var doc = CustomApiOpenApiWriter.Build(api, null, new Dictionary<string, EntityMetadata>(), version: version);
        var json = OpenApiDocumentFormatter.Create(api.UniqueName, doc, OpenApiOutputFormat.Json);
        var yaml = OpenApiDocumentFormatter.Create(api.UniqueName, doc, OpenApiOutputFormat.Yaml);
        Assert.Equal("new_Test.openapi.json", json.FileName);
        Assert.Equal("new_Test.openapi.yaml", yaml.FileName);
        Assert.True(JToken.DeepEquals(JObject.Parse(json.Content), ParseYaml(yaml.Content)));
    }

    [Fact]
    public void YamlPreservesStringKeysExamplesEnumsAndReferences()
    {
        var doc = new JObject
        {
            ["responses"] = new JObject { ["200"] = new JObject { ["description"] = "success" }, ["4XX"] = new JObject { ["description"] = "error" } },
            ["properties"] = new JObject { ["on"] = new JObject { ["example"] = "true" }, ["@Name"] = new JObject { ["example"] = "'O''Brien & café/+%'" } },
            ["examples"] = new JArray("null", "false", "1", "01", "2000-01-01T00:00:00Z", "a\r\nb\nc", ""),
            ["enum"] = new JArray(1, 2), ["required"] = true, ["nullable"] = false, ["empty"] = new JArray(),
            ["x-enumDescriptions"] = new JObject { ["1"] = "First" }, ["$ref"] = "#/components/schemas/Entity",
        };
        var yaml = OpenApiDocumentFormatter.Create("new_Test", doc, OpenApiOutputFormat.Yaml);
        var parsed = ParseYaml(yaml.Content);
        Assert.True(JToken.DeepEquals(doc, parsed));
        Assert.Equal(JTokenType.String, parsed["examples"]![2]!.Type);
        Assert.Equal(JTokenType.Integer, parsed["enum"]![0]!.Type);
        Assert.Equal(JTokenType.Boolean, parsed["required"]!.Type);
    }

    [Fact]
    public void GeneralOptionsDefaultToJsonAndRejectUnknownSerializationFormat()
    {
        Assert.Equal(OpenApiOutputFormat.Json, new GeneralOptions().OpenApiOutputFormat);
        Assert.Equal(OpenApiSpecificationVersion.V3_0, new GeneralOptions().OpenApiSpecificationVersion);
        Assert.Throws<ArgumentOutOfRangeException>(() => OpenApiDocumentFormatter.Create("new_Test", new JObject(), (OpenApiOutputFormat)999));
    }

    private static JToken ParseYaml(string content)
    {
        var model = new DeserializerBuilder().WithAttemptingUnquotedStringTypeDeserialization().Build().Deserialize<object>(content);
        return JToken.FromObject(model);
    }
}
