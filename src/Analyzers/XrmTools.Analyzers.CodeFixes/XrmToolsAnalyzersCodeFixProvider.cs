#nullable enable
namespace XrmTools.Analyzers;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Immutable;
using System.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(XrmToolsAnalyzersCodeFixProvider)), Shared]
public class XrmToolsAnalyzersCodeFixProvider : CodeFixProvider
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds =>
        [
            PluginDependencyAnalyzer.MissingDependencyAttributeId,
            PluginDependencyAnalyzer.HasSetterOrInitId,
            ProxyTypesAssemblyAnalyzer.MissingProxyTypesAssemblyAttributeId,
        ];

    public sealed override FixAllProvider GetFixAllProvider() =>
        new ProxyTypesFixAllProvider();

    private sealed class ProxyTypesFixAllProvider : FixAllProvider
    {
        public override async Task<CodeAction?> GetFixAsync(FixAllContext fixAllContext)
        {
            if (fixAllContext.DiagnosticIds.Contains(ProxyTypesAssemblyAnalyzer.MissingProxyTypesAssemblyAttributeId))
            {
                return CodeAction.Create(
                    title: "Add [assembly: ProxyTypesAssemblyAttribute]",
                    createChangedSolution: c => AddProxyTypesAssemblyAttributeFileAsync(fixAllContext.Project, c),
                    equivalenceKey: "AddProxyTypesAssemblyAttribute");
            }

            return await WellKnownFixAllProviders.BatchFixer.GetFixAsync(fixAllContext).ConfigureAwait(false);
        }
    }

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var diagnostic = context.Diagnostics.First();

        if (diagnostic.Id == ProxyTypesAssemblyAnalyzer.MissingProxyTypesAssemblyAttributeId)
        {
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Add [assembly: ProxyTypesAssemblyAttribute]",
                    createChangedSolution: c => AddProxyTypesAssemblyAttributeFileAsync(context.Document.Project, c),
                    equivalenceKey: "AddProxyTypesAssemblyAttribute"),
                diagnostic);
            return;
        }

        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null)
            return;

        var diagnosticSpan = diagnostic.Location.SourceSpan;

        // Find the property declaration
        var propertyDecl = root
            .FindToken(diagnosticSpan.Start)
            .Parent
            ?.AncestorsAndSelf()
            .OfType<PropertyDeclarationSyntax>()
            .FirstOrDefault();

        if (propertyDecl is null)
            return;

        if (diagnostic.Id == PluginDependencyAnalyzer.MissingDependencyAttributeId)
        {
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Add [Dependency] attribute",
                    createChangedDocument: c => AddDependencyAttributeAsync(context.Document, propertyDecl, c),
                    equivalenceKey: "AddDependencyAttribute"),
                diagnostic);
        }
        else if (diagnostic.Id == PluginDependencyAnalyzer.HasSetterOrInitId)
        {
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Remove setter/init accessor",
                    createChangedDocument: c => RemoveSetterOrInitAsync(context.Document, propertyDecl, c),
                    equivalenceKey: "RemoveSetterOrInit"),
                diagnostic);
        }
    }

    private static async Task<Solution> AddProxyTypesAssemblyAttributeFileAsync(Project project, CancellationToken cancellationToken)
    {
        var compilation = await project.GetCompilationAsync(cancellationToken).ConfigureAwait(false);
        if (compilation is null)
            return project.Solution;

        var proxyAttrSymbol = compilation.GetTypeByMetadataName("Microsoft.Xrm.Sdk.Client.ProxyTypesAssemblyAttribute");
        if (proxyAttrSymbol is null)
            return project.Solution;

        if (HasProxyTypesAssemblyAttribute(compilation, proxyAttrSymbol))
            return project.Solution;

        var documentName = GetUniqueProxyTypesAssemblyInfoName(project);

        var sourceText = CreateProxyTypesAssemblyInfoSourceText(
            fullyQualifiedAttributeName: "global::Microsoft.Xrm.Sdk.Client.ProxyTypesAssemblyAttribute");

        var filePath = GetNewDocumentFilePath(project, documentName);

        var newDocId = DocumentId.CreateNewId(project.Id);
        var newSolution = project.Solution.AddDocument(newDocId, documentName, sourceText, filePath: filePath);

        return newSolution;
    }

    private static bool HasProxyTypesAssemblyAttribute(Compilation compilation, INamedTypeSymbol proxyAttrSymbol)
    {
        foreach (var attr in compilation.Assembly.GetAttributes())
        {
            if (SymbolEqualityComparer.Default.Equals(attr.AttributeClass, proxyAttrSymbol))
                return true;
        }

        return false;
    }

    private static string GetUniqueProxyTypesAssemblyInfoName(Project project)
    {
        const string baseName = "ProxyTypesAssemblyInfo.cs";
        if (!project.Documents.Any(d => d.Name.Equals(baseName, StringComparison.OrdinalIgnoreCase)))
            return baseName;

        for (var i = 1; ; i++)
        {
            var candidate = $"ProxyTypesAssemblyInfo{i}.cs";
            if (!project.Documents.Any(d => d.Name.Equals(candidate, StringComparison.OrdinalIgnoreCase)))
                return candidate;
        }
    }

    private static SourceText CreateProxyTypesAssemblyInfoSourceText(string fullyQualifiedAttributeName)
    {
        // Keep it simple; no need to build syntax tree here.
        var text =
    $@"// <auto-generated />
[assembly: {fullyQualifiedAttributeName}]
";
        return SourceText.From(text);
    }

    private static string? GetNewDocumentFilePath(Project project, string documentName)
    {
        if (project.FilePath is null)
            return null;

        var projectDir = Path.GetDirectoryName(project.FilePath);
        if (string.IsNullOrWhiteSpace(projectDir))
            return null;

        return Path.Combine(projectDir, documentName);
    }

    private static async Task<Document> AddDependencyAttributeAsync(
        Document document,
        PropertyDeclarationSyntax propertyDecl,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is null)
            return document;

        // Create the [XrmTools.Meta.Attributes.Dependency] attribute
        var attribute = SyntaxFactory.Attribute(
            SyntaxFactory.ParseName("XrmTools.Meta.Attributes.Dependency"));

        var attributeList = SyntaxFactory.AttributeList(
            SyntaxFactory.SingletonSeparatedList(attribute));

        // Add the attribute to the property
        var newPropertyDecl = propertyDecl.AddAttributeLists(attributeList);

        var newRoot = root.ReplaceNode(propertyDecl, newPropertyDecl);
        return document.WithSyntaxRoot(newRoot);
    }

    private static async Task<Document> RemoveSetterOrInitAsync(
        Document document,
        PropertyDeclarationSyntax propertyDecl,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is null)
            return document;

        if (propertyDecl.AccessorList is null)
            return document;

        // Keep only get accessors, remove set/init
        var accessors = propertyDecl.AccessorList.Accessors
            .Where(a => a.IsKind(SyntaxKind.GetAccessorDeclaration))
            .ToList();

        if (accessors.Count == 0)
            return document;

        // Preserve opening and closing braces trivia from the original accessor list
        var newAccessorList = SyntaxFactory.AccessorList(
            propertyDecl.AccessorList.OpenBraceToken,
            SyntaxFactory.List(accessors),
            propertyDecl.AccessorList.CloseBraceToken);

        var newPropertyDecl = propertyDecl.WithAccessorList(newAccessorList);

        var newRoot = root.ReplaceNode(propertyDecl, newPropertyDecl);
        return document.WithSyntaxRoot(newRoot);
    }
}
#nullable restore
