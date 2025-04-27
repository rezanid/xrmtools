#nullable enable
namespace XrmTools.Analyzers;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using XrmTools.Meta.Attributes;
using XrmTools.Meta.Model;

public interface ILocalDependencyAnalyzerOld
{
    Dependency Analyze(INamedTypeSymbol rootType, Compilation compilation);
}

//public class Dependency
//{
//    public string? PropertyName { get; set; }
//    public string FullTypeName { get; set; } = "";
//    public string ShortTypeName { get; set; } = "";
//    public List<Dependency> Dependencies { get; set; } = new();
//    public bool IsProperty { get; set; } // true = Property, false = Constructor
//}

[Export(typeof(ILocalDependencyAnalyzerOld))]
public class LocalDependencyAnalyzerOld : ILocalDependencyAnalyzerOld
{
    public Dependency Analyze(INamedTypeSymbol rootType, Compilation compilation)
    {
        var implementationMap = BuildImplementationMap(compilation);
        var dependedOn = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
        return BuildNode(rootType, null, compilation, implementationMap, dependedOn);
    }

    private Dependency BuildNode(
        INamedTypeSymbol classSymbol,
        string? propertyName,
        Compilation compilation,
        Dictionary<ITypeSymbol, INamedTypeSymbol> implementationMap,
        HashSet<INamedTypeSymbol> dependedOn)
    {
        var serviceProvider = compilation.GetTypeByMetadataName("System.IServiceProvider");

        var node = new Dependency
        {
            PropertyName = propertyName,
            FullTypeName = classSymbol.ToDisplayString(),
            ShortTypeName = classSymbol.Name,
            Dependencies = []
        };

        // Analyze [Dependency] properties
        foreach (var prop in classSymbol.GetMembers().OfType<IPropertySymbol>())
        {
            if (!prop.GetAttributes().Any(attr => attr.AttributeClass?.ToDisplayString() == typeof(DependencyAttribute).FullName))
                continue;

            var propType = prop.Type;
            if (SymbolEqualityComparer.Default.Equals(propType, serviceProvider))
            {
                node.Dependencies.Add(new Dependency
                {
                    PropertyName = prop.Name,
                    FullTypeName = propType.ToDisplayString(),
                    ShortTypeName = prop.Name,
                    IsProperty = true
                });
                continue;
            }

            if ((propType.TypeKind == TypeKind.Interface ||
                 (propType.TypeKind == TypeKind.Class && ((INamedTypeSymbol)propType).IsAbstract)) &&
                implementationMap.TryGetValue(propType, out var impl))
            {
                propType = impl;
            }

            if (propType is not INamedTypeSymbol depSymbol || depSymbol.ContainingType != null)
                continue;

            if (!dependedOn.Add(depSymbol))
                continue;

            var childNode = BuildNode(depSymbol, prop.Name, compilation, implementationMap, dependedOn);
            childNode.IsProperty = true;
            node.Dependencies.Add(childNode);
        }

        // Analyze constructor parameters
        var ctor = GetConstructor(classSymbol);
        if (ctor is not null)
        {
            foreach (var param in ctor.Parameters)
            {
                var paramType = param.Type;
                if (SymbolEqualityComparer.Default.Equals(paramType, serviceProvider))
                {
                    node.Dependencies.Add(new Dependency
                    {
                        PropertyName = param.Name,
                        FullTypeName = paramType.ToDisplayString(),
                        ShortTypeName = param.Name,
                        IsProperty = false
                    });
                    continue;
                }

                if ((paramType.TypeKind == TypeKind.Interface ||
                     (paramType.TypeKind == TypeKind.Class && ((INamedTypeSymbol)paramType).IsAbstract)) &&
                    implementationMap.TryGetValue(paramType, out var impl))
                {
                    paramType = impl;
                }

                if (paramType is not INamedTypeSymbol depSymbol || depSymbol.ContainingType != null)
                    continue;

                if (!dependedOn.Add(depSymbol))
                    continue;

                var childNode = BuildNode(depSymbol, param.Name, compilation, implementationMap, dependedOn);
                childNode.IsProperty = false;
                node.Dependencies.Add(childNode);
            }
        }

        return node;
    }

    private IMethodSymbol? GetConstructor(INamedTypeSymbol classSymbol)
    {
        var ctors = classSymbol.InstanceConstructors
            .Where(c => c.DeclaredAccessibility == Accessibility.Public)
            .ToList();

        if (ctors.Count == 0)
            return null;

        foreach (var ctor in ctors)
        {
            if (ctor.GetAttributes().Any(attr => attr.AttributeClass?.ToDisplayString() == typeof(DependencyConstructorAttribute).FullName))
                return ctor;
        }

        return ctors.First();
    }

    private Dictionary<ITypeSymbol, INamedTypeSymbol> BuildImplementationMap(Compilation compilation)
    {
        var result = new Dictionary<ITypeSymbol, INamedTypeSymbol>(SymbolEqualityComparer.Default);

        void ProcessNamespace(INamespaceSymbol ns)
        {
            foreach (var type in ns.GetTypeMembers())
            {
                if (type.TypeKind == TypeKind.Class && !type.IsAbstract)
                {
                    foreach (var iface in type.AllInterfaces)
                    {
                        if (!result.ContainsKey(iface))
                            result[iface] = type;
                    }

                    var baseType = type.BaseType;
                    while (baseType is { SpecialType: not SpecialType.System_Object })
                    {
                        if (baseType.TypeKind == TypeKind.Class && baseType.IsAbstract && !result.ContainsKey(baseType))
                            result[baseType] = type;

                        baseType = baseType.BaseType;
                    }
                }
            }

            foreach (var subNs in ns.GetNamespaceMembers())
                ProcessNamespace(subNs);
        }

        ProcessNamespace(compilation.Assembly.GlobalNamespace);

        foreach (var reference in compilation.References)
        {
            if (compilation.GetAssemblyOrModuleSymbol(reference) is IAssemblySymbol referencedAssembly)
            {
                ProcessNamespace(referencedAssembly.GlobalNamespace);
            }
        }

        return result;
    }
}
#nullable restore