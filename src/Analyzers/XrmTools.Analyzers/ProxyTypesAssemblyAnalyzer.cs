#nullable enable
namespace XrmTools.Analyzers;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ProxyTypesAssemblyAnalyzer : DiagnosticAnalyzer
{
    public const string MissingProxyTypesAssemblyAttributeId = "XrmTools010";

    private static readonly DiagnosticDescriptor MissingProxyTypesAssemblyAttributeRule =
        new(
            MissingProxyTypesAssemblyAttributeId,
            "Assembly missing ProxyTypesAssemblyAttribute",
            "Assembly contains entity type '{0}' but is missing [assembly: ProxyTypesAssemblyAttribute]",
            "Usage",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Assemblies that define entity types should declare ProxyTypesAssemblyAttribute.",
            helpLinkUri: "https://github.com/rezanid/xrmtools/wiki/Analyzers#xrmtools006---missing-proxytypesassembly-attribute",
            customTags: ["CompilationEnd"]);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        [MissingProxyTypesAssemblyAttributeRule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(static compilationStart =>
        {
            var entitySymbol = compilationStart.Compilation.GetTypeByMetadataName("Microsoft.Xrm.Sdk.Entity");
            if (entitySymbol is null)
                return;

            var proxyAttrSymbol = compilationStart.Compilation.GetTypeByMetadataName("Microsoft.Xrm.Sdk.Client.ProxyTypesAssemblyAttribute");
            if (proxyAttrSymbol is null)
                return;

            var hasProxyAttr = compilationStart.Compilation.Assembly.GetAttributes()
                .Any(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, proxyAttrSymbol));
            if (hasProxyAttr)
                return;

            Location? entityLocation = null;
            string? entityTypeName = null;
            int entityStart = int.MaxValue;
            object gate = new();

            compilationStart.RegisterSymbolAction(symbolContext =>
            {
                if (symbolContext.Symbol is not INamedTypeSymbol typeSymbol)
                    return;

                if (typeSymbol.TypeKind != TypeKind.Class)
                    return;

                if (typeSymbol.IsAbstract)
                    return;

                if (!DerivesFrom(typeSymbol, entitySymbol))
                    return;

                var syntaxRef = typeSymbol.DeclaringSyntaxReferences.FirstOrDefault();
                if (syntaxRef is null)
                    return;

                var syntaxNode = syntaxRef.GetSyntax(symbolContext.CancellationToken);
                var location = syntaxNode switch
                {
                    ClassDeclarationSyntax classDecl => classDecl.Identifier.GetLocation(),
                    _ => syntaxNode.GetLocation()
                };

                var start = location.SourceSpan.Start;

                lock (gate)
                {
                    if (start >= entityStart)
                        return;

                    entityStart = start;
                    entityLocation = location;
                    entityTypeName = typeSymbol.Name;
                }
            }, SymbolKind.NamedType);

            compilationStart.RegisterCompilationEndAction(compilationEnd =>
            {
                if (entityLocation is null || entityTypeName is null)
                    return;

                compilationEnd.ReportDiagnostic(Diagnostic.Create(
                    MissingProxyTypesAssemblyAttributeRule,
                    entityLocation,
                    entityTypeName));
            });
        });
    }

    private static bool DerivesFrom(INamedTypeSymbol type, INamedTypeSymbol baseType)
    {
        for (var current = type.BaseType; current is not null; current = current.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(current, baseType))
                return true;
        }

        return false;
    }
}
#nullable restore
