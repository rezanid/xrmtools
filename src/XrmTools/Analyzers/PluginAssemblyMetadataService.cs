#nullable enable
namespace XrmTools.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.Xrm.Sdk;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.Helpers;
using XrmTools.Meta.Attributes;
using XrmTools.Meta.Model;
using XrmTools.Xrm.Model;

public interface IPluginAssemblyMetadataService
{
    Task<PluginAssemblyConfig?> GetAssemblyConfigAsync(Document document, CancellationToken cancellationToken = default);
}

public class PluginAssemblyMetadataService : IPluginAssemblyMetadataService
{
    private readonly VisualStudioWorkspace _workspace;
    private readonly IAttributeExtractor _attributeExtractor;

    public PluginAssemblyMetadataService(VisualStudioWorkspace workspace, IAttributeExtractor attributeExtractor)
    {
        _workspace = workspace ?? throw new ArgumentNullException(nameof(workspace));
        _attributeExtractor = attributeExtractor ?? throw new ArgumentNullException(nameof(attributeExtractor));
    }

    public async Task<PluginAssemblyConfig?> GetAssemblyConfigAsync(Document document, CancellationToken cancellationToken = default)
    {
        if (document == null) throw new ArgumentNullException(nameof(document));

        try
        {
            var compilation = await document.Project.GetCompilationAsync(cancellationToken).ConfigureAwait(false);
            if (compilation == null) return null;

            var assemblySymbol = compilation.Assembly;
            var assemblyAttributeData = GetPluginAssemblyAttribute(assemblySymbol);
            if (assemblyAttributeData == null) return null;

            var pluginAssemblyConfig = CreatePluginAssemblyConfig(assemblySymbol, assemblyAttributeData);
            if (pluginAssemblyConfig == null) return null;

            var syntaxTree = await document.GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(false);
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            if (syntaxTree == null || semanticModel == null) return null;

            var root = await syntaxTree.GetRootAsync(cancellationToken).ConfigureAwait(false);
            var classDeclarations = root.DescendantNodes().OfType<ClassDeclarationSyntax>();

            foreach (var classDeclaration in classDeclarations)
            {
                // Get the symbol for the class declaration
                if (semanticModel.GetDeclaredSymbol(classDeclaration) is not INamedTypeSymbol typeSymbol) continue;
                var pluginType = _attributeExtractor.ExtractAttributes(typeSymbol);
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

    private PluginAssemblyConfig CreatePluginAssemblyConfig(IAssemblySymbol assemblySymbol, AttributeData attributeData)
    {
        var pluginAssemblyConfig = new PluginAssemblyConfig
        {
            Name = assemblySymbol.Name,
            Version = assemblySymbol.Identity.Version.ToString(),
            PublicKeyToken = assemblySymbol.Identity.PublicKeyToken.ToHexString(),
            SourceType = attributeData.GetValue<int?>(nameof(PluginAssemblyAttribute.SourceType)) is int sourceType
                ? (SourceTypes)sourceType : PluginAssemblyAttribute.DefaultSourceType,
            IsolationMode = attributeData.GetValue<int?>(nameof(PluginAssemblyAttribute.IsolationMode)) is int isolationMode
                ? (IsolationModes)isolationMode : PluginAssemblyAttribute.DefaultIsolationMode
        };

        if (attributeData.GetValue<string>(nameof(PluginAssemblyAttribute.Id)) is string pluginAssemblyId)
        {
            pluginAssemblyConfig.PluginAssemblyId = Guid.Parse(pluginAssemblyId);
        }

        if (attributeData.GetValue<string>(nameof(PluginAssemblyAttribute.SolutionId)) is string solutionId)
        {
            pluginAssemblyConfig.SolutionId = Guid.Parse(solutionId);// new EntityReference("solutions", Guid.Parse(solutionId));
        }

        return pluginAssemblyConfig;
    }

    private AttributeData? GetPluginAssemblyAttribute(IAssemblySymbol assemblySymbol)
        => assemblySymbol.GetAttributes()
            .SingleOrDefault(attr => attr.AttributeClass?.ToDisplayString() == typeof(PluginAssemblyAttribute).FullName);
}