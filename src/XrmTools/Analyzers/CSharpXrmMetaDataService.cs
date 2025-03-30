#nullable enable
namespace XrmTools.Analyzers;

using Community.VisualStudio.Toolkit;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.Helpers;
using XrmTools.Meta.Attributes;
using XrmTools.Meta.Model;
using XrmTools.Resources;
using XrmTools.Xrm.Model;
using XrmTools.Logging.Compatibility;
using System.ComponentModel.Composition;

public interface IXrmMetaDataService
{
    /// <summary>
    /// Parses the input file and returns the PluginAssemblyConfig plus the PluginTypeConfigs and EntityConfigs that are found in the document.
    /// </summary>
    /// <param name="filePath">The full file path to the document to be parsed.</param>
    /// <returns>PluginAssemblyConfig that contains PluginTypeConfigs and EntityConfigs that are found in the document.</returns>
    Task<PluginAssemblyConfig?> ParseAsync(string inputFileName, CancellationToken cancellationToken = default);
}

[Export(typeof(IXrmMetaDataService))]
[method: ImportingConstructor]
public class CSharpXrmMetaDataService(
    ILogger<CSharpXrmMetaDataService> logger, 
    IAttributeConverter attributeConverter) : IXrmMetaDataService
{
    private readonly ILogger<CSharpXrmMetaDataService> Logger = logger ?? throw new ArgumentNullException(nameof(logger));

    private readonly IAttributeConverter AttributeConverter = attributeConverter ?? throw new ArgumentNullException(nameof(attributeConverter));

    public async Task<PluginAssemblyConfig?> ParseAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var proj = await VS.Solutions.GetActiveProjectAsync();
        if (proj is null)
        {
            Logger.LogWarning(Strings.NoActiveProject);
            return null;
        }

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
            var compilation = await document.Project.GetCompilationAsync(cancellationToken).ConfigureAwait(false);
            if (compilation == null) return null;

            var assemblySymbol = compilation.Assembly;
            var assemblyAttribute = GetPluginAssemblyAttribute(assemblySymbol);
            if (assemblyAttribute == null) return null;

            var assemblyEntityAttributes = GetAssemblyEntityAttributes(assemblySymbol);

            var pluginAssemblyConfig = CreatePluginAssemblyConfig(assemblySymbol, assemblyAttribute, assemblyEntityAttributes);
            if (pluginAssemblyConfig == null) return null;
            pluginAssemblyConfig.FilePath = document.Project.OutputFilePath;

            var syntaxTree = await document.GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(false);
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            if (syntaxTree == null || semanticModel == null) return null;

            var root = await syntaxTree.GetRootAsync(cancellationToken).ConfigureAwait(false);
            var classDeclarations = root.DescendantNodes().OfType<ClassDeclarationSyntax>();

            foreach (var classDeclaration in classDeclarations)
            {
                // Get the symbol for the class declaration
                if (semanticModel.GetDeclaredSymbol(classDeclaration) is not INamedTypeSymbol typeSymbol) continue;
                var pluginType = AttributeConverter.ConvertPluginAttributes(typeSymbol);
                if (pluginType != null)
                {
                    pluginAssemblyConfig.PluginTypes.Add(pluginType);
                }
            }
            return pluginAssemblyConfig;
        }
        catch (Exception ex)
        {
            // Log or handle the exception as necessary
            throw new InvalidOperationException("An error occurred while retrieving assembly metadata.", ex);
        }
    }

    private PluginAssemblyConfig CreatePluginAssemblyConfig(
        IAssemblySymbol assemblySymbol, AttributeData assemblyAttribute, IEnumerable<AttributeData> entityAttributes)
    {
        var pluginAssemblyConfig = new PluginAssemblyConfig
        {
            Name = assemblySymbol.Name,
            Version = assemblySymbol.Identity.Version.ToString(),
            PublicKeyToken = assemblySymbol.Identity.PublicKeyToken.ToHexString(),
            SourceType = assemblyAttribute.GetValue<int?>(nameof(PluginAssemblyAttribute.SourceType)) is int sourceType
                ? (SourceTypes)sourceType : PluginAssemblyAttribute.DefaultSourceType,
            IsolationMode = assemblyAttribute.GetValue<int?>(nameof(PluginAssemblyAttribute.IsolationMode)) is int isolationMode
                ? (IsolationModes)isolationMode : PluginAssemblyAttribute.DefaultIsolationMode,
            Entities = AttributeConverter.ConvertEntityAttributes(entityAttributes).ToList(),
        };

        if (assemblyAttribute.GetValue<string>(nameof(PluginAssemblyAttribute.Id)) is string pluginAssemblyId)
        {
            pluginAssemblyConfig.PluginAssemblyId = Guid.Parse(pluginAssemblyId);
        }

        if (assemblyAttribute.GetValue<string>(nameof(PluginAssemblyAttribute.SolutionId)) is string solutionId)
        {
            pluginAssemblyConfig.SolutionId = Guid.Parse(solutionId);// new EntityReference("solutions", Guid.Parse(solutionId));
        }

        return pluginAssemblyConfig;
    }

    private AttributeData? GetPluginAssemblyAttribute(IAssemblySymbol assemblySymbol)
        => assemblySymbol.GetAttributes()
            .SingleOrDefault(attr => attr.AttributeClass?.ToDisplayString() == typeof(PluginAssemblyAttribute).FullName);

    private IEnumerable<AttributeData> GetAssemblyEntityAttributes(IAssemblySymbol assemblySymbol)
        => [.. assemblySymbol.GetAttributes().Where(attr => attr.AttributeClass?.ToDisplayString() == typeof(EntityAttribute).FullName)];
}