namespace XrmTools.Analyzers;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class PluginDependencyScopeAnalyzer : DiagnosticAnalyzer
{
    public const string DependencyOutsideScopeId = "XrmTools005";

    private static readonly DiagnosticDescriptor DependencyOutsideScopeRule =
        new(
            DependencyOutsideScopeId,
            "Dependency property accessed outside CreateScope",
            "Dependency property '{0}' is accessed outside a CreateScope(serviceProvider) scope",
            "Usage",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Dependency properties (Require<T>() facades) must only be accessed while a scope created by CreateScope(serviceProvider) is active.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DependencyOutsideScopeRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(static compilationStart =>
        {
            var pluginSymbol = compilationStart.Compilation.GetTypeByMetadataName("Microsoft.Xrm.Sdk.IPlugin");
            if (pluginSymbol is null)
                return;

            compilationStart.RegisterSyntaxNodeAction(
                c => AnalyzeClass(c, pluginSymbol),
                SyntaxKind.ClassDeclaration);
        });
    }

    private static void AnalyzeClass(SyntaxNodeAnalysisContext context, INamedTypeSymbol pluginSymbol)
    {
        var classDecl = (ClassDeclarationSyntax)context.Node;

        var typeSymbol = context.SemanticModel.GetDeclaredSymbol(classDecl, context.CancellationToken) as INamedTypeSymbol;
        if (typeSymbol is null)
            return;

        if (!Implements(typeSymbol, pluginSymbol))
            return;

        var createScopeMethod = typeSymbol.GetMembers("CreateScope").OfType<IMethodSymbol>()
            .FirstOrDefault(m => m.MethodKind == MethodKind.Ordinary && m.Parameters.Length == 1);
        if (createScopeMethod is null)
            return;

        var dependencyProps = GetDependencyProperties(typeSymbol, context.SemanticModel.Compilation, context.CancellationToken);
        if (dependencyProps.Count == 0)
            return;

        var methods = new Dictionary<IMethodSymbol, MethodInfo>(SymbolEqualityComparer.Default);

        foreach (var methodDecl in classDecl.Members.OfType<MethodDeclarationSyntax>())
        {
            var method = context.SemanticModel.GetDeclaredSymbol(methodDecl, context.CancellationToken) as IMethodSymbol;
            if (method is null)
                continue;

            if (method.MethodKind != MethodKind.Ordinary)
                continue;

            if (!SymbolEqualityComparer.Default.Equals(method.ContainingType, typeSymbol))
                continue;

            var info = new MethodInfo(method);
            methods[method] = info;

            var scopeSpans = GetCreateScopeRegionSpans(methodDecl, context.SemanticModel, createScopeMethod, context.CancellationToken);
            var inScopeUsingNodes = GetCreateScopeUsingNodes(methodDecl, context.SemanticModel, createScopeMethod, context.CancellationToken);

            foreach (var expr in methodDecl.DescendantNodes().OfType<ExpressionSyntax>())
            {
                if (expr is not IdentifierNameSyntax && expr is not MemberAccessExpressionSyntax)
                    continue;

                // Skip declarations and attributes (we only care about actual reads).
                if (expr.Parent is AttributeSyntax || expr.Parent is AttributeArgumentSyntax)
                    continue;

                if (expr.Parent is PropertyDeclarationSyntax)
                    continue;

                if (expr.Parent is MemberAccessExpressionSyntax ma && !ReferenceEquals(ma.Name, expr))
                    continue;

                var symbol = context.SemanticModel.GetSymbolInfo(expr, context.CancellationToken).Symbol;
                if (symbol is not IPropertySymbol prop)
                    continue;

                if (!dependencyProps.Contains(prop.OriginalDefinition))
                    continue;

                var inScope = IsSpanWithinAnySpan(expr.Span, scopeSpans) || IsUnderAnyUsing(expr, inScopeUsingNodes);
                info.DependencyReads.Add(new PropertyRead(prop.OriginalDefinition, expr.GetLocation(), inScope));
            }

            foreach (var invocation in methodDecl.DescendantNodes().OfType<InvocationExpressionSyntax>())
            {
                var sym = context.SemanticModel.GetSymbolInfo(invocation, context.CancellationToken).Symbol as IMethodSymbol;
                if (sym is null)
                    continue;

                sym = sym.ReducedFrom ?? sym;
                if (!SymbolEqualityComparer.Default.Equals(sym.ContainingType, typeSymbol))
                    continue;

                if (sym.MethodKind != MethodKind.Ordinary)
                    continue;

                var inScope = IsSpanWithinAnySpan(invocation.Span, scopeSpans) || IsUnderAnyUsing(invocation, inScopeUsingNodes);
                info.Calls.Add(new Call(sym, inScope));
            }
        }

        if (methods.Count == 0)
            return;

        var entryMethods = methods.Keys.Where(m => m.Name == "Execute" && m.Parameters.Length == 1).ToList();
        if (entryMethods.Count == 0)
            return;

        var reachableOut = new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default);
        var reachableIn = new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default);
        var queue = new Queue<(IMethodSymbol method, bool inScope)>();

        for (int i = 0; i < entryMethods.Count; i++)
            queue.Enqueue((entryMethods[i], false));

        while (queue.Count > 0)
        {
            var (m, inScope) = queue.Dequeue();
            var set = inScope ? reachableIn : reachableOut;
            if (!set.Add(m))
                continue;

            if (!methods.TryGetValue(m, out var info))
                continue;

            for (int i = 0; i < info.Calls.Count; i++)
            {
                var call = info.Calls[i];
                queue.Enqueue((call.Target, inScope || call.InScope));
            }
        }

        foreach (var kvp in methods)
        {
            var method = kvp.Key;
            var info = kvp.Value;

            var methodReachableOut = reachableOut.Contains(method);

            for (int i = 0; i < info.DependencyReads.Count; i++)
            {
                var read = info.DependencyReads[i];
                if (methodReachableOut && !read.InScope)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        DependencyOutsideScopeRule,
                        read.Location,
                        read.Property.Name));
                }
            }
        }
    }

    private static HashSet<IPropertySymbol> GetDependencyProperties(INamedTypeSymbol typeSymbol, Compilation compilation, System.Threading.CancellationToken cancellationToken)
    {
        var result = new HashSet<IPropertySymbol>(SymbolEqualityComparer.Default);

        foreach (var prop in typeSymbol.GetMembers().OfType<IPropertySymbol>())
        {
            foreach (var syntaxRef in prop.DeclaringSyntaxReferences)
            {
                if (syntaxRef.GetSyntax(cancellationToken) is not PropertyDeclarationSyntax propDecl)
                    continue;

                if (CallsRequireInGetter(propDecl, compilation.GetSemanticModel(propDecl.SyntaxTree), cancellationToken))
                {
                    result.Add(prop);
                    break;
                }
            }
        }

        return result;
    }

    private static bool CallsRequireInGetter(PropertyDeclarationSyntax propDecl, SemanticModel semanticModel, System.Threading.CancellationToken cancellationToken)
    {
        if (propDecl.ExpressionBody is not null)
            return ContainsRequireInvocation(propDecl.ExpressionBody.Expression, semanticModel, cancellationToken);

        if (propDecl.AccessorList is null)
            return false;

        foreach (var accessor in propDecl.AccessorList.Accessors)
        {
            if (!accessor.IsKind(SyntaxKind.GetAccessorDeclaration))
                continue;

            if (accessor.ExpressionBody is not null && ContainsRequireInvocation(accessor.ExpressionBody.Expression, semanticModel, cancellationToken))
                return true;

            if (accessor.Body is not null && ContainsRequireInvocation(accessor.Body, semanticModel, cancellationToken))
                return true;
        }

        return false;
    }

    private static bool ContainsRequireInvocation(SyntaxNode node, SemanticModel semanticModel, System.Threading.CancellationToken cancellationToken)
    {
        foreach (var invocation in node.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>())
        {
            var symbol = semanticModel.GetSymbolInfo(invocation, cancellationToken).Symbol as IMethodSymbol;
            if (symbol is null)
                continue;

            if (symbol.Name != "Require")
                continue;

            // Be permissive about overloads/signatures; generated code may add more overloads over time.
            // We only require the method name and that it is invoked as a generic method with one type argument.
            if (symbol.IsGenericMethod && symbol.TypeArguments.Length != 1)
                continue;

            return true;
        }

        return false;
    }

    private static List<TextSpan> GetCreateScopeRegionSpans(
        MethodDeclarationSyntax methodDecl,
        SemanticModel semanticModel,
        IMethodSymbol createScope,
        System.Threading.CancellationToken cancellationToken)
    {
        var spans = new List<TextSpan>();

        foreach (var u in methodDecl.DescendantNodes().OfType<UsingStatementSyntax>())
        {
            if (!UsingContainsCreateScopeInvocation(u, semanticModel, createScope, cancellationToken))
                continue;

            if (u.Statement is not null)
                spans.Add(u.Statement.Span);
        }

        foreach (var local in methodDecl.DescendantNodes().OfType<LocalDeclarationStatementSyntax>())
        {
            if (!local.UsingKeyword.IsKind(SyntaxKind.UsingKeyword))
                continue;

            if (!LocalUsingContainsCreateScopeInvocation(local, semanticModel, createScope, cancellationToken))
                continue;

            if (local.Parent is not BlockSyntax block)
                continue;

            var index = block.Statements.IndexOf(local);
            if (index < 0)
                continue;

            var start = local.Span.End;
            var end = block.CloseBraceToken.SpanStart;
            if (end > start)
                spans.Add(TextSpan.FromBounds(start, end));
        }

        return spans;
    }

    private static List<UsingStatementSyntax> GetCreateScopeUsingNodes(
        MethodDeclarationSyntax methodDecl,
        SemanticModel semanticModel,
        IMethodSymbol createScope,
        System.Threading.CancellationToken cancellationToken)
    {
        var nodes = new List<UsingStatementSyntax>();
        foreach (var u in methodDecl.DescendantNodes().OfType<UsingStatementSyntax>())
        {
            if (UsingContainsCreateScopeInvocation(u, semanticModel, createScope, cancellationToken))
                nodes.Add(u);
        }
        return nodes;
    }

    private static bool IsUnderAnyUsing(SyntaxNode node, List<UsingStatementSyntax> usings)
    {
        for (int i = 0; i < usings.Count; i++)
        {
            var u = usings[i];
            // For `using (CreateScope(...)) { ... }` the body is definitely in-scope,
            // but the resource acquisition expression/declaration should also count as in-scope
            // for purposes of this analyzer.
            if (u.Span.Contains(node.Span))
                return true;
        }

        return false;
    }

    private static bool UsingContainsCreateScopeInvocation(UsingStatementSyntax usingStmt, SemanticModel semanticModel, IMethodSymbol createScope, System.Threading.CancellationToken cancellationToken)
    {
        if (usingStmt.Expression is not null)
            return ExpressionContainsCreateScope(usingStmt.Expression, semanticModel, createScope, cancellationToken);

        if (usingStmt.Declaration is not null)
        {
            foreach (var v in usingStmt.Declaration.Variables)
            {
                if (v.Initializer?.Value is null)
                    continue;

                if (ExpressionContainsCreateScope(v.Initializer.Value, semanticModel, createScope, cancellationToken))
                    return true;
            }
        }

        return false;
    }

    private static bool LocalUsingContainsCreateScopeInvocation(LocalDeclarationStatementSyntax local, SemanticModel semanticModel, IMethodSymbol createScope, System.Threading.CancellationToken cancellationToken)
    {
        if (local.Declaration is null)
            return false;

        foreach (var v in local.Declaration.Variables)
        {
            if (v.Initializer?.Value is null)
                continue;

            if (ExpressionContainsCreateScope(v.Initializer.Value, semanticModel, createScope, cancellationToken))
                return true;
        }

        return false;
    }

    private static bool ExpressionContainsCreateScope(ExpressionSyntax expr, SemanticModel semanticModel, IMethodSymbol createScope, System.Threading.CancellationToken cancellationToken)
    {
        foreach (var invocation in expr.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>())
        {
            var sym = semanticModel.GetSymbolInfo(invocation, cancellationToken).Symbol as IMethodSymbol;
            if (sym is null)
                continue;

            if (SymbolEqualityComparer.Default.Equals(sym, createScope))
                return true;
        }

        return false;
    }

    private static bool IsSpanWithinAnySpan(TextSpan span, List<TextSpan> spans)
    {
        for (int i = 0; i < spans.Count; i++)
        {
            if (spans[i].Contains(span))
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

    private sealed class MethodInfo
    {
        public IMethodSymbol Method { get; }
        public List<Call> Calls { get; }
        public List<PropertyRead> DependencyReads { get; }

        public MethodInfo(IMethodSymbol method)
        {
            Method = method;
            Calls = new List<Call>();
            DependencyReads = new List<PropertyRead>();
        }
    }

    private readonly struct Call
    {
        public IMethodSymbol Target { get; }
        public bool InScope { get; }

        public Call(IMethodSymbol target, bool inScope)
        {
            Target = target;
            InScope = inScope;
        }
    }

    private readonly struct PropertyRead
    {
        public IPropertySymbol Property { get; }
        public Location Location { get; }
        public bool InScope { get; }

        public PropertyRead(IPropertySymbol property, Location location, bool inScope)
        {
            Property = property;
            Location = location;
            InScope = inScope;
        }
    }
}
