#nullable enable
namespace XrmTools.Analyzers;

using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using XrmTools.Meta.Attributes;
using XrmTools.Meta.Model;

public interface ILocalDependencyAnalyzer
{
    Dependency Analyze(INamedTypeSymbol rootType, Compilation compilation);
}


[Export(typeof(ILocalDependencyAnalyzer))]
public class LocalDependencyAnalyzer : ILocalDependencyAnalyzer
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

            if (SymbolEqualityComparer.Default.Equals(prop.Type, serviceProvider))
            {
                node.Dependencies.Add(new()
                {
                    PropertyName = prop.Name,
                    FullTypeName = serviceProvider.ToDisplayString(),
                    ShortTypeName = serviceProvider.Name,
                    Dependencies = [],
                    IsProperty = true
                });
                continue;
            }

            var propType = ResolveType(prop.Type, implementationMap);

            if (propType is not INamedTypeSymbol depSymbol || depSymbol.ContainingType != null)
                continue;

            //if (!dependedOn.Add(depSymbol))
            //    continue;

            //var childNode = BuildNode(depSymbol, prop.Name, compilation, implementationMap, dependedOn);

            //childNode.IsProperty = true;
            //node.Dependencies.Add(childNode);

            //INSTEAD:

            // Recursive analysis — per branch copy of dependedOn
            if (!dependedOn.Contains(depSymbol))
            {
                var newDependedOn = new HashSet<INamedTypeSymbol>(dependedOn, SymbolEqualityComparer.Default)
                {
                    depSymbol
                };

                var childNode = BuildNode(depSymbol, prop.Name, compilation, implementationMap, newDependedOn);
                childNode.IsProperty = true;
                node.Dependencies.Add(childNode);
            }
        }

        // Analyze constructor parameters
        var ctor = SelectConstructor(classSymbol);
        if (ctor is not null)
        {
            foreach (var param in ctor.Parameters)
            {
                if (SymbolEqualityComparer.Default.Equals(param.Type, serviceProvider))
                {
                    node.Dependencies.Add(new Dependency
                    {
                        PropertyName = param.Name,
                        FullTypeName = param.Type.ToDisplayString(),
                        ShortTypeName = param.Name,
                        IsProperty = false,
                    });
                    continue;
                }

                var paramType = ResolveType(param.Type, implementationMap);


                if (paramType is not INamedTypeSymbol depSymbol || depSymbol.ContainingType != null)
                    continue;

                //if (!dependedOn.Add(depSymbol))
                //    continue;

                //var childNode = BuildNode(depSymbol, param.Name, compilation, implementationMap, dependedOn);
                //childNode.IsProperty = false;
                //node.Dependencies.Add(childNode);

                //INSTEAD:
                // Recursive analysis — per branch copy of dependedOn
                if (!dependedOn.Contains(depSymbol))
                {
                    var newDependedOn = new HashSet<INamedTypeSymbol>(dependedOn, SymbolEqualityComparer.Default)
                    {
                        depSymbol
                    };

                    var childNode = BuildNode(depSymbol, param.Name, compilation, implementationMap, newDependedOn);
                    childNode.IsProperty = false;
                    node.Dependencies.Add(childNode);
                }
            }
        }

        return node;
    }

    private ITypeSymbol ResolveType(ITypeSymbol originalType, Dictionary<ITypeSymbol, INamedTypeSymbol> implementationMap)
    {
        if (originalType is null)
            return originalType;

        if (implementationMap.TryGetValue(originalType, out var impl))
            return impl;

        return originalType;
    }

    private IMethodSymbol? SelectConstructor(INamedTypeSymbol classSymbol)
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

        // Process current project
        ProcessNamespace(compilation.Assembly.GlobalNamespace);

        // Process referenced projects
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