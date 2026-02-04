namespace XrmTools.Analyzers;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class PluginDependencyAnalyzer : DiagnosticAnalyzer
{
    public const string MissingDependencyAttributeId = "XrmTools001";
    public const string HasSetterOrInitId = "XrmTools002";
    public const string PluginAttributeCombinationId = "XrmTools003";

    private static readonly DiagnosticDescriptor MissingDependencyAttributeRule =
        new(
            MissingDependencyAttributeId,
            "Dependency property doesn't have [Dependency] attribute",
            "Dependency property '{0}' doesn't have a [Dependency] attribute",
            "Usage",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Properties that call Require<T>() should have [Dependency].");

    private static readonly DiagnosticDescriptor HasSetterOrInitRule =
        new(
            HasSetterOrInitId,
            "Dependency property should not have a setter",
            "Dependency property '{0}' should not declare a setter (set/init)",
            "Usage",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Dependency properties are DI facades and must not store state.");

    private static readonly DiagnosticDescriptor PluginAttributeCombinationRule =
        new(
            PluginAttributeCombinationId,
            "Plugin type has invalid attribute combination",
            "Plugin type '{0}' must have [Plugin] and exactly one of [CustomApi] or [Step]",
            "Usage",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Plugin types must be marked with [Plugin] and represent exactly one of CustomApi or Step.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(MissingDependencyAttributeRule, HasSetterOrInitRule, PluginAttributeCombinationRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(static compilationStart =>
        {
            var pluginSymbol = compilationStart.Compilation.GetTypeByMetadataName("Microsoft.Xrm.Sdk.IPlugin");
            if (pluginSymbol is null)
                return;

            // TODO: Replace with the fully-qualified metadata name of your attribute.
            // Examples:
            // "XrmTools.Annotations.DependencyAttribute"
            // "YourCompany.XrmTools.DependencyAttribute"
            var dependencyAttrSymbol = compilationStart.Compilation.GetTypeByMetadataName("XrmTools.Meta.Attributes.DependencyAttribute");
            if (dependencyAttrSymbol is null)
                return;

            var pluginAttrSymbol = compilationStart.Compilation.GetTypeByMetadataName("XrmTools.Meta.Attributes.PluginAttribute");
            var customApiAttrSymbol = compilationStart.Compilation.GetTypeByMetadataName("XrmTools.Meta.Attributes.CustomApiAttribute");
            var stepAttrSymbol = compilationStart.Compilation.GetTypeByMetadataName("XrmTools.Meta.Attributes.StepAttribute");

            compilationStart.RegisterSyntaxNodeAction(
                c => AnalyzeProperty(c, pluginSymbol, dependencyAttrSymbol),
                SyntaxKind.PropertyDeclaration);

            compilationStart.RegisterSymbolAction(
                c => AnalyzePluginTypeAttributes(c, pluginSymbol, pluginAttrSymbol, customApiAttrSymbol, stepAttrSymbol),
                SymbolKind.NamedType);
        });
    }

    private static void AnalyzePluginTypeAttributes(
        SymbolAnalysisContext context,
        INamedTypeSymbol pluginSymbol,
        INamedTypeSymbol? pluginAttrSymbol,
        INamedTypeSymbol? customApiAttrSymbol,
        INamedTypeSymbol? stepAttrSymbol)
    {
        if (pluginAttrSymbol is null || customApiAttrSymbol is null || stepAttrSymbol is null)
            return;

        if (context.Symbol is not INamedTypeSymbol typeSymbol)
            return;

        if (typeSymbol.TypeKind != TypeKind.Class)
            return;

        if (!Implements(typeSymbol, pluginSymbol))
            return;

        var attrs = typeSymbol.GetAttributes();
        var hasPlugin = HasAttribute(attrs, pluginAttrSymbol);
        var hasCustomApi = HasAttribute(attrs, customApiAttrSymbol);
        var hasStep = HasAttribute(attrs, stepAttrSymbol);

        if (!hasPlugin)
            return;

        if (hasCustomApi == hasStep)
        {
            var location = typeSymbol.Locations.FirstOrDefault();
            if (location is null)
                return;

            context.ReportDiagnostic(Diagnostic.Create(
                PluginAttributeCombinationRule,
                location,
                typeSymbol.Name));
        }
    }

    private static void AnalyzeProperty(
        SyntaxNodeAnalysisContext context,
        INamedTypeSymbol pluginSymbol,
        INamedTypeSymbol dependencyAttrSymbol)
    {
        var propDecl = (PropertyDeclarationSyntax)context.Node;

        // Only consider properties inside IPlugin types
        var containingType = context.ContainingSymbol?.ContainingType;
        if (containingType is null || !Implements(containingType, pluginSymbol))
            return;

        // Get declared property symbol (for attribute inspection + nice name)
        if (context.SemanticModel.GetDeclaredSymbol(propDecl, context.CancellationToken) is not IPropertySymbol propSymbol)
            return;

        // Dependency property = getter/expression body calls Require<T>() (no args)
        if (!CallsRequireInGetter(propDecl, context.SemanticModel, context.CancellationToken))
            return;

        // Rule 1: must have [Dependency]
        if (!HasAttribute(propSymbol, dependencyAttrSymbol))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                MissingDependencyAttributeRule,
                propDecl.Identifier.GetLocation(),
                propSymbol.Name));
        }

        // Rule 2: must not have set/init (any scope)
        if (HasSetterOrInit(propDecl, out var setterOrInitLocation))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                HasSetterOrInitRule,
                setterOrInitLocation ?? propDecl.Identifier.GetLocation(),
                propSymbol.Name));
        }
    }

    private static bool CallsRequireInGetter(
        PropertyDeclarationSyntax propDecl,
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        // Expression-bodied property:
        // ITracingService Tracing => Require<ITracingService>();
        if (propDecl.ExpressionBody is not null)
            return ContainsRequireInvocation(propDecl.ExpressionBody.Expression, semanticModel, cancellationToken);

        // Accessor-based property:
        if (propDecl.AccessorList is null)
            return false;

        foreach (var accessor in propDecl.AccessorList.Accessors)
        {
            if (!accessor.IsKind(SyntaxKind.GetAccessorDeclaration))
                continue;

            // get => Require<T>();
            if (accessor.ExpressionBody is not null &&
                ContainsRequireInvocation(accessor.ExpressionBody.Expression, semanticModel, cancellationToken))
            {
                return true;
            }

            // get { return Require<T>(); }
            if (accessor.Body is not null &&
                ContainsRequireInvocation(accessor.Body, semanticModel, cancellationToken))
            {
                return true;
            }
        }

        return false;
    }

    private static bool ContainsRequireInvocation(
        SyntaxNode node,
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        foreach (var invocation in node.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>())
        {
            // Fast syntax gate: Require<...>(...)
            if (invocation.Expression is not GenericNameSyntax g ||
                g.Identifier.ValueText != "Require" ||
                g.TypeArgumentList.Arguments.Count != 1)
            {
                continue;
            }

            // Your definition: Require<T>() must have zero args
            if (invocation.ArgumentList.Arguments.Count != 0)
                continue;

            // Semantic confirmation: binds to a method named Require with 1 type arg.
            // (Works whether Require is declared or inherited; also covers extension methods.)
            var symbolInfo = semanticModel.GetSymbolInfo(invocation, cancellationToken);
            if (symbolInfo.Symbol is IMethodSymbol method &&
                method.Name == "Require" &&
                method.TypeArguments.Length == 1)
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasSetterOrInit(PropertyDeclarationSyntax propDecl, out Location location)
    {
        location = null;

        if (propDecl.AccessorList is null)
            return false;

        foreach (var accessor in propDecl.AccessorList.Accessors)
        {
            if (accessor.IsKind(SyntaxKind.SetAccessorDeclaration) ||
                accessor.IsKind(SyntaxKind.InitAccessorDeclaration))
            {
                location = accessor.Keyword.GetLocation(); // points to `set`/`init`
                return true;
            }
        }

        return false;
    }

    private static bool HasAttribute(ISymbol symbol, INamedTypeSymbol attributeType)
    {
        foreach (var attr in symbol.GetAttributes())
        {
            if (attr.AttributeClass is null) continue;
            if (SymbolEqualityComparer.Default.Equals(attr.AttributeClass, attributeType))
                return true;
        }
        return false;
    }

    private static bool HasAttribute(ImmutableArray<AttributeData> attributes, INamedTypeSymbol attributeType)
    {
        for (int i = 0; i < attributes.Length; i++)
        {
            var ac = attributes[i].AttributeClass;
            if (ac is null)
                continue;
            if (SymbolEqualityComparer.Default.Equals(ac, attributeType))
                return true;
        }
        return false;
    }

    private static bool Implements(INamedTypeSymbol type, INamedTypeSymbol iface)
    {
        foreach (var i in type.AllInterfaces)
        {
            if (SymbolEqualityComparer.Default.Equals(i, iface))
                return true;
        }
        return false;
    }
}