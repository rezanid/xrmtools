#nullable enable
namespace XrmTools.Analyzers;

using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using XrmTools.Meta.Attributes;
using XrmTools.Meta.Model;

public interface ICSharpDependencyAnalyzer
{
    Dependency Analyze(INamedTypeSymbol rootType, Compilation compilation);
}


[Export(typeof(ICSharpDependencyAnalyzer))]
public class CSharpDependencyAnalyzer : ICSharpDependencyAnalyzer
{
    private INamedTypeSymbol? serviceProviderType;

    public Dependency Analyze(INamedTypeSymbol rootType, Compilation compilation)
    {
        serviceProviderType = compilation.GetTypeByMetadataName("System.IServiceProvider");
        var implementationMap = BuildImplementationMap(compilation);
        var providerProperties = DiscoverProviderProperties(rootType);
        var dependedOn = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
        return BuildNode(rootType, null, compilation, implementationMap, dependedOn, providerProperties);
    }

    private Dependency BuildNode(
        INamedTypeSymbol classSymbol,
        string? propertyName,
        Compilation compilation,
        Dictionary<ITypeSymbol, INamedTypeSymbol> implementationMap,
        HashSet<INamedTypeSymbol> dependedOn,
        Dictionary<ITypeSymbol, string> providerProperties
        )
    {
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

            if (SymbolEqualityComparer.Default.Equals(prop.Type, serviceProviderType))
            {
                node.Dependencies.Add(new()
                {
                    PropertyName = prop.Name,
                    FullTypeName = serviceProviderType.ToDisplayString(),
                    ShortTypeName = serviceProviderType.Name,
                    Dependencies = [],
                    IsProperty = true
                });
                continue;
            }

            var propResolvedType = ResolveType(prop.Type, implementationMap);

            if (propResolvedType is not INamedTypeSymbol depSymbol || depSymbol.ContainingType != null)
                continue;

            //var existingPropertyName = FindExistingProperty(classSymbol, propType);

            if (providerProperties.TryGetValue(prop.Type, out var existingPropertyName) ||
                providerProperties.TryGetValue(propResolvedType, out existingPropertyName))
            {
                node.Dependencies.Add(new Dependency
                {
                    PropertyName = prop.Name,
                    FullTypeName = propResolvedType.ToDisplayString(),
                    ShortTypeName = propResolvedType.Name,
                    Dependencies = [],
                    IsProperty = true,
                    ProvidedByBaseProperty = existingPropertyName
                });
                continue;
            }

            // Recursive analysis — per branch copy of dependedOn
            if (!dependedOn.Contains(depSymbol))
            {
                var newDependedOn = new HashSet<INamedTypeSymbol>(dependedOn, SymbolEqualityComparer.Default)
                {
                    depSymbol
                };

                var childNode = BuildNode(depSymbol, prop.Name, compilation, implementationMap, newDependedOn, providerProperties);
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
                if (SymbolEqualityComparer.Default.Equals(param.Type, serviceProviderType))
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

                var paramResolvedType = ResolveType(param.Type, implementationMap);


                if (paramResolvedType is not INamedTypeSymbol depSymbol || depSymbol.ContainingType != null)
                    continue;

                //var existingPropertyName = FindExistingProperty(classSymbol, paramType);

                if (providerProperties.TryGetValue(param.Type, out var existingPropertyName) ||
                    providerProperties.TryGetValue(paramResolvedType, out existingPropertyName))
                {
                    node.Dependencies.Add(new Dependency
                    {
                        PropertyName = param.Name,
                        FullTypeName = paramResolvedType.ToDisplayString(),
                        ShortTypeName = paramResolvedType.Name,
                        Dependencies = [],
                        IsProperty = false,
                        ProvidedByBaseProperty = existingPropertyName
                    });
                    continue;
                }

                // Recursive analysis — per branch copy of dependedOn
                if (!dependedOn.Contains(depSymbol))
                {
                    var newDependedOn = new HashSet<INamedTypeSymbol>(dependedOn, SymbolEqualityComparer.Default)
                    {
                        depSymbol
                    };

                    var childNode = BuildNode(depSymbol, param.Name, compilation, implementationMap, newDependedOn, providerProperties);
                    childNode.IsProperty = false;
                    node.Dependencies.Add(childNode);
                }
            }
        }

        return node;
    }

    private string? FindExistingProperty(INamedTypeSymbol classSymbol, ITypeSymbol dependencyType)
    {
        INamedTypeSymbol? current = classSymbol;
        while (current != null && current.SpecialType != SpecialType.System_Object)
        {
            foreach (var member in current.GetMembers().OfType<IPropertySymbol>())
            {
                if (member.DeclaredAccessibility == Accessibility.Public &&
                    SymbolEqualityComparer.Default.Equals(member.Type, dependencyType))
                {
                    return member.Name;
                }
            }
            current = current.BaseType;
        }
        return null;
    }

    private Dictionary<ITypeSymbol, string> DiscoverProviderProperties(INamedTypeSymbol classSymbol)
    {
        var result = new Dictionary<ITypeSymbol, string>(SymbolEqualityComparer.Default);

        INamedTypeSymbol? current = classSymbol;
        while (current is not null && current.SpecialType != SpecialType.System_Object)
        {
            foreach (var member in current.GetMembers().OfType<IPropertySymbol>())
            {
                if (member.DeclaredAccessibility != Accessibility.Private && 
                    !member.IsWriteOnly && 
                    member.Type.TypeKind is TypeKind.Class or TypeKind.Interface &&
                    member.Type.SpecialType != SpecialType.System_String &&
                    !result.ContainsKey(member.Type) &&
                    member.GetAttributes().Any(attr => attr.AttributeClass?.ToDisplayString() == typeof(DependencyProviderAttribute).FullName)
                    )
                {
                    result[member.Type] = member.Name;
                }
            }
            current = current.BaseType;
        }

        return result;
    }

    private ITypeSymbol? ResolveType(ITypeSymbol originalType, Dictionary<ITypeSymbol, INamedTypeSymbol> implementationMap)
    {
        if (originalType is null)
            return null;

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