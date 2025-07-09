#nullable enable
namespace XrmTools.Analyzers;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.Helpers;
using XrmTools.Xrm.Model;
using XrmTools.Logging.Compatibility;
using System.ComponentModel.Composition;
using XrmTools.WebApi;

internal interface IXrmMetaDataService
{
    /// <summary>
    /// Parses the input file and returns the PluginAssemblyConfig plus the PluginTypeConfigs and EntityConfigs that are found in the document.
    /// </summary>
    /// <param name="filePath">The full file path to the document to be parsed.</param>
    /// <returns>PluginAssemblyConfig that contains PluginTypeConfigs and EntityConfigs that are found in the document.</returns>
    Task<PluginAssemblyConfig?> ParsePluginsAsync(string documentFilePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Parses the project file and returns the PluginAssemblyConfig plus the PluginTypeConfigs (but not EntityConfigs) that are found in the project.
    /// </summary>
    /// <param name="projectFilePath"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<PluginAssemblyConfig?> ParseProjectPluginsAsync(string projectFilePath, CancellationToken cancellationToken = default);
    Task<PluginAssemblyConfig?> ParseEntitiesAsync(string filePath, CancellationToken cancellationToken = default);
}

[Export(typeof(IXrmMetaDataService))]
[method: ImportingConstructor]
internal class CSharpXrmMetaDataService(
    ILogger<CSharpXrmMetaDataService> logger, 
    ICSharpXrmMetaParser parser,
    IWebApiService webApi) : IXrmMetaDataService
{
    private readonly ILogger<CSharpXrmMetaDataService> Logger = logger ?? throw new ArgumentNullException(nameof(logger));

    private readonly ICSharpXrmMetaParser AttributeParser = parser ?? throw new ArgumentNullException(nameof(parser));

    public async Task<PluginAssemblyConfig?> ParsePluginsAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var document = await FileHelper.GetDocumentAsync(filePath);
        if (document == null)
        {
            return null;
        }

        return await ParsePluginsAsync(document, cancellationToken);
    }

    public async Task<PluginAssemblyConfig?> ParsePluginsAsync(Document document, CancellationToken cancellationToken = default)
    {
        if (document == null) throw new ArgumentNullException(nameof(document));

        try
        {
            var config = await ParseConfigFromProjectAsync(document.Project, cancellationToken).ConfigureAwait(false);
            if (config == null) return null;

            var processedSymbols = new HashSet<string>();
            var semanticModelCache = new Dictionary<DocumentId, SemanticModel>();

            var compilation = await document.Project.GetCompilationAsync(cancellationToken).ConfigureAwait(false);
            if (compilation == null) return null;
            var pluginTypes = await ParsePluginConfigsFromDocumentAsync(document, compilation, processedSymbols, semanticModelCache, cancellationToken).ConfigureAwait(false);
            pluginTypes.ForEach(config.PluginTypes.Add);

            return config;
        }
        catch (Exception ex)
        {
            // Log or handle the exception as necessary
            throw new InvalidOperationException("An error occurred while retrieving assembly metadata.", ex);
        }
    }

    public async Task<PluginAssemblyConfig?> ParseEntitiesAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var document = await FileHelper.GetDocumentAsync(filePath);
        if (document == null)
        {
            return null;
        }

        return await ParseEntitiesAsync(document, cancellationToken);
    }

    public async Task<PluginAssemblyConfig?> ParseEntitiesAsync(Document document, CancellationToken cancellationToken = default)
    {
        if (document == null) throw new ArgumentNullException(nameof(document));

        try
        {
            var config = await ParseConfigFromProjectAsync(document.Project, cancellationToken).ConfigureAwait(false);
            if (config == null) return null;

            var compilation = await document.Project.GetCompilationAsync(cancellationToken).ConfigureAwait(false);
            if (compilation == null) return null;
            var entityConfigs = await ParseEntityAttributesFromDocumentAsync(document, compilation, cancellationToken).ConfigureAwait(false);
            config.Entities = entityConfigs;
            return config;
        }
        catch (Exception ex)
        {
            // Log or handle the exception as necessary
            throw new InvalidOperationException("An error occurred while retrieving assembly metadata.", ex);
        }
    }

    public async Task<PluginAssemblyConfig?> ParseProjectPluginsAsync(string projectFilePath, CancellationToken cancellationToken = default)
    {
        var project = await FileHelper.GetProjectAsync(projectFilePath);
        if (project == null)
        {
            return null;
        }

        return await ParseProjectPluginsAsync(project, cancellationToken);
    }

    public async Task<PluginAssemblyConfig?> ParseProjectPluginsAsync(Project project, CancellationToken cancellationToken = default)
    {
        if (project == null) throw new ArgumentNullException(nameof(project));

        try
        {
            var config = await ParseConfigFromProjectAsync(project, cancellationToken).ConfigureAwait(false);
            if (config == null) return null;

            var processedSymbols = new HashSet<string>();
            var semanticModelCache = new Dictionary<DocumentId, SemanticModel>();

            var allPluginTypes = new List<PluginTypeConfig>();

            var compilation = await project.GetCompilationAsync(cancellationToken).ConfigureAwait(false);

            foreach (var document in project.Documents.Where(d => d.SourceCodeKind == SourceCodeKind.Regular))
            {
                var pluginTypes = await ParsePluginConfigsFromDocumentAsync(document, compilation, processedSymbols, semanticModelCache, cancellationToken).ConfigureAwait(false);
                allPluginTypes.AddRange(pluginTypes);
            }

            allPluginTypes.ForEach(config.PluginTypes.Add);

            return config;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An error occurred while parsing the project metadata.", ex);
        }
    }

    private async Task<PluginAssemblyConfig?> ParseConfigFromProjectAsync(Project project, CancellationToken cancellationToken)
    {
        var compilation = await project.GetCompilationAsync(cancellationToken).ConfigureAwait(false);
        if (compilation == null) return null;

        var config = parser.ParsePluginAssemblyConfig(compilation);
        if (config is not null)
        {
            config.FilePath = project.OutputFilePath;
        }
        return config;
    }

    private async Task<List<PluginTypeConfig>> ParsePluginConfigsFromDocumentAsync(
        Document document,
        Compilation compilation,
        HashSet<string> processedSymbols,
        Dictionary<DocumentId, SemanticModel> semanticModelCache,
        CancellationToken cancellationToken)
    {
        var result = new List<PluginTypeConfig>();

        var syntaxTree = await document.GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(false);
        if (syntaxTree == null) return result;

        if (!semanticModelCache.TryGetValue(document.Id, out var semanticModel))
        {
            semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            if (semanticModel == null) return result;

            semanticModelCache[document.Id] = semanticModel;
        }

        var root = await syntaxTree.GetRootAsync(cancellationToken).ConfigureAwait(false);
        //var usingDirectives = root.DescendantNodes().OfType<UsingDirectiveSyntax>()
        //    .Select(u => u.ToString());
        var classDeclarations = root.DescendantNodes().OfType<ClassDeclarationSyntax>();

        foreach (var classDeclaration in classDeclarations)
        {
            if (semanticModel.GetDeclaredSymbol(classDeclaration) is not INamedTypeSymbol typeSymbol)
                continue;

            var typeKey = typeSymbol.ToDisplayString();

            if (!processedSymbols.Add(typeKey))
                continue;

            var pluginType = AttributeParser.ParsePluginConfig(typeSymbol, compilation);
            if (pluginType != null)
            {
                result.Add(pluginType);
                pluginType.IsNullableEnabled = semanticModel.GetNullableContext(classDeclaration.SpanStart).AnnotationsEnabled();
            }
        }

        return result;
    }

    private async Task<List<EntityConfig>> ParseEntityAttributesFromDocumentAsync(
        Document document,
        Compilation compilation,
        CancellationToken cancellationToken)
    {
        var syntaxTree = await document.GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(false);
        if (syntaxTree == null) return [];

        var entityConfigs = new List<EntityConfig>();
        var assemblyAttributes = compilation.Assembly.GetAttributes();
        foreach (var assemblyAttribute in assemblyAttributes)
        {
            if (syntaxTree == assemblyAttribute.ApplicationSyntaxReference!.SyntaxTree)
            {
                entityConfigs.Add(parser.ParseEntityConfig(assemblyAttribute));
            }
        }
        return entityConfigs;
    }
}