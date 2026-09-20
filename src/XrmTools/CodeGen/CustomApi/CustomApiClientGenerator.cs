#nullable enable
namespace XrmTools.CodeGen.CustomApi;

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.WebApi;
using XrmTools.WebApi.Entities;
using XrmTools.WebApi.Methods;
using Api = XrmTools.WebApi.Entities.CustomApi;

internal sealed class GeneratedClient(string fileName, string content)
{
    public string FileName { get; } = fileName;
    public string Content { get; } = content;
}

internal interface ICustomApiClientGenerator
{
    Task<GeneratedClient> GenerateAsync(Guid apiId, CustomApiClientLanguage language, CancellationToken cancellationToken);
}

[Export(typeof(ICustomApiClientGenerator))]
[method: ImportingConstructor]
internal sealed class CustomApiClientGenerator(IWebApiService webApi, ScribanClientRenderer renderer) : ICustomApiClientGenerator
{
    public async Task<GeneratedClient> GenerateAsync(Guid apiId, CustomApiClientLanguage language, CancellationToken cancellationToken)
    {
        if (apiId == Guid.Empty) throw new ArgumentException("A Custom API ID is required.", nameof(apiId));
        if (!Enum.IsDefined(typeof(CustomApiClientLanguage), language)) throw new ArgumentOutOfRangeException(nameof(language));
        cancellationToken.ThrowIfCancellationRequested();
        var baseUrl = await webApi.GetBaseUrlAsync() ?? throw new InvalidOperationException("Select a Dataverse environment first.");
        cancellationToken.ThrowIfCancellationRequested();
        // Read the server definition, not the partial display model in the explorer.
        var query = $"customapis?$filter=customapiid eq {apiId}&$select=customapiid,uniquename,isfunction,bindingtype,boundentitylogicalname,isprivate";
        var response = await webApi.RetrieveMultipleAsync<Api>(new Uri(baseUrl, query).AbsoluteUri, cancellationToken: cancellationToken);
        var api = response.Value.SingleOrDefault() ?? throw new InvalidOperationException("The Custom API no longer exists. Refresh Dataverse Explorer.");
        api.RequestParameters = await LoadAllAsync<CustomApiRequestParameter>(baseUrl,
            $"customapirequestparameters?$filter=_customapiid_value eq {apiId}&$select=uniquename,type,logicalentityname,isoptional", cancellationToken);
        api.ResponseProperties = await LoadAllAsync<CustomApiResponseProperty>(baseUrl,
            $"customapiresponseproperties?$filter=_customapiid_value eq {apiId}&$select=uniquename,type,logicalentityname", cancellationToken);
        var tables = new Dictionary<string, EntityMetadata>(StringComparer.Ordinal);
        if (language is CustomApiClientLanguage.Http or CustomApiClientLanguage.OData)
        {
            var names = api.RequestParameters.Select(field => field.LogicalEntityName).Concat([api.BoundEntityLogicalName])
                .Where(name => !string.IsNullOrWhiteSpace(name)).Distinct(StringComparer.Ordinal);
            foreach (var name in names)
            {
                var metadata = await webApi.RetrieveMultipleAsync<EntityMetadata>(new Uri(baseUrl,
                    "EntityDefinitions?$select=LogicalName,EntitySetName,PrimaryIdAttribute&$filter=LogicalName eq '" + name!.Replace("'", "''") + "'").AbsoluteUri,
                    cancellationToken: cancellationToken);
                tables[name!] = metadata.Value.SingleOrDefault() ?? throw new InvalidOperationException("Table metadata not found: " + name);
            }
        }
        cancellationToken.ThrowIfCancellationRequested();
        var model = CustomApiClientModelBuilder.Build(api, language, baseUrl, tables);
        var resource = "XrmTools.CodeGenTemplates.CustomApi." + language + ".sbn";
        using var stream = typeof(CustomApiClientGenerator).Assembly.GetManifestResourceStream(resource)
            ?? throw new InvalidOperationException("Bundled client template not found: " + resource);
        using var reader = new StreamReader(stream);
        var content = renderer.Render(await reader.ReadToEndAsync(), resource, model, cancellationToken);
        var extension = language switch { CustomApiClientLanguage.PluginCSharp => ".cs", CustomApiClientLanguage.TypeScript => ".ts", CustomApiClientLanguage.OData => ".odata", _ => ".http" };
        return new GeneratedClient(model.ClassName + extension, content);
    }

    private async Task<List<T>> LoadAllAsync<T>(Uri baseUrl, string query, CancellationToken token)
    {
        var result = new List<T>();
        string? next = new Uri(baseUrl, query).AbsoluteUri;
        while (next != null)
        {
            token.ThrowIfCancellationRequested();
            var page = await webApi.RetrieveMultipleAsync<T>(next, cancellationToken: token);
            result.AddRange(page.Value);
            next = page.NextLink;
        }
        return result;
    }
}
