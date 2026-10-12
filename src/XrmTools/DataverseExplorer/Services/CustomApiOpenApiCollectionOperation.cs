#nullable enable
namespace XrmTools.DataverseExplorer.Services;

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.CodeGen.CustomApi;
using XrmTools.DataverseExplorer.Models;
using XrmTools.WebApi;
using XrmTools.WebApi.Methods;

[Export(typeof(IExplorerCollectionOperation))]
[method: ImportingConstructor]
internal sealed class CustomApiOpenApiCollectionOperation(IWebApiService webApi, ICustomApiOpenApiGenerator generator) : IExplorerCollectionDocumentationOperation
{
    public string Title => "Generate OpenAPI specification";
    public string ActionLabel => "Generate";
    public bool SupportsScope(ExplorerNodeBase scope) => scope is AssemblyNode or PackageNode or CategoryNode { ArtifactCategory: "Assemblies" or "Packages" };

    public async Task<IReadOnlyList<ExplorerCollectionCandidate>> LoadAsync(ExplorerNodeBase scope, Uri environment, CancellationToken token)
    {
        var baseUrl = await CustomApiEnvironment.RequireAsync(webApi, environment, token);
        // Single-valued expansions avoid paging nested collections. No parameters, steps or images are fetched.
        var query = "customapis?$select=customapiid,displayname,uniquename,modifiedon&$orderby=uniquename" +
            "&$expand=PluginTypeId($select=typename;$expand=pluginassemblyid($select=pluginassemblyid,name,_packageid_value))";
        var rows = new List<JObject>();
        string? next = new Uri(baseUrl, query).AbsoluteUri;
        while (next != null)
        {
            var page = await webApi.RetrieveMultipleAsync<JObject>(next, cancellationToken: token).ConfigureAwait(false);
            token.ThrowIfCancellationRequested();
            rows.AddRange(page.Value); next = page.NextLink;
        }
        return SelectCandidates(rows, scope);
    }

    internal static IReadOnlyList<ExplorerCollectionCandidate> SelectCandidates(IEnumerable<JObject> rows, ExplorerNodeBase scope)
    {
        var result = new List<ExplorerCollectionCandidate>();
        foreach (var api in rows)
        {
            var plugin = api["PluginTypeId"] as JObject;
            var assembly = plugin?["pluginassemblyid"] as JObject;
            // These commands are scoped to plugin assemblies/packages, so unassociated APIs are outside their scope.
            if (assembly == null) continue;
            var assemblyId = (Guid?)assembly["pluginassemblyid"] ?? Guid.Empty;
            var packageId = (Guid?)assembly["_packageid_value"];
            var included = scope switch
            {
                AssemblyNode node => node.AssemblyId == assemblyId,
                PackageNode node => node.PackageId == packageId,
                CategoryNode { ArtifactCategory: "Assemblies" } => packageId == null,
                CategoryNode { ArtifactCategory: "Packages" } => packageId != null,
                _ => false,
            };
            if (!included) continue;
            var id = (Guid?)api["customapiid"] ?? Guid.Empty;
            var name = (string?)api["uniquename"];
            if (id == Guid.Empty || assemblyId == Guid.Empty || string.IsNullOrWhiteSpace(name))
                throw new InvalidOperationException("Custom API candidate metadata is incomplete.");
            var assemblyName = (string?)assembly["name"] ?? assemblyId.ToString();
            result.Add(new ExplorerCollectionCandidate(id.ToString(), (string?)api["displayname"] ?? name!, name!,
                assemblyId.ToString(), assemblyName, new CustomApiOpenApiTarget(id, (string?)plugin?["typename"], assemblyName, name), (DateTimeOffset?)api["modifiedon"]));
        }
        return result;
    }

    public Task ExecuteAsync(IReadOnlyList<ExplorerCollectionCandidate> selection, Uri environment, CancellationToken token)
        => GenerateAsync(selection, environment, false, token);

    public Task ExecuteWithDocumentationAsync(IReadOnlyList<ExplorerCollectionCandidate> selection, Uri environment, CancellationToken token)
        => GenerateAsync(selection, environment, true, token);

    private async Task GenerateAsync(IReadOnlyList<ExplorerCollectionCandidate> selection, Uri environment, bool documentation, CancellationToken token)
    {
        var result = await generator.GenerateManyAsync(selection.Select(c => (CustomApiOpenApiTarget)c.Target).ToArray(), environment, token);
        token.ThrowIfCancellationRequested();
        if (documentation)
            await GeneratedCustomApiDocument.OpenWithDocumentationAsync(result, token);
        else
            await GeneratedCustomApiDocument.OpenAsync(result, "OpenAPI specification", token);
    }
}
