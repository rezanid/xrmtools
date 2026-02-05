#nullable enable
namespace XrmTools.Analyzers;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(XrmToolsAnalyzersCodeFixProvider)), Shared]
public class XrmToolsAnalyzersCodeFixProvider : CodeFixProvider
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(
            PluginDependencyAnalyzer.MissingDependencyAttributeId,
            PluginDependencyAnalyzer.HasSetterOrInitId);

    public sealed override FixAllProvider GetFixAllProvider() =>
        WellKnownFixAllProviders.BatchFixer;

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null)
            return;

        var diagnostic = context.Diagnostics.First();
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
