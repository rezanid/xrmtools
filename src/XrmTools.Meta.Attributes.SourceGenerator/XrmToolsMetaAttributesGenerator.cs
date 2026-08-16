#nullable enable
namespace XrmTools.Meta.Attributes.SourceGenerator;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

[Generator(LanguageNames.CSharp)]
public sealed class XrmToolsMetaAttributesGenerator : IIncrementalGenerator
{
    private const string ResourcePrefix = "XrmTools.Meta.Attributes.Sources/";
    private const string UsePublicAccessibilityPropertyName = "build_property.XrmToolsMetaAttributesUsePublicAccessibility";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var compilationAndOptions = context.CompilationProvider.Combine(context.AnalyzerConfigOptionsProvider);

        context.RegisterSourceOutput(compilationAndOptions, static (sourceProductionContext, value) =>
        {
            var compilation = value.Left;
            var options = GeneratorOptions.From(value.Right.GlobalOptions);

            foreach (var sourceFile in GetSourceFiles(compilation, options))
            {
                sourceProductionContext.AddSource(sourceFile.HintName, sourceFile.Content);
            }
        });
    }

    private static IReadOnlyList<GeneratedSourceFile> GetSourceFiles(Compilation compilation, GeneratorOptions options)
    {
        var assembly = typeof(XrmToolsMetaAttributesGenerator).Assembly;
        var names = assembly.GetManifestResourceNames()
            .Where(static name => name.Replace('\\', '/').StartsWith(ResourcePrefix, StringComparison.Ordinal))
            .OrderBy(static name => name, StringComparer.Ordinal)
            .ToArray();

        var files = new List<GeneratedSourceFile>(names.Length);
        var hasXrmSdkReference = HasXrmSdkReference(compilation);

        foreach (var name in names)
        {
            using var stream = assembly.GetManifestResourceStream(name);
            if (stream is null)
            {
                continue;
            }

            using var reader = new StreamReader(stream, Encoding.UTF8, true);
            var content = reader.ReadToEnd();
            var normalizedName = name.Replace('\\', '/');
            var relativePath = normalizedName.Substring(ResourcePrefix.Length);
            if (!hasXrmSdkReference && string.Equals(relativePath, "DependencyScope.cs", StringComparison.Ordinal))
            {
                continue;
            }

            var emittedContent = options.UsePublicAccessibility
                ? RewriteTopLevelAccessibility(content)
                : content;

            files.Add(new GeneratedSourceFile(relativePath.Replace('/', '_'), emittedContent));
        }

        return files;
    }

    private static bool HasXrmSdkReference(Compilation compilation)
    {
        foreach (var reference in compilation.ReferencedAssemblyNames)
        {
            if (string.Equals(reference.Name, "Microsoft.Xrm.Sdk", StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static string RewriteTopLevelAccessibility(string content)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(content);
        var root = syntaxTree.GetRoot();
        var rewrittenRoot = new TopLevelAccessibilityRewriter().Visit(root);

        return rewrittenRoot?.ToFullString() ?? content;
    }

    private readonly struct GeneratorOptions
    {
        public GeneratorOptions(bool usePublicAccessibility)
        {
            UsePublicAccessibility = usePublicAccessibility;
        }

        public bool UsePublicAccessibility { get; }

        public static GeneratorOptions From(AnalyzerConfigOptions options)
        {
            if (options.TryGetValue(UsePublicAccessibilityPropertyName, out var value))
            {
                if (bool.TryParse(value, out var usePublicAccessibility))
                {
                    return new GeneratorOptions(usePublicAccessibility);
                }
            }

            return default;
        }
    }

    private sealed class TopLevelAccessibilityRewriter : CSharpSyntaxRewriter
    {
        public override SyntaxNode? VisitClassDeclaration(ClassDeclarationSyntax node) =>
            base.VisitClassDeclaration(RewriteDeclaration(node, static (declaration, modifiers) => declaration.WithModifiers(modifiers)));

        public override SyntaxNode? VisitEnumDeclaration(EnumDeclarationSyntax node) =>
            base.VisitEnumDeclaration(RewriteDeclaration(node, static (declaration, modifiers) => declaration.WithModifiers(modifiers)));

        public override SyntaxNode? VisitInterfaceDeclaration(InterfaceDeclarationSyntax node) =>
            base.VisitInterfaceDeclaration(RewriteDeclaration(node, static (declaration, modifiers) => declaration.WithModifiers(modifiers)));

        public override SyntaxNode? VisitRecordDeclaration(RecordDeclarationSyntax node) =>
            base.VisitRecordDeclaration(RewriteDeclaration(node, static (declaration, modifiers) => declaration.WithModifiers(modifiers)));

        public override SyntaxNode? VisitStructDeclaration(StructDeclarationSyntax node) =>
            base.VisitStructDeclaration(RewriteDeclaration(node, static (declaration, modifiers) => declaration.WithModifiers(modifiers)));

        private static T RewriteDeclaration<T>(T declaration, Func<T, SyntaxTokenList, T> updateModifiers)
            where T : MemberDeclarationSyntax
        {
            if (!IsTopLevelDeclaration(declaration))
            {
                return declaration;
            }

            var modifiers = GetModifiers(declaration);
            for (int i = 0; i < modifiers.Count; i++)
            {
                if (!modifiers[i].IsKind(SyntaxKind.InternalKeyword))
                {
                    continue;
                }

                modifiers = modifiers.Replace(modifiers[i], SyntaxFactory.Token(modifiers[i].LeadingTrivia, SyntaxKind.PublicKeyword, modifiers[i].TrailingTrivia));
                return updateModifiers(declaration, modifiers);
            }

            return declaration;
        }

        private static SyntaxTokenList GetModifiers(MemberDeclarationSyntax declaration) => declaration switch
        {
            ClassDeclarationSyntax classDeclaration => classDeclaration.Modifiers,
            EnumDeclarationSyntax enumDeclaration => enumDeclaration.Modifiers,
            InterfaceDeclarationSyntax interfaceDeclaration => interfaceDeclaration.Modifiers,
            RecordDeclarationSyntax recordDeclaration => recordDeclaration.Modifiers,
            StructDeclarationSyntax structDeclaration => structDeclaration.Modifiers,
            _ => default,
        };

        private static bool IsTopLevelDeclaration(MemberDeclarationSyntax declaration) =>
            declaration.Parent is NamespaceDeclarationSyntax or FileScopedNamespaceDeclarationSyntax or CompilationUnitSyntax;
    }

    private readonly struct GeneratedSourceFile
    {
        public GeneratedSourceFile(string hintName, string content)
        {
            HintName = hintName;
            Content = content;
        }

        public string HintName { get; }

        public string Content { get; }
    }
}
