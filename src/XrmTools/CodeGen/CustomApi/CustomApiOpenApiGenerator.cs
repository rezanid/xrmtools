#nullable enable
namespace XrmTools.CodeGen.CustomApi;

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.WebApi;
using XrmTools.WebApi.Entities;
using XrmTools.WebApi.Methods;
using Api = XrmTools.WebApi.Entities.CustomApi;

internal interface ICustomApiOpenApiGenerator
{
    Task<GeneratedClient> GenerateAsync(Guid apiId, CancellationToken cancellationToken, string? typeName = null, string? assemblyName = null);
    Task<GeneratedClient> GenerateManyAsync(IReadOnlyList<CustomApiOpenApiTarget> targets, Uri environment, CancellationToken cancellationToken);
}

internal sealed record CustomApiOpenApiTarget(Guid ApiId, string? TypeName, string? AssemblyName, string? UniqueName = null);

[Export(typeof(ICustomApiOpenApiGenerator))]
[method: ImportingConstructor]
internal sealed class CustomApiOpenApiGenerator(IWebApiService webApi, ICustomApiSourceReader sourceReader, IOpenApiOutputSettings outputSettings) : ICustomApiOpenApiGenerator
{
    public async Task<GeneratedClient> GenerateAsync(Guid apiId, CancellationToken cancellationToken, string? typeName = null, string? assemblyName = null)
    {
        if (apiId == Guid.Empty) throw new ArgumentException("A Custom API ID is required.", nameof(apiId));
        cancellationToken.ThrowIfCancellationRequested();
        var baseUrl = await webApi.GetBaseUrlAsync().ConfigureAwait(false) ?? throw new InvalidOperationException("Select a Dataverse environment first.");
        var tables = new Dictionary<string, EntityMetadata>(StringComparer.Ordinal);
        var input = await LoadAsync(new CustomApiOpenApiTarget(apiId, typeName, assemblyName), baseUrl, tables, cancellationToken).ConfigureAwait(false);
        var format = await outputSettings.GetFormatAsync().ConfigureAwait(false);
        var version = await outputSettings.GetVersionAsync().ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        return OpenApiDocumentFormatter.Create(input.Api.UniqueName!, CustomApiOpenApiWriter.Build(input.Api, baseUrl, tables, input.Source, version), format);
    }

    public async Task<GeneratedClient> GenerateManyAsync(IReadOnlyList<CustomApiOpenApiTarget> targets, Uri environment, CancellationToken cancellationToken)
    {
        if (targets.Count == 0) throw new ArgumentException("Select at least one Custom API.", nameof(targets));
        if (targets.Any(t => t.ApiId == Guid.Empty) || targets.Select(t => t.ApiId).Distinct().Count() != targets.Count)
            throw new ArgumentException("Custom API identities must be valid and unique.", nameof(targets));
        var baseUrl = await CustomApiEnvironment.RequireAsync(webApi, environment, cancellationToken).ConfigureAwait(false);
        var tables = new Dictionary<string, EntityMetadata>(StringComparer.Ordinal);
        var inputs = new List<CustomApiOpenApiInput>();
        foreach (var target in targets.OrderBy(t => t.ApiId))
        {
            try { inputs.Add(await LoadAsync(target, baseUrl, tables, cancellationToken).ConfigureAwait(false)); }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex) { throw new InvalidOperationException("Unable to generate API " + (target.UniqueName ?? target.ApiId.ToString()) + ": " + ex.Message, ex); }
        }
        var format = await outputSettings.GetFormatAsync().ConfigureAwait(false);
        var version = await outputSettings.GetVersionAsync().ConfigureAwait(false);
        var groupByAssembly = await outputSettings.GetGroupByAssemblyAsync().ConfigureAwait(false);
        await CustomApiEnvironment.RequireAsync(webApi, environment, cancellationToken).ConfigureAwait(false);
        return OpenApiDocumentFormatter.Create("custom-apis", CustomApiOpenApiWriter.BuildMany(inputs, baseUrl, tables, version, groupByAssembly), format);
    }

    private async Task<CustomApiOpenApiInput> LoadAsync(CustomApiOpenApiTarget target, Uri baseUrl,
        Dictionary<string, EntityMetadata> tables, CancellationToken cancellationToken)
    {
        var apiId = target.ApiId;
        cancellationToken.ThrowIfCancellationRequested();
        var response = await webApi.RetrieveMultipleAsync<Api>(new Uri(baseUrl,
            $"customapis?$filter=customapiid eq {apiId}&$select=customapiid,uniquename,displayname,description,isfunction,bindingtype,boundentitylogicalname,isprivate,executeprivilegename").AbsoluteUri,
            cancellationToken: cancellationToken).ConfigureAwait(false);
        var api = response.Value.SingleOrDefault() ?? throw new InvalidOperationException("The Custom API no longer exists. Refresh Dataverse Explorer.");
        api.RequestParameters = await LoadAllAsync<CustomApiRequestParameter>(baseUrl,
            $"customapirequestparameters?$filter=_customapiid_value eq {apiId}&$select=uniquename,displayname,description,type,logicalentityname,isoptional", cancellationToken).ConfigureAwait(false);
        api.ResponseProperties = await LoadAllAsync<CustomApiResponseProperty>(baseUrl,
            $"customapiresponseproperties?$filter=_customapiid_value eq {apiId}&$select=uniquename,displayname,description,type,logicalentityname", cancellationToken).ConfigureAwait(false);
        ODataFunctionParameters.Validate(api);
        var names = api.RequestParameters.Select(p => p.LogicalEntityName).Concat(api.ResponseProperties.Select(p => p.LogicalEntityName))
            .Concat([api.BoundEntityLogicalName]).Where(n => !string.IsNullOrWhiteSpace(n)).Distinct(StringComparer.Ordinal);
        foreach (var name in names)
        {
            CustomApiOpenApiWriter.RequireName(name);
            if (tables.ContainsKey(name!)) continue;
            var metadata = await webApi.RetrieveMultipleAsync<EntityMetadata>(new Uri(baseUrl,
                "EntityDefinitions?$select=LogicalName,EntitySetName,PrimaryIdAttribute&$filter=LogicalName eq '" + name + "'").AbsoluteUri,
                cancellationToken: cancellationToken).ConfigureAwait(false);
            tables[name!] = metadata.Value.SingleOrDefault() ?? throw new InvalidOperationException("Table metadata not found: " + name);
        }
        cancellationToken.ThrowIfCancellationRequested();
        var source = await sourceReader.ReadAsync(api, target.TypeName, target.AssemblyName, cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        return new CustomApiOpenApiInput(api, source, target.AssemblyName);
    }

    private async Task<List<T>> LoadAllAsync<T>(Uri baseUrl, string query, CancellationToken token)
    {
        var result = new List<T>();
        string? next = new Uri(baseUrl, query).AbsoluteUri;
        while (next != null)
        {
            token.ThrowIfCancellationRequested();
            var page = await webApi.RetrieveMultipleAsync<T>(next, cancellationToken: token).ConfigureAwait(false);
            result.AddRange(page.Value);
            next = page.NextLink;
        }
        return result;
    }
}
