#nullable enable
namespace XrmTools.Helpers;

using System.Linq;
using Microsoft.CodeAnalysis;

public static class RoslynHelpers
{
    /// <summary>
    /// Returns true if the type is array or implements IEnumerable / IEnumerable&lt;T&gt;.
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
}
#nullable restore