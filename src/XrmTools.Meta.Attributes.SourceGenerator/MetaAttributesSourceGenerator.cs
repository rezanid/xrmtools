#nullable enable
namespace XrmTools.Meta.Attributes.SourceGenerator;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

[Generator(LanguageNames.CSharp)]
public sealed class MetaAttributesSourceGenerator : IIncrementalGenerator
{
    private const string UsePublicAccessibilityPropertyName = "build_property.XrmToolsMetaAttributesUsePublicAccessibility";
    private const string ResourcePrefix = "XrmTools.Meta.Attributes.Sources.";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValueProvider<bool> usePublicAccessibility = context.AnalyzerConfigOptionsProvider
            .Select((options, _) => UsePublicAccessibility(options.GlobalOptions));

        IncrementalValueProvider<GeneratorOptions> options = context.CompilationProvider
            .Combine(usePublicAccessibility)
            .Select((pair, _) => new GeneratorOptions(pair.Right, ShouldEmitDependencyScope(pair.Left)));

        IncrementalValueProvider<ImmutableArray<EmbeddedSourceFile>> sourceFiles = options
            .Select((generatorOptions, _) => LoadEmbeddedSources(generatorOptions));

        context.RegisterSourceOutput(sourceFiles, static (productionContext, sources) =>
        {
            foreach (EmbeddedSourceFile source in sources)
            {
                productionContext.AddSource(source.HintName, source.Content);
            }
        });
    }

    private static bool UsePublicAccessibility(AnalyzerConfigOptions globalOptions)
    {
        if (!globalOptions.TryGetValue(UsePublicAccessibilityPropertyName, out string? value))
        {
            return false;
        }

        return bool.TryParse(value, out bool usePublicAccessibility) && usePublicAccessibility;
    }

    private static bool ShouldEmitDependencyScope(Compilation compilation)
        => compilation.GetTypeByMetadataName("Microsoft.Xrm.Sdk.IPlugin") is not null;

    private static ImmutableArray<EmbeddedSourceFile> LoadEmbeddedSources(GeneratorOptions options)
    {
        System.Reflection.Assembly assembly = typeof(MetaAttributesSourceGenerator).Assembly;

        IEnumerable<string> resourceNames = assembly.GetManifestResourceNames()
            .Where(static name => name.StartsWith(ResourcePrefix, StringComparison.Ordinal) && name.EndsWith(".cs", StringComparison.Ordinal))
            .OrderBy(static name => name, StringComparer.Ordinal);

        ImmutableArray<EmbeddedSourceFile>.Builder builder = ImmutableArray.CreateBuilder<EmbeddedSourceFile>();

        foreach (string resourceName in resourceNames)
        {
            if (!options.EmitDependencyScope && resourceName.EndsWith("DependencyScope.cs", StringComparison.Ordinal))
            {
                continue;
            }

            using Stream? stream = assembly.GetManifestResourceStream(resourceName);
            if (stream is null)
            {
                continue;
            }

            using StreamReader reader = new(stream);
            string content = reader.ReadToEnd();
            builder.Add(new EmbeddedSourceFile(CreateHintName(resourceName), RewriteAccessibility(content, options.UsePublicAccessibility)));
        }

        return builder.ToImmutable();
    }

    private static string RewriteAccessibility(string content, bool usePublicAccessibility)
    {
        if (!usePublicAccessibility)
        {
            return content;
        }

        SyntaxNode root = CSharpSyntaxTree.ParseText(content).GetRoot();
        SyntaxNode rewrittenRoot = new AccessibilityRewriter().Visit(root);

        return rewrittenRoot.ToFullString();
    }

    private static string CreateHintName(string resourceName)
    {
        string relativeName = resourceName.Substring(ResourcePrefix.Length);
        return relativeName.Replace(Path.DirectorySeparatorChar, '_').Replace(Path.AltDirectorySeparatorChar, '_');
    }

    private sealed class AccessibilityRewriter : CSharpSyntaxRewriter
    {
        private int typeDepth;

        public override SyntaxNode? VisitClassDeclaration(ClassDeclarationSyntax node)
            => RewriteType(node, static (currentNode, modifiers) => currentNode.WithModifiers(modifiers));

        public override SyntaxNode? VisitRecordDeclaration(RecordDeclarationSyntax node)
            => RewriteType(node, static (currentNode, modifiers) => currentNode.WithModifiers(modifiers));

        public override SyntaxNode? VisitStructDeclaration(StructDeclarationSyntax node)
            => RewriteType(node, static (currentNode, modifiers) => currentNode.WithModifiers(modifiers));

        public override SyntaxNode? VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
            => RewriteType(node, static (currentNode, modifiers) => currentNode.WithModifiers(modifiers));

        public override SyntaxNode? VisitEnumDeclaration(EnumDeclarationSyntax node)
            => RewriteType(node, static (currentNode, modifiers) => currentNode.WithModifiers(modifiers));

        public override SyntaxNode? VisitDelegateDeclaration(DelegateDeclarationSyntax node)
        {
            if (typeDepth != 0)
            {
                return base.VisitDelegateDeclaration(node);
            }

            return base.VisitDelegateDeclaration(node.WithModifiers(RewriteModifiers(node.Modifiers)));
        }

        private TNode RewriteType<TNode>(TNode node, Func<TNode, SyntaxTokenList, TNode> update) where TNode : BaseTypeDeclarationSyntax
        {
            bool isTopLevelType = typeDepth == 0;
            TNode currentNode = isTopLevelType ? update(node, RewriteModifiers(node.Modifiers)) : node;

            typeDepth++;

            SyntaxNode? visitedNode = base.Visit(currentNode);

            typeDepth--;

            return (TNode)visitedNode!;
        }

        private static SyntaxTokenList RewriteModifiers(SyntaxTokenList modifiers)
        {
            int internalIndex = modifiers.IndexOf(SyntaxKind.InternalKeyword);
            if (internalIndex < 0)
            {
                return modifiers;
            }

            SyntaxToken publicToken = SyntaxFactory.Token(modifiers[internalIndex].LeadingTrivia, SyntaxKind.PublicKeyword, modifiers[internalIndex].TrailingTrivia);

            return modifiers.Replace(modifiers[internalIndex], publicToken);
        }
    }

    private readonly struct EmbeddedSourceFile
    {
        public EmbeddedSourceFile(string hintName, string content)
        {
            HintName = hintName;
            Content = content;
        }

        public string HintName { get; }

        public string Content { get; }
    }

    private readonly struct GeneratorOptions
    {
        public GeneratorOptions(bool usePublicAccessibility, bool emitDependencyScope)
        {
            UsePublicAccessibility = usePublicAccessibility;
            EmitDependencyScope = emitDependencyScope;
        }

        public bool UsePublicAccessibility { get; }

        public bool EmitDependencyScope { get; }
    }
}
