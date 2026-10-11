#nullable enable
namespace XrmTools.Tests.DataverseExplorer;

using Moq;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using XrmTools.CodeGen.CustomApi;
using XrmTools.DataverseExplorer.Models;
using XrmTools.DataverseExplorer.Services;
using XrmTools.WebApi;
using XrmTools.WebApi.Messages;

public sealed class OpenApiCollectionOperationTests
{
    private static readonly Uri Environment = new("https://example.crm.dynamics.com/api/data/v9.2/");
    private static readonly Guid AssemblyId = Guid.NewGuid();
    private static readonly Guid PackageId = Guid.NewGuid();

    [Fact]
    public async Task DiscoveryReadsEveryPageWithoutLoadingApiDetails()
    {
        var webApi = new Mock<IWebApiService>(MockBehavior.Strict);
        webApi.Setup(w => w.GetBaseUrlAsync()).ReturnsAsync(Environment);
        var requests = new List<string>();
        var next = Environment + "customapis?$skiptoken=next";
        webApi.Setup(w => w.SendAsync(It.IsAny<WebApiRequest<ODataQueryResponse<JObject>>>(), false, It.IsAny<CancellationToken>()))
            .Callback<WebApiRequest<ODataQueryResponse<JObject>>, bool, CancellationToken>((r, _, _) => requests.Add(r.RequestUri!.OriginalString))
            .Returns<WebApiRequest<ODataQueryResponse<JObject>>, bool, CancellationToken>((r, _, _) => Task.FromResult(
                r.RequestUri!.OriginalString == next ? new ODataQueryResponse<JObject> { Value = [Row("new_B")] } : new ODataQueryResponse<JObject> { Value = [Row("new_A")], NextLink = next }));
        var operation = new CustomApiOpenApiCollectionOperation(webApi.Object, Mock.Of<ICustomApiOpenApiGenerator>());
        var candidates = await operation.LoadAsync(new AssemblyNode { AssemblyId = AssemblyId }, Environment, TestContext.Current.CancellationToken);
        Assert.Equal(2, candidates.Count); Assert.Equal(2, requests.Count); Assert.Equal(next, requests[1]);
        Assert.DoesNotContain("RequestParameters", requests[0]); Assert.DoesNotContain("sdkmessageprocessingstep", requests[0]);
    }

    [Fact]
    public async Task DiscoveryResponseDeserializesSingleValuedAssociations()
    {
        var request = new RetrieveMultipleRequest<JObject>("customapis");
        using var raw = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(new JObject { ["value"] = new JArray(Row("new_A")) }.ToString()) };
        var page = await request.CreateResponseAsync(raw, TestContext.Current.CancellationToken);
        var candidates = CustomApiOpenApiCollectionOperation.SelectCandidates(page.Value, new AssemblyNode { AssemblyId = AssemblyId });
        var candidate = Assert.Single(candidates);
        var target = Assert.IsType<CustomApiOpenApiTarget>(candidate.Target);
        Assert.Equal("Contoso.Plugin", target.TypeName); Assert.Equal("Contoso", target.AssemblyName);
    }

    [Fact]
    public void ScopesDistinguishPackagedAndStandaloneAssembliesAndExcludeUnassociatedApis()
    {
        var standalone = Row("new_A"); var packaged = Row("new_B", PackageId);
        var unassociated = new JObject { ["customapiid"] = Guid.NewGuid(), ["uniquename"] = "new_C" };
        var assemblies = new CategoryNode(); assemblies.SetArtifactCategory("Assemblies");
        var packages = new CategoryNode(); packages.SetArtifactCategory("Packages");
        Assert.Equal("new_A", Assert.Single(CustomApiOpenApiCollectionOperation.SelectCandidates([standalone, packaged, unassociated], assemblies)).UniqueName);
        Assert.Equal("new_B", Assert.Single(CustomApiOpenApiCollectionOperation.SelectCandidates([standalone, packaged, unassociated], packages)).UniqueName);
        Assert.Single(CustomApiOpenApiCollectionOperation.SelectCandidates([standalone, packaged], new PackageNode { PackageId = PackageId }));
        Assert.Empty(CustomApiOpenApiCollectionOperation.SelectCandidates([standalone, packaged], new AssemblyNode { AssemblyId = Guid.NewGuid() }));
    }

    [Fact]
    public async Task ChangedEnvironmentIsRejectedBeforeAnyDiscoveryRequest()
    {
        var webApi = new Mock<IWebApiService>(MockBehavior.Strict);
        webApi.Setup(w => w.GetBaseUrlAsync()).ReturnsAsync(new Uri("https://other.crm.dynamics.com/api/data/v9.2/"));
        var operation = new CustomApiOpenApiCollectionOperation(webApi.Object, Mock.Of<ICustomApiOpenApiGenerator>());
        await Assert.ThrowsAsync<InvalidOperationException>(() => operation.LoadAsync(new AssemblyNode(), Environment, TestContext.Current.CancellationToken));
    }

    private static JObject Row(string name, Guid? package = null) => new()
    {
        ["customapiid"] = Guid.NewGuid(), ["displayname"] = name, ["uniquename"] = name,
        ["PluginTypeId"] = new JObject { ["typename"] = "Contoso.Plugin", ["pluginassemblyid"] = new JObject
        { ["pluginassemblyid"] = AssemblyId, ["name"] = "Contoso", ["_packageid_value"] = package.HasValue ? JToken.FromObject(package.Value) : JValue.CreateNull() } },
    };
}
