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
using System.ComponentModel.Composition;
using XrmTools.Meta.Model.Configuration;
using XrmTools.Meta.Attributes;

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
internal class CSharpXrmMetaDataService(ICSharpXrmMetaParser parser) : IXrmMetaDataService
{
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

            var compilation = await document.Project.GetCompilationAsync(cancellationToken).ConfigureAwait(false);
            if (compilation == null) return null;

            var pluginTypesByDocument = await ParseProjectPluginConfigsByDocumentAsync(document.Project, compilation, cancellationToken).ConfigureAwait(false);
            var allPluginTypes = pluginTypesByDocument.SelectMany(x => x.Value).ToList();
            ValidateCustomApiUniqueNames(allPluginTypes);

            if (pluginTypesByDocument.TryGetValue(document.Id, out var pluginTypes))
            {
                pluginTypes.ForEach(config.PluginTypes.Add);
            }

            config.OtherPluginTypes = allPluginTypes
                .Where(p => !config.PluginTypes.Any(currentPlugin => currentPlugin.TypeName == p.TypeName))
                .Select(p => new PluginTypeConfig { TypeName = p.TypeName })
                .ToList();

            config.AssemblyPluginTypeNames = GetAssemblyPluginTypeNames(compilation);

            return config;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
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
            var (documentEntities, otherEntities) = await ParseEntityAttributesFromDocumentAsync(document, compilation, cancellationToken).ConfigureAwait(false);
            config.Entities = documentEntities;
            config.OtherEntities = otherEntities;
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

            var compilation = await project.GetCompilationAsync(cancellationToken).ConfigureAwait(false);
            if (compilation == null) return null;

            var pluginTypesByDocument = await ParseProjectPluginConfigsByDocumentAsync(project, compilation, cancellationToken).ConfigureAwait(false);
            var allPluginTypes = pluginTypesByDocument.SelectMany(x => x.Value).ToList();
            ValidateCustomApiUniqueNames(allPluginTypes);

            allPluginTypes.ForEach(config.PluginTypes.Add);

            config.AssemblyPluginTypeNames = GetAssemblyPluginTypeNames(compilation);

            return config;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An error occurred while parsing the project metadata.", ex);
        }
    }

    private static void ValidateCustomApiUniqueNames(IEnumerable<PluginTypeConfig> pluginTypes)
    {
        var duplicateCustomApis = pluginTypes
            .Where(pluginType => !string.IsNullOrWhiteSpace(pluginType.CustomApi?.UniqueName))
            .GroupBy(pluginType => pluginType.CustomApi!.UniqueName!, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Skip(1).Any())
            .Select(group => $"'{group.Key}' ({string.Join(", ", group.Select(pluginType => pluginType.TypeName).OrderBy(typeName => typeName, StringComparer.Ordinal))})")
            .OrderBy(x => x, StringComparer.Ordinal)
            .ToList();

        if (duplicateCustomApis.Count > 0)
        {
            throw new InvalidOperationException($"Duplicate Custom API unique names were found: {string.Join("; ", duplicateCustomApis)}.");
        }
    }

    /// <summary>
    /// Enumerates every plugin type compiled into the assembly (types implementing
    /// <c>Microsoft.Xrm.Sdk.IPlugin</c>, directly or through a base class), regardless of whether they
    /// carry XrmTools attributes. Returns <see langword="null"/> when <c>IPlugin</c> cannot be resolved
    /// in the compilation, so callers can conservatively avoid deleting plugin types.
    /// </summary>
    private static ISet<string>? GetAssemblyPluginTypeNames(Compilation compilation)
    {
        var pluginInterface = compilation.GetTypeByMetadataName("Microsoft.Xrm.Sdk.IPlugin");
        if (pluginInterface is null)
        {
            return null;
        }

        var names = new HashSet<string>(StringComparer.Ordinal);
        foreach (var type in EnumerateNamedTypes(compilation.Assembly.GlobalNamespace))
        {
            if (type.TypeKind != TypeKind.Class || type.IsAbstract || type.IsStatic)
            {
                continue;
            }

            if (type.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, pluginInterface)))
            {
                names.Add(type.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));
            }
        }

        return names;
    }

    private static IEnumerable<INamedTypeSymbol> EnumerateNamedTypes(INamespaceSymbol root)
    {
        foreach (var type in root.GetTypeMembers())
        {
            foreach (var nested in EnumerateNamedTypesCore(type))
            {
                yield return nested;
            }
        }

        foreach (var childNamespace in root.GetNamespaceMembers())
        {
            foreach (var type in EnumerateNamedTypes(childNamespace))
            {
                yield return type;
            }
        }
    }

    private static IEnumerable<INamedTypeSymbol> EnumerateNamedTypesCore(INamedTypeSymbol type)
    {
        yield return type;
        foreach (var nested in type.GetTypeMembers())
        {
            foreach (var inner in EnumerateNamedTypesCore(nested))
            {
                yield return inner;
            }
        }
    }

    private async Task<Dictionary<DocumentId, List<PluginTypeConfig>>> ParseProjectPluginConfigsByDocumentAsync(
        Project project,
        Compilation compilation,
        CancellationToken cancellationToken)
    {
        var processedSymbols = new HashSet<string>();
        var semanticModelCache = new Dictionary<DocumentId, SemanticModel>();
        var pluginTypesByDocument = new Dictionary<DocumentId, List<PluginTypeConfig>>();

        foreach (var projectDocument in project.Documents.Where(
            d => 
                d.SourceCodeKind == SourceCodeKind.Regular && 
                d.FilePath != null &&
                !d.FilePath.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase) &&
                !d.FilePath.EndsWith(".generated.cs", StringComparison.OrdinalIgnoreCase) &&
                !d.FilePath.EndsWith(".designer.cs", StringComparison.OrdinalIgnoreCase) &&
                d.FilePath.IndexOf("\\xrmtools.meta.attributes\\", StringComparison.OrdinalIgnoreCase) < 0))
        {
            var pluginTypes = await ParsePluginConfigsFromDocumentAsync(projectDocument, compilation, processedSymbols, semanticModelCache, cancellationToken).ConfigureAwait(false);
            pluginTypesByDocument[projectDocument.Id] = pluginTypes;
        }

        return pluginTypesByDocument;
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
        var syntaxTree = await document.GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(false);
        if (syntaxTree == null) return [];

        if (!semanticModelCache.TryGetValue(document.Id, out var semanticModel))
        {
            semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            if (semanticModel == null) return [];

            semanticModelCache[document.Id] = semanticModel;
        }

        var root = await syntaxTree.GetRootAsync(cancellationToken).ConfigureAwait(false);
        //var usingDirectives = root.DescendantNodes().OfType<UsingDirectiveSyntax>()
        //    .Select(u => u.ToString());
        var classDeclarations = root.DescendantNodes().OfType<ClassDeclarationSyntax>();

        var result = new List<PluginTypeConfig>();

        foreach (var classDeclaration in classDeclarations)
        {
            if (semanticModel.GetDeclaredSymbol(classDeclaration) is not INamedTypeSymbol typeSymbol)
                continue;

            var typeKey = typeSymbol.ToDisplayString();

            if (processedSymbols.Contains(typeKey))
                continue;

            var pluginType = AttributeParser.ParsePluginConfig(typeSymbol, compilation);
            if (pluginType == null)
                continue;

            processedSymbols.Add(typeKey);
            result.Add(pluginType);
            pluginType.IsNullableEnabled = semanticModel.GetNullableContext(classDeclaration.SpanStart).AnnotationsEnabled();
        }

        return result;
    }

    private async Task<(List<EntityConfig> documentEntities, List<EntityConfig> otherEntities)> ParseEntityAttributesFromDocumentAsync(
        Document document,
        Compilation compilation,
        CancellationToken cancellationToken)
    {
        var syntaxTree = await document.GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(false);
        if (syntaxTree == null) return ([], []);

        var documentEntities = new List<EntityConfig>();
        var otherEntities = new List<EntityConfig>();
        var assemblyAttributes = compilation.Assembly.GetAttributes();
        foreach (var assemblyAttribute in assemblyAttributes)
        {
            if (assemblyAttribute.AttributeClass?.ToDisplayString() == typeof(EntityAttribute).FullName && syntaxTree == assemblyAttribute.ApplicationSyntaxReference!.SyntaxTree)
            {
                documentEntities.Add(parser.ParseEntityConfig(assemblyAttribute));
            }
            else if (assemblyAttribute.AttributeClass?.ToDisplayString() == typeof(EntityAttribute).FullName)
            {
                otherEntities.Add(parser.ParseEntityConfig(assemblyAttribute));
            }
        }
        return (documentEntities, otherEntities);
    }
}