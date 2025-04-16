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

public interface IXrmMetaDataService
{
    /// <summary>
    /// Parses the input file and returns the PluginAssemblyConfig plus the PluginTypeConfigs and EntityConfigs that are found in the document.
    /// </summary>
    /// <param name="filePath">The full file path to the document to be parsed.</param>
    /// <returns>PluginAssemblyConfig that contains PluginTypeConfigs and EntityConfigs that are found in the document.</returns>
    Task<PluginAssemblyConfig?> ParseAsync(string documentFilePath, CancellationToken cancellationToken = default);

    Task<PluginAssemblyConfig?> ParseProjectAsync(string projectFilePath, CancellationToken cancellationToken = default);
}

[Export(typeof(IXrmMetaDataService))]
[method: ImportingConstructor]
internal class CSharpXrmMetaDataService(
    ILogger<CSharpXrmMetaDataService> logger, 
    ICSharpXrmMetaParser parser,
    IWebApiService webApi) : IXrmMetaDataService
{
    private readonly ILogger<CSharpXrmMetaDataService> Logger = logger ?? throw new ArgumentNullException(nameof(logger));

    private readonly ICSharpXrmMetaParser AttributeConverter = parser ?? throw new ArgumentNullException(nameof(parser));

    public async Task<PluginAssemblyConfig?> ParseAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var document = await FileHelper.GetDocumentAsync(filePath);
        if (document == null)
        {
            return null;
        }

        return await ParseAsync(document, cancellationToken);
    }

    public async Task<PluginAssemblyConfig?> ParseAsync(Document document, CancellationToken cancellationToken = default)
    {
        if (document == null) throw new ArgumentNullException(nameof(document));

        try
        {
            var config = await CreateConfigFromProjectAsync(document.Project, cancellationToken).ConfigureAwait(false);
            if (config == null) return null;

            var processedSymbols = new HashSet<string>();
            var semanticModelCache = new Dictionary<DocumentId, SemanticModel>();

            var pluginTypes = await ParseClassDeclarationsFromDocumentAsync(document, processedSymbols, semanticModelCache, cancellationToken).ConfigureAwait(false);
            pluginTypes.ForEach(config.PluginTypes.Add);

            return config;
        }
        catch (Exception ex)
        {
            // Log or handle the exception as necessary
            throw new InvalidOperationException("An error occurred while retrieving assembly metadata.", ex);
        }
    }

    public async Task<PluginAssemblyConfig?> ParseProjectAsync(string projectFilePath, CancellationToken cancellationToken = default)
    {
        var project = await FileHelper.GetProjectAsync(projectFilePath);
        if (project == null)
        {
            return null;
        }

        return await ParseProjectAsync(project, cancellationToken);
    }

    public async Task<PluginAssemblyConfig?> ParseProjectAsync(Project project, CancellationToken cancellationToken = default)
    {
        if (project == null) throw new ArgumentNullException(nameof(project));

        try
        {
            var config = await CreateConfigFromProjectAsync(project, cancellationToken).ConfigureAwait(false);
            if (config == null) return null;

            var processedSymbols = new HashSet<string>();
            var semanticModelCache = new Dictionary<DocumentId, SemanticModel>();

            var allPluginTypes = new List<PluginTypeConfig>();

            foreach (var document in project.Documents.Where(d => d.SourceCodeKind == SourceCodeKind.Regular))
            {
                var pluginTypes = await ParseClassDeclarationsFromDocumentAsync(document, processedSymbols, semanticModelCache, cancellationToken).ConfigureAwait(false);
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

    private async Task<PluginAssemblyConfig?> CreateConfigFromProjectAsync(Project project, CancellationToken cancellationToken)
    {
        var compilation = await project.GetCompilationAsync(cancellationToken).ConfigureAwait(false);
        if (compilation == null) return null;

        var config = parser.ParsePluginAssemblyConfig(compilation.Assembly);
        if (config is not null)
        {
            config.FilePath = project.OutputFilePath;
        }
        return config;
    }

    private async Task<List<PluginTypeConfig>> ParseClassDeclarationsFromDocumentAsync(
        Document document,
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

            var pluginType = AttributeConverter.ParsePluginConfig(typeSymbol);
            if (pluginType != null)
            {
                result.Add(pluginType);
            }
        }

        return result;
    }
}