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
        return BuildNode(rootType, null, null, compilation, implementationMap, dependedOn, providerProperties);
    }

    private static string? TryGetDependencyNameFromAttributes(IEnumerable<AttributeData> attributes)
    {
        foreach (var attr in attributes)
        {
            if (attr.AttributeClass?.ToDisplayString() != typeof(DependencyAttribute).FullName)
                continue;

            foreach (var na in attr.NamedArguments)
            {
                if (na.Key == "Name" && na.Value.Value is string s)
                    return s;
            }

            if (attr.ConstructorArguments.Length > 0 && attr.ConstructorArguments[0].Value is string s2)
                return s2;
        }
        return null;
    }

    private Dependency BuildNode(
        INamedTypeSymbol typeSymbol,
        INamedTypeSymbol? originalTypeSymbol,
        string? propertyName,
        Compilation compilation,
        Dictionary<ITypeSymbol, INamedTypeSymbol> implementationMap,
        HashSet<INamedTypeSymbol> dependedOn,
        ProviderMap providerMap
        )
    {
        var node = new Dependency
        {
            PropertyName = propertyName,
            ResolvedFullTypeName = typeSymbol.ToDisplayString(),
            ResolvedShortTypeName = typeSymbol.Name,
            FullTypeName = originalTypeSymbol?.ToDisplayString() ?? typeSymbol.ToDisplayString(),
            ShortTypeName = originalTypeSymbol?.Name ?? typeSymbol.Name,
            Dependencies = [],
            IsDisposable = IsDisposable(typeSymbol)
        };

        // Analyze [Dependency] properties
        foreach (var prop in typeSymbol.GetMembers().OfType<IPropertySymbol>())
        {
            var depAttrs = prop.GetAttributes()
                               .Where(a => a.AttributeClass?.ToDisplayString() == typeof(DependencyAttribute).FullName)
                               .ToArray();
            if (depAttrs.Length == 0)
                continue;

            var depName = TryGetDependencyNameFromAttributes(depAttrs);
            if (SymbolEqualityComparer.Default.Equals(prop.Type, serviceProviderType))
            {
                node.Dependencies.Add(new()
                {
                    PropertyName = prop.Name,
                    FullTypeName = serviceProviderType.ToDisplayString(),
                    ShortTypeName = serviceProviderType.Name,
                    Dependencies = [],
                    IsProperty = true,
                    IsDisposable = IsDisposable(prop.Type as INamedTypeSymbol),
                    ProvidedByName = depName
                });
                continue;
            }

            var propResolvedType = ResolveType(prop.Type, implementationMap);
            if (propResolvedType is not INamedTypeSymbol depSymbol || depSymbol.ContainingType != null)
                continue;

            // Try provider property resolution (named preferred)
            if (providerMap.TryResolve(prop.Type, depName, out var providerPropertyName))
            {
                node.Dependencies.Add(new Dependency
                {
                    PropertyName = prop.Name,
                    FullTypeName = prop.Type.ToDisplayString(),
                    ShortTypeName = prop.Type.Name,
                    ResolvedFullTypeName = propResolvedType.ToDisplayString(),
                    ResolvedShortTypeName = propResolvedType.Name,
                    Dependencies = [],
                    IsProperty = true,
                    IsDisposable = IsDisposable(depSymbol),
                    ProvidedByProperty = providerPropertyName,
                    ProvidedByName = depName
                });
                continue;
            }

            if (!dependedOn.Contains(depSymbol))
            {
                var newDependedOn = new HashSet<INamedTypeSymbol>(dependedOn, SymbolEqualityComparer.Default)
                {
                    depSymbol
                };

                var childNode = BuildNode(depSymbol, prop.Type as INamedTypeSymbol, prop.Name, compilation, implementationMap, newDependedOn, providerMap);
                childNode.IsProperty = true;
                node.Dependencies.Add(childNode);
            }
        }

        // Analyze constructor parameters
        var ctor = SelectConstructor(typeSymbol);
        if (ctor is not null)
        {
            var stringIndex = 0; // root-level string #1 => Config, #2 => SecureConfig
            foreach (var param in ctor.Parameters)
            {
                var pDepName = TryGetDependencyNameFromAttributes(param.GetAttributes());

                if (SymbolEqualityComparer.Default.Equals(param.Type, serviceProviderType))
                {
                    node.Dependencies.Add(new Dependency
                    {
                        PropertyName = param.Name,
                        FullTypeName = param.Type.ToDisplayString(),
                        ShortTypeName = param.Name,
                        IsProperty = false,
                        IsDisposable = IsDisposable(param.Type as INamedTypeSymbol),
                        ProvidedByName = pDepName
                    });
                    continue;
                }

                if (param.Type.SpecialType == SpecialType.System_String && originalTypeSymbol is null && stringIndex < 2)
                {
                    var special = stringIndex == 0 ? "Config" : "SecureConfig";
                    stringIndex++;

                    node.Dependencies.Add(new Dependency
                    {
                        PropertyName = param.Name,
                        FullTypeName = param.Type.ToDisplayString(),
                        ShortTypeName = param.Type.Name,
                        ResolvedFullTypeName = param.Type.ToDisplayString(),
                        ResolvedShortTypeName = param.Type.Name,
                        Dependencies = [],
                        IsProperty = false,
                        IsDisposable = false,
                        ProvidedByName = pDepName ?? special
                    });
                    continue;
                }

                var paramResolvedType = ResolveType(param.Type, implementationMap);
                if (paramResolvedType is not INamedTypeSymbol depSymbol || depSymbol.ContainingType != null)
                    continue;

                if (providerMap.TryResolve(param.Type, pDepName, out var providerPropName))
                {
                    node.Dependencies.Add(new Dependency
                    {
                        PropertyName = param.Name,
                        FullTypeName = param.Type.ToDisplayString(),
                        ShortTypeName = param.Type.Name,
                        ResolvedFullTypeName = paramResolvedType.ToDisplayString(),
                        ResolvedShortTypeName = paramResolvedType.Name,
                        Dependencies = [],
                        IsProperty = false,
                        IsDisposable = IsDisposable(depSymbol),
                        ProvidedByProperty = providerPropName,
                        ProvidedByName = pDepName
                    });
                    continue;
                }

                if (!dependedOn.Contains(depSymbol))
                {
                    var newDependedOn = new HashSet<INamedTypeSymbol>(dependedOn, SymbolEqualityComparer.Default)
                    {
                        depSymbol
                    };

                    var childNode = BuildNode(depSymbol, param.Type as INamedTypeSymbol, param.Name, compilation, implementationMap, newDependedOn, providerMap);
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

    private sealed class ProviderInfo
    {
        public string PropertyName { get; set; }
        public string? Name { get; set; }
    }

    private sealed class ProviderMap
    {
        private readonly Dictionary<ITypeSymbol, List<ProviderInfo>> _byType;
        public ProviderMap(Dictionary<ITypeSymbol, List<ProviderInfo>> map) { _byType = map; }

        public bool TryResolve(ITypeSymbol type, string? name, out string providerPropertyName)
        {
            providerPropertyName = null;
            if (!_byType.TryGetValue(type, out var list)) return false;

            if (!string.IsNullOrEmpty(name))
            {
                var exact = list.FirstOrDefault(p => string.Equals(p.Name, name, System.StringComparison.Ordinal));
                if (exact != null) { providerPropertyName = exact.PropertyName; return true; }
            }

            // No name requested or exact not found: prefer unnamed first, otherwise first named
            var unnamed = list.FirstOrDefault(p => p.Name == null);
            if (unnamed != null) { providerPropertyName = unnamed.PropertyName; return true; }

            var any = list.FirstOrDefault();
            if (any != null) { providerPropertyName = any.PropertyName; return true; }

            return false;
        }
    }

    private ProviderMap DiscoverProviderProperties(INamedTypeSymbol classSymbol)
    {
        var result = new Dictionary<ITypeSymbol, List<ProviderInfo>>(SymbolEqualityComparer.Default);

        INamedTypeSymbol? current = classSymbol;
        while (current is not null && current.SpecialType != SpecialType.System_Object)
        {
            foreach (var member in current.GetMembers().OfType<IPropertySymbol>())
            {
                if (member.DeclaredAccessibility != Accessibility.Private && 
                    !member.IsWriteOnly && 
                    member.Type.TypeKind is TypeKind.Class or TypeKind.Interface &&
                    member.Type.SpecialType != SpecialType.System_String &&
                    member.GetAttributes().FirstOrDefault(attr => attr.AttributeClass?.ToDisplayString() == typeof(DependencyProviderAttribute).FullName) is AttributeData providerAttr)
                {
                    string? name = null;
                    if (providerAttr.ConstructorArguments.Length > 0 && providerAttr.ConstructorArguments[0].Value is string s)
                        name = s;
                    else
                    {
                        foreach (var na in providerAttr.NamedArguments)
                            if (na.Key == "Name" && na.Value.Value is string sn) { name = sn; break; }
                    }

                    if (!result.TryGetValue(member.Type, out var list))
                    {
                        list = new List<ProviderInfo>();
                        result[member.Type] = list;
                    }
                    list.Add(new ProviderInfo { PropertyName = member.Name, Name = name });
                }
            }
            current = current.BaseType;
        }

        return new ProviderMap(result);
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

    private static bool IsDisposable(INamedTypeSymbol? typeSymbol)
    {
        if (typeSymbol is null)
            return false;
        if (typeSymbol.SpecialType == SpecialType.System_Object)
            return false;
        if (typeSymbol.AllInterfaces.Any(i => i.ToDisplayString() == "System.IDisposable"))
            return true;
        return typeSymbol.BaseType?.SpecialType != SpecialType.System_Object && IsDisposable(typeSymbol.BaseType);
    }
}
#nullable restore