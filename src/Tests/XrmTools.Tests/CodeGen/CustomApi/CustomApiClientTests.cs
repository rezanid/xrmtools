#nullable enable
namespace XrmTools.Tests.CodeGen.CustomApi;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Moq;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using XrmTools.CodeGen.CustomApi;
using XrmTools.OData;
using XrmTools.WebApi;
using XrmTools.WebApi.Entities;
using XrmTools.WebApi.Types;
using Api = XrmTools.WebApi.Entities.CustomApi;

public sealed class CustomApiClientTests
{
    private static readonly Uri BaseUrl = new("https://example.crm.dynamics.com/api/data/v9.2/");
    private static readonly Dictionary<string, EntityMetadata> Tables = new()
    {
        ["account"] = new EntityMetadata("account", "accounts") { PrimaryIdAttribute = "accountid" },
    };

    [Theory]
    [InlineData(Api.BindingTypes.Global, false)]
    [InlineData(Api.BindingTypes.Entity, false)]
    [InlineData(Api.BindingTypes.EntityCollection, false)]
    [InlineData(Api.BindingTypes.Global, true)]
    [InlineData(Api.BindingTypes.Entity, true)]
    [InlineData(Api.BindingTypes.EntityCollection, true)]
    public void CSharpHandlesAllParameterTypesAndBindings(Api.BindingTypes binding, bool function)
    {
        var output = Render(AllTypes(binding, function), CustomApiClientLanguage.PluginCSharp);
        Assert.Empty(CSharpSyntaxTree.ParseText(output).GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Contains("IOrganizationService service", output);
        Assert.Contains("new OrganizationRequest(\"new_AllTypes\")", output);
        Assert.Contains("service.Execute(request)", output);
        Assert.DoesNotContain("ServiceClient", output);
        Assert.Equal(binding == Api.BindingTypes.Entity, output.Contains("request.Parameters[\"Target\"]"));
        Assert.Contains("if (input.Parameter_P_Boolean != null)", output);
        SaveFixture("client-" + binding + "-" + function + ".cs", output);
    }

    [Theory]
    [InlineData(Api.BindingTypes.Global, false)]
    [InlineData(Api.BindingTypes.Entity, false)]
    [InlineData(Api.BindingTypes.Global, true)]
    [InlineData(Api.BindingTypes.Entity, true)]
    public void TypeScriptUsesFormClientAndCorrectMetadata(Api.BindingTypes binding, bool function)
    {
        var output = Render(AllTypes(binding, function), CustomApiClientLanguage.TypeScript);
        Assert.Contains("Xrm.WebApi.online.execute(new Request(input))", output);
        Assert.Contains("operationType: " + (function ? "1" : "0"), output);
        Assert.Contains(binding == Api.BindingTypes.Entity ? "boundParameter: \"entity\"" : "boundParameter: null", output);
        Assert.Contains("if (response.status === 204) return {};", output);
        SaveFixture("client-" + binding + "-" + function + ".ts", output);
    }

    [Theory]
    [InlineData(Api.BindingTypes.Global, "new_AllTypes")]
    [InlineData(Api.BindingTypes.Entity, "accounts({{recordId}})/Microsoft.Dynamics.CRM.new_AllTypes")]
    [InlineData(Api.BindingTypes.EntityCollection, "accounts/Microsoft.Dynamics.CRM.new_AllTypes")]
    public void HttpActionsHaveCorrectPathsAndJson(Api.BindingTypes binding, string path)
    {
        var output = Render(AllTypes(binding, false), CustomApiClientLanguage.Http);
        Assert.Contains("POST {{baseUrl}}/" + path, output);
        Assert.Contains("Authorization: Bearer {{accessToken}}", output);
        var body = JObject.Parse(output.Substring(output.IndexOf('{', output.IndexOf("Content-Type:", StringComparison.Ordinal))));
        Assert.Null(body["Target"]);
        Assert.Equal("Microsoft.Dynamics.CRM.account", (string?)body["P_EntityReference"]!["@odata.type"]);
        Assert.NotNull(body["P_EntityReference"]!["accountid"]);
    }

    [Fact]
    public void HttpFunctionUsesAliasesAndNoBody()
    {
        var output = Render(AllTypes(Api.BindingTypes.EntityCollection, true), CustomApiClientLanguage.Http);
        Assert.Contains("GET {{baseUrl}}/accounts/Microsoft.Dynamics.CRM.new_AllTypes(", output);
        Assert.Contains("P_String=@p", output);
        Assert.Contains("REPLACE_WITH_VALUE", output);
        Assert.DoesNotContain("Content-Type:", output);
    }

    [Fact]
    public void CollectionBoundTypeScriptHasActionableDiagnostic()
    {
        var error = Assert.Throws<InvalidOperationException>(() => Render(AllTypes(Api.BindingTypes.EntityCollection, false), CustomApiClientLanguage.TypeScript));
        Assert.Contains("Use the HTTP generator", error.Message);
    }

    [Theory]
    [InlineData(Api.BindingTypes.Global, false)]
    [InlineData(Api.BindingTypes.Entity, false)]
    [InlineData(Api.BindingTypes.EntityCollection, false)]
    [InlineData(Api.BindingTypes.Global, true)]
    [InlineData(Api.BindingTypes.Entity, true)]
    [InlineData(Api.BindingTypes.EntityCollection, true)]
    public void ODataUsesRelativeHttpRequestWithEnvironmentAuthentication(Api.BindingTypes binding, bool function)
    {
        var api = AllTypes(binding, function);
        var output = Render(api, CustomApiClientLanguage.OData);
        Assert.DoesNotContain("baseUrl", output);
        Assert.DoesNotContain("accessToken", output);
        Assert.DoesNotContain("Authorization:", output);
        Assert.DoesNotContain(BaseUrl.Host, output);
        var document = ODataDocument.Parse(output);
        Assert.Empty(document.Errors);
        var request = document.Resolve(Assert.Single(document.Requests));
        var http = CustomApiClientModelBuilder.Build(api, CustomApiClientLanguage.Http, BaseUrl, Tables);
        Assert.Equal(http.HttpMethod, request.Method);
        Assert.Equal(http.HttpUrl.Replace("{{baseUrl}}", "").Replace("{{recordId}}", Guid.Empty.ToString()), request.Target);
        Assert.Equal(http.HttpBody.Replace("\r\n", "\n"), request.Body);
        using var message = ODataRequestBuilder.Build(request, BaseUrl, "execution-time-token");
        Assert.Equal(BaseUrl.Host, message.RequestUri!.Host);
        Assert.StartsWith(BaseUrl.AbsolutePath, message.RequestUri.AbsolutePath);
        Assert.Equal("execution-time-token", message.Headers.Authorization!.Parameter);
    }

    [Fact]
    public void EmptyApiGeneratesValidClients()
    {
        var api = new Api { UniqueName = "new_Empty" };
        Assert.Contains("return new Output", Render(api, CustomApiClientLanguage.PluginCSharp));
        Assert.Contains("return {};", Render(api, CustomApiClientLanguage.TypeScript));
        Assert.Contains("{}", Render(api, CustomApiClientLanguage.Http));
    }

    [Theory]
    [InlineData(CustomApiFieldType.Entity)]
    [InlineData(CustomApiFieldType.EntityCollection)]
    public void TypeScriptWrapsDirectEntityResponses(CustomApiFieldType type)
    {
        var api = new Api { UniqueName = "new_AllTypes", ResponseProperties = [new CustomApiResponseProperty { UniqueName = "Result", Type = type }] };
        var output = Render(api, CustomApiClientLanguage.TypeScript);
        Assert.Contains("[\"Result\"]", output);
        Assert.Contains(type == CustomApiFieldType.Entity ? "await response.json() as Record<string, unknown>" : "body.value", output);
        SaveFixture("direct-" + type + ".ts", output);
    }

    [Fact]
    public void BoundTargetIsSynthesizedOnlyOnceWithoutMutatingMetadata()
    {
        var api = AllTypes(Api.BindingTypes.Entity, false);
        api.RequestParameters.Add(new CustomApiRequestParameter { UniqueName = "Target", Type = CustomApiFieldType.EntityReference });
        var count = api.RequestParameters.Count;
        var model = CustomApiClientModelBuilder.Build(api, CustomApiClientLanguage.PluginCSharp, BaseUrl, Tables);
        Assert.Single(model.Inputs.Where(field => field.BoundTarget));
        Assert.Equal(count, api.RequestParameters.Count);
    }

    [Fact]
    public void InvalidMetadataAndTemplateFailuresNeverBecomeCode()
    {
        var api = new Api { UniqueName = "new_Valid" };
        api.RequestParameters.Add(new CustomApiRequestParameter { UniqueName = "bad\"name", Type = CustomApiFieldType.String });
        Assert.Throws<InvalidOperationException>(() => Render(api, CustomApiClientLanguage.PluginCSharp));
        var renderer = new ScribanClientRenderer();
        Assert.Throws<InvalidOperationException>(() => renderer.Render("{{ if }}", "bad.sbn", new { }, CancellationToken.None));
        Assert.ThrowsAny<Exception>(() => renderer.Render("{{ missing }}", "missing.sbn", new { }, CancellationToken.None));
    }

    [Fact]
    public async Task RendererDoesNotShareConfiguration()
    {
        var renderer = new ScribanClientRenderer();
        var tasks = Enumerable.Range(0, 30).Select(index => Task.Run(() => renderer.Render("{{ model.value }}", "test", new { Value = index }, TestContext.Current.CancellationToken))).ToArray();
        Assert.Equal(Enumerable.Range(0, 30).Select(index => index.ToString()), await Task.WhenAll(tasks));
    }

    [Theory]
    [InlineData((int)CustomApiClientLanguage.PluginCSharp, ".cs")]
    [InlineData((int)CustomApiClientLanguage.Http, ".http")]
    [InlineData((int)CustomApiClientLanguage.OData, ".odata")]
    public async Task GeneratorLoadsByIdAndUsesUniqueNames(int languageValue, string extension)
    {
        var language = (CustomApiClientLanguage)languageValue;
        var id = Guid.NewGuid();
        var api = new Mock<IWebApiService>(MockBehavior.Strict);
        api.Setup(service => service.GetBaseUrlAsync()).ReturnsAsync(BaseUrl);
        var queries = new List<string>();
        api.Setup(service => service.SendAsync(It.IsAny<WebApiRequest<ODataQueryResponse<Api>>>(), false, It.IsAny<CancellationToken>()))
            .Callback<WebApiRequest<ODataQueryResponse<Api>>, bool, CancellationToken>((request, _, _) => queries.Add(request.RequestUri!.OriginalString))
            .ReturnsAsync(new ODataQueryResponse<Api> { Value = [new Api { Id = id, Name = "Display name", UniqueName = "new_ServerOperation" }] });
        api.Setup(service => service.SendAsync(It.IsAny<WebApiRequest<ODataQueryResponse<CustomApiRequestParameter>>>(), false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ODataQueryResponse<CustomApiRequestParameter> { Value = [new CustomApiRequestParameter { Name = "Display parameter", UniqueName = "WireName", Type = CustomApiFieldType.String }] });
        api.Setup(service => service.SendAsync(It.IsAny<WebApiRequest<ODataQueryResponse<CustomApiResponseProperty>>>(), false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ODataQueryResponse<CustomApiResponseProperty>());
        var result = await new CustomApiClientGenerator(api.Object, new ScribanClientRenderer()).GenerateAsync(id, language, TestContext.Current.CancellationToken);
        Assert.Contains(id.ToString(), Assert.Single(queries));
        Assert.Equal("new_ServerOperationClient" + extension, result.FileName);
        Assert.Contains("\"WireName\"", result.Content);
        Assert.DoesNotContain("Display parameter", result.Content);
    }

    [Fact]
    public async Task CancelledGenerationDoesNotReadMetadata()
    {
        var api = new Mock<IWebApiService>(MockBehavior.Strict);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => new CustomApiClientGenerator(api.Object, new ScribanClientRenderer())
            .GenerateAsync(Guid.NewGuid(), CustomApiClientLanguage.PluginCSharp, cancellation.Token));
        Assert.Empty(api.Invocations);
    }

    private static Api AllTypes(Api.BindingTypes binding, bool function)
    {
        var api = new Api { UniqueName = "new_AllTypes", BindingType = binding, BoundEntityLogicalName = binding == Api.BindingTypes.Global ? null : "account", IsFunction = function };
        foreach (CustomApiFieldType type in Enum.GetValues(typeof(CustomApiFieldType)))
        {
            api.RequestParameters.Add(new CustomApiRequestParameter { UniqueName = "P_" + type, Type = type, IsOptional = true, LogicalEntityName = type is CustomApiFieldType.Entity or CustomApiFieldType.EntityReference ? "account" : null });
            api.ResponseProperties.Add(new CustomApiResponseProperty { UniqueName = "R_" + type, Type = type });
        }
        return api;
    }

    private static string Render(Api api, CustomApiClientLanguage language)
    {
        var model = CustomApiClientModelBuilder.Build(api, language, BaseUrl, Tables);
        var resource = "XrmTools.CodeGenTemplates.CustomApi." + language + ".sbn";
        using var stream = typeof(CustomApiClientGenerator).Assembly.GetManifestResourceStream(resource)!;
        using var reader = new StreamReader(stream);
        return new ScribanClientRenderer().Render(reader.ReadToEnd(), resource, model, CancellationToken.None);
    }

    private static void SaveFixture(string name, string content)
    {
        var directory = Environment.GetEnvironmentVariable("XRMTOOLS_GENERATOR_TEST_OUTPUT");
        if (string.IsNullOrEmpty(directory)) return;
        Directory.CreateDirectory(directory);
        File.WriteAllText(Path.Combine(directory, name), content);
    }
}
