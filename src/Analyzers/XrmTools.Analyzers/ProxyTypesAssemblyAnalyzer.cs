#nullable enable
namespace XrmTools.Analyzers;

using Microsoft.CodeAnalysis;
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

            int reported = 0;

            compilationStart.RegisterSymbolAction(symbolContext =>
            {
                if (Interlocked.Exchange(ref reported, 1) == 1)
                    return;

                if (symbolContext.Symbol is not INamedTypeSymbol typeSymbol)
                    return;

                if (typeSymbol.TypeKind != TypeKind.Class)
                    return;

                if (!DerivesFrom(typeSymbol, entitySymbol))
                    return;

                // Prefer declaring syntax location (more reliable than Locations in some cases)
                var syntaxRef = typeSymbol.DeclaringSyntaxReferences.FirstOrDefault();
                if (syntaxRef is null)
                    return;

                var location = syntaxRef.GetSyntax(symbolContext.CancellationToken).GetLocation();

                symbolContext.ReportDiagnostic(Diagnostic.Create(
                    MissingProxyTypesAssemblyAttributeRule,
                    location,
                    typeSymbol.Name));
            }, SymbolKind.NamedType);

            //Location? entityLocation = null;
            //string? entityTypeName = null;

            //compilationStart.RegisterSymbolAction(symbolContext =>
            //{
            //    if (entityLocation is not null)
            //        return;

            //    if (symbolContext.Symbol is not INamedTypeSymbol typeSymbol)
            //        return;

            //    if (typeSymbol.TypeKind != TypeKind.Class)
            //        return;

            //    if (!DerivesFrom(typeSymbol, entitySymbol))
            //        return;

            //    var location = typeSymbol.Locations.FirstOrDefault(static l => l.IsInSource);
            //    if (location is null)
            //        return;

            //    if (Interlocked.CompareExchange(ref entityLocation, location, null) is null)
            //        entityTypeName = typeSymbol.Name;
            //}, SymbolKind.NamedType);

            //compilationStart.RegisterCompilationEndAction(compilationEnd =>
            //{
            //    if (entityLocation is null || entityTypeName is null)
            //        return;

            //    compilationEnd.ReportDiagnostic(Diagnostic.Create(
            //        MissingProxyTypesAssemblyAttributeRule,
            //        entityLocation,
            //        entityTypeName));
            //});
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
