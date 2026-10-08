#nullable enable
namespace XrmTools.CodeGen.CustomApi;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.Analyzers;
using XrmTools.Environments;
using XrmTools.WebApi;
using XrmTools.WebApi.Entities;
using XrmTools.WebApi.Methods;

internal interface ICustomApiSourceGenerator
{
    Task<GeneratedClient> GenerateAsync(Document document, TextSpan classSpan, CancellationToken token);
}

[Export(typeof(ICustomApiSourceGenerator))]
[method: ImportingConstructor]
internal sealed class CustomApiSourceGenerator(ICSharpXrmMetaParser parser, IEnvironmentSelection environments, IWebApiService webApi) : ICustomApiSourceGenerator
{
    public async Task<GeneratedClient> GenerateAsync(Document document, TextSpan classSpan, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var root = await document.GetSyntaxRootAsync(token).ConfigureAwait(false);
        var declaration = root?.FindNode(classSpan) as ClassDeclarationSyntax;
        var model = await document.GetSemanticModelAsync(token).ConfigureAwait(false);
        var symbol = declaration == null ? null : model?.GetDeclaredSymbol(declaration, token) as INamedTypeSymbol;
        if (symbol == null || CustomApiSourceReader.Attribute(symbol, "CustomApiAttribute") == null)
            throw new InvalidOperationException("The selected class is not an Xrm Tools Custom API.");
        var api = parser.ParsePluginConfig(symbol, model!.Compilation)?.CustomApi
            ?? throw new InvalidOperationException("The Custom API class must also have the Xrm Tools [Plugin] attribute.");
        if (api.IsFunction) throw new InvalidOperationException("OpenAPI generation for functions is not available yet. Currently, only Custom API actions are supported.");
        var schemas = CustomApiSourceReader.Enrich(api, api, symbol, model.Compilation, token);
        if (string.IsNullOrWhiteSpace(api.Description)) api.Description = CustomApiSourceReader.Summary(symbol);
        // Source generation never prompts for an environment or reads deployed API definitions.
        var environment = await environments.GetSelectedEnvironmentAsync().ConfigureAwait(false);
        var baseUrl = environment?.BaseServiceUrl;
        var tables = new Dictionary<string, EntityMetadata>(StringComparer.Ordinal);
        if (api.BindingType != CustomApi.BindingTypes.Global)
        {
            if (baseUrl == null) throw new InvalidOperationException("Select a Dataverse environment to resolve the bound table's entity set name. The Custom API does not need to be deployed.");
            var name = CustomApiOpenApiWriter.RequireName(api.BoundEntityLogicalName);
            var metadata = await webApi.RetrieveMultipleAsync<EntityMetadata>(new Uri(baseUrl,
                "EntityDefinitions?$select=LogicalName,EntitySetName,PrimaryIdAttribute&$filter=LogicalName eq '" + name + "'").AbsoluteUri,
                cancellationToken: token).ConfigureAwait(false);
            tables[name] = metadata.Value.SingleOrDefault() ?? throw new InvalidOperationException("Bound table metadata not found: " + name);
        }
        token.ThrowIfCancellationRequested();
        return new GeneratedClient(api.UniqueName + ".openapi.json", CustomApiOpenApiWriter.Write(api, baseUrl, tables, schemas));
    }
}
