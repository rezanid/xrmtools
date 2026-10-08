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
}

[Export(typeof(ICustomApiOpenApiGenerator))]
[method: ImportingConstructor]
internal sealed class CustomApiOpenApiGenerator(IWebApiService webApi, ICustomApiSourceReader sourceReader) : ICustomApiOpenApiGenerator
{
    public async Task<GeneratedClient> GenerateAsync(Guid apiId, CancellationToken cancellationToken, string? typeName = null, string? assemblyName = null)
    {
        if (apiId == Guid.Empty) throw new ArgumentException("A Custom API ID is required.", nameof(apiId));
        cancellationToken.ThrowIfCancellationRequested();
        var baseUrl = await webApi.GetBaseUrlAsync().ConfigureAwait(false) ?? throw new InvalidOperationException("Select a Dataverse environment first.");
        var response = await webApi.RetrieveMultipleAsync<Api>(new Uri(baseUrl,
            $"customapis?$filter=customapiid eq {apiId}&$select=customapiid,uniquename,displayname,description,isfunction,bindingtype,boundentitylogicalname,isprivate,executeprivilegename").AbsoluteUri,
            cancellationToken: cancellationToken).ConfigureAwait(false);
        var api = response.Value.SingleOrDefault() ?? throw new InvalidOperationException("The Custom API no longer exists. Refresh Dataverse Explorer.");
        if (api.IsFunction) throw new InvalidOperationException("OpenAPI generation for functions is not available yet. Currently, only Custom API actions are supported.");
        api.RequestParameters = await LoadAllAsync<CustomApiRequestParameter>(baseUrl,
            $"customapirequestparameters?$filter=_customapiid_value eq {apiId}&$select=uniquename,displayname,description,type,logicalentityname,isoptional", cancellationToken).ConfigureAwait(false);
        api.ResponseProperties = await LoadAllAsync<CustomApiResponseProperty>(baseUrl,
            $"customapiresponseproperties?$filter=_customapiid_value eq {apiId}&$select=uniquename,displayname,description,type,logicalentityname", cancellationToken).ConfigureAwait(false);
        var tables = new Dictionary<string, EntityMetadata>(StringComparer.Ordinal);
        var names = api.RequestParameters.Select(p => p.LogicalEntityName).Concat(api.ResponseProperties.Select(p => p.LogicalEntityName))
            .Concat([api.BoundEntityLogicalName]).Where(n => !string.IsNullOrWhiteSpace(n)).Distinct(StringComparer.Ordinal);
        foreach (var name in names)
        {
            CustomApiOpenApiWriter.RequireName(name);
            var metadata = await webApi.RetrieveMultipleAsync<EntityMetadata>(new Uri(baseUrl,
                "EntityDefinitions?$select=LogicalName,EntitySetName,PrimaryIdAttribute&$filter=LogicalName eq '" + name + "'").AbsoluteUri,
                cancellationToken: cancellationToken).ConfigureAwait(false);
            tables[name!] = metadata.Value.SingleOrDefault() ?? throw new InvalidOperationException("Table metadata not found: " + name);
        }
        cancellationToken.ThrowIfCancellationRequested();
        var source = await sourceReader.ReadAsync(api, typeName, assemblyName, cancellationToken).ConfigureAwait(false);
        var content = CustomApiOpenApiWriter.Write(api, baseUrl, tables, source);
        return new GeneratedClient(api.UniqueName + ".openapi.json", content);
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
