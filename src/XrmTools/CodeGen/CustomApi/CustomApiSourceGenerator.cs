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
internal sealed class CustomApiSourceGenerator(ICSharpXrmMetaParser parser, IEnvironmentSelection environments, IWebApiService webApi, IOpenApiOutputSettings outputSettings) : ICustomApiSourceGenerator
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
        ODataFunctionParameters.Validate(api);
        var schemas = CustomApiSourceReader.Enrich(api, api, symbol, model.Compilation, token);
        if (string.IsNullOrWhiteSpace(api.Description)) api.Description = CustomApiSourceReader.Summary(symbol);
        // Source generation never prompts for an environment or reads deployed API definitions.
        var environment = await environments.GetSelectedEnvironmentAsync().ConfigureAwait(false);
        var baseUrl = environment?.BaseServiceUrl;
        var tables = new Dictionary<string, EntityMetadata>(StringComparer.Ordinal);
        if (api.BindingType != CustomApi.BindingTypes.Global && baseUrl == null)
            throw new InvalidOperationException("Select a Dataverse environment to resolve the bound table's entity set name. The Custom API does not need to be deployed.");
        var tableNames = (api.BindingType == CustomApi.BindingTypes.Global ? Array.Empty<string?>() : new[] { api.BoundEntityLogicalName })
            .Concat(api.IsFunction && baseUrl != null ? api.RequestParameters.Select(p => p.LogicalEntityName) : Array.Empty<string?>())
            .Where(n => !string.IsNullOrWhiteSpace(n)).Distinct(StringComparer.Ordinal);
        foreach (var logicalName in tableNames)
        {
            var name = CustomApiOpenApiWriter.RequireName(logicalName);
            var metadata = await webApi.RetrieveMultipleAsync<EntityMetadata>(new Uri(baseUrl!,
                "EntityDefinitions?$select=LogicalName,EntitySetName,PrimaryIdAttribute&$filter=LogicalName eq '" + name + "'").AbsoluteUri,
                cancellationToken: token).ConfigureAwait(false);
            tables[name] = metadata.Value.SingleOrDefault() ?? throw new InvalidOperationException("Table metadata not found: " + name);
        }
        token.ThrowIfCancellationRequested();
        var format = await outputSettings.GetFormatAsync().ConfigureAwait(false);
        var version = await outputSettings.GetVersionAsync().ConfigureAwait(false);
        token.ThrowIfCancellationRequested();
        return OpenApiDocumentFormatter.Create(api.UniqueName!, CustomApiOpenApiWriter.Build(api, baseUrl, tables, schemas, version), format);
    }
}
