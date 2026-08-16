#nullable enable
namespace XrmTools.Helpers;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

public static class RoslynHelpers
{
    /// <summary>
    /// Determines whether the specified type declaration contains a member with the given name.
    /// </summary>
    /// <remarks>This method checks for members including methods, properties, events, fields, and nested
    /// types. The comparison is case-sensitive and matches the exact identifier name.</remarks>
    /// <param name="type">The type declaration to search for members.</param>
    /// <param name="name">The name of the member to locate within the type declaration.</param>
    /// <returns>true if a member with the specified name exists in the type declaration; otherwise, false.</returns>
    public static bool HasMember(this TypeDeclarationSyntax type, string name)
    {
        foreach (var m in type.Members)
        {
            var id = m switch
            {
                BaseTypeDeclarationSyntax t => t.Identifier,
                MethodDeclarationSyntax mth => mth.Identifier,
                PropertyDeclarationSyntax p => p.Identifier,
                EventDeclarationSyntax ev => ev.Identifier,
                //FieldDeclarationSyntax f => default,
                _ => default
            };

            if (id != default && id.ValueText == name)
                return true;

            if (m is FieldDeclarationSyntax fd)
                foreach (var v in fd.Declaration.Variables)
                    if (v.Identifier.ValueText == name)
                        return true;
        }

        return false;
    }

    /// <summary>
    /// Returns true if the type is array or implements <see cref="System.Collections.IEnumerable"/>.
    /// By default, treats string as NOT enumerable (set includeString=true to change).
    /// </summary>
    public static bool IsEnumerableLike(this ITypeSymbol? type, Compilation compilation, bool includeString = false)
    {
        if (type is null)
            return false;

        // arrays are enumerable
        if (type is IArrayTypeSymbol)
            return true;

        // unwrap nullable annotations (NRT) — this doesn't change the symbol identity,
        // but we'll use OriginalDefinition where appropriate below.
        if (type is INamedTypeSymbol named)
        {
            if (!includeString && named.SpecialType == SpecialType.System_String)
                return false;

            var ienumerable = compilation.GetSpecialType(SpecialType.System_Collections_IEnumerable);
            var ienumerableOfTDef = compilation.GetSpecialType(SpecialType.System_Collections_Generic_IEnumerable_T);

            // The type itself is exactly IEnumerable
            if (SymbolEqualityComparer.Default.Equals(named, ienumerable))
                return true;

            // The type itself is (constructed) IEnumerable<T>
            if (SymbolEqualityComparer.Default.Equals(named.OriginalDefinition, ienumerableOfTDef))
                return true;

            // Or it implements either of them
            if (named.AllInterfaces.Any(i =>
                    SymbolEqualityComparer.Default.Equals(i, ienumerable) ||
                    SymbolEqualityComparer.Default.Equals(i.OriginalDefinition, ienumerableOfTDef)))
                return true;
        }

        return false;
    }

    public static ITypeSymbol? TryGetEnumerableElementType(this ITypeSymbol type, Compilation compilation, bool includeArrays)
    {
        if (includeArrays && type is IArrayTypeSymbol arr)
            return arr.ElementType;

        var ienumT = compilation.GetSpecialType(SpecialType.System_Collections_Generic_IEnumerable_T);

        if (type is INamedTypeSymbol named)
        {
            // The type itself is IEnumerable<T>
            if (SymbolEqualityComparer.Default.Equals(named.OriginalDefinition, ienumT))
                return named.TypeArguments[0];

            // Or it implements IEnumerable<T>
            foreach (var i in named.AllInterfaces)
                if (SymbolEqualityComparer.Default.Equals(i.OriginalDefinition, ienumT))
                    return i.TypeArguments[0];
        }

        // If the collection itself is a type parameter, see if any constraint is IEnumerable<T>
        if (type is ITypeParameterSymbol tp)
        {
            foreach (var c in tp.ConstraintTypes)
            {
                var elem = TryGetEnumerableElementType(c, compilation, includeArrays);
                if (elem is not null) return elem;
            }
        }

        return null;
    }

    public static bool DerivesFromOrEquals(this INamedTypeSymbol type, INamedTypeSymbol baseClass)
    {
        for (INamedTypeSymbol? t = type; t is not null; t = t.BaseType)
            if (SymbolEqualityComparer.Default.Equals(t, baseClass))
                return true;
        return false;
    }

    /// <summary>
    /// Gets all project types that are annotated with the specified attribute.
    /// </summary>
    /// <param name="compilation">The compilation to search.</param>
    /// <param name="attributeMetadataName">The metadata name of the attribute to search for.</param>
    /// <param name="includeDerivedAttributeTypes">If true, include types annotated with attributes that derive from the specified attribute.</param>
    /// <param name="includeNestedTypes">If true, include nested types in the search; otherwise, only top-level types are considered.</param>
    /// <returns>An enumeration of type symbols that have the specified attribute.</returns>
    public static IEnumerable<INamedTypeSymbol> GetProjectTypesWithAttribute(
        this Compilation compilation,
        string attributeMetadataName,
        bool includeDerivedAttributeTypes = false,
        bool includeNestedTypes = false)
    {
        var attrSymbol = compilation.GetTypeByMetadataName(attributeMetadataName);
        if (attrSymbol is null) yield break;

        foreach (var type in EnumerateTypes(compilation.Assembly.GlobalNamespace, includeNestedTypes))
        {
            foreach (var a in type.GetAttributes())
            {
                var aClass = a.AttributeClass;
                if (aClass is null) continue;

                if (SymbolEqualityComparer.Default.Equals(aClass, attrSymbol) ||
                    (includeDerivedAttributeTypes && InheritsFrom(aClass, attrSymbol)))
                {
                    yield return type;
                    break;
                }
            }
        }
    }

    private static IEnumerable<INamedTypeSymbol> EnumerateTypes(INamespaceSymbol ns, bool includeNestedTypes)
    {
        foreach (var t in ns.GetTypeMembers())
        {
            yield return t;

            if (includeNestedTypes)
            {
                foreach (var n in EnumerateNestedTypes(t))
                    yield return n;
            }
        }

        foreach (var child in ns.GetNamespaceMembers())
        {
            foreach (var t in EnumerateTypes(child, includeNestedTypes))
                yield return t;
        }
    }

    private static IEnumerable<INamedTypeSymbol> EnumerateNestedTypes(INamedTypeSymbol t)
    {
        foreach (var n in t.GetTypeMembers())
        {
            yield return n;

            foreach (var nn in EnumerateNestedTypes(n))
                yield return nn;
        }
    }

    private static bool InheritsFrom(INamedTypeSymbol? type, INamedTypeSymbol baseType)
    {
        for (var cur = type; cur is not null; cur = cur.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(cur, baseType))
                return true;
        }
        return false;
    }
}
#nullable restore