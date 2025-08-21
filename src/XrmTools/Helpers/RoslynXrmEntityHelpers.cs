#nullable enable
namespace XrmTools.Helpers;

using Microsoft.CodeAnalysis;
using System.Linq;

public static class RoslynXrmEntityHelpers
{
    public static bool IsXrmEntityLike(this ITypeSymbol? type, Compilation compilation)
    {
        var entity = GetXrmEntitySymbol(compilation);
        if (type is null || entity is null) return false;

        return IsEntityLikeOrConstrained(type, entity);
    }

    /// <summary>
    /// Returns true if 'type' is IEnumerable&lt;T&gt; (or optionally an array) where T is Entity or derives from Entity.
    /// If true, also returns the element type in 'elementType'.
    /// </summary>
    public static bool IsEnumerableOfXrmEntityLike(
        this ITypeSymbol? type,
        Compilation compilation,
        out ITypeSymbol? elementType,
        bool includeArrays = true)
    {
        elementType = null;
        var entity = GetXrmEntitySymbol(compilation);
        if (type is null || entity is null) return false;

        var elem = type.TryGetEnumerableElementType(compilation, includeArrays);
        if (elem is null) return false;

        if (IsEntityLikeOrConstrained(elem, entity))
        {
            elementType = elem;
            return true;
        }
        return false;
    }

    private static INamedTypeSymbol? GetXrmEntitySymbol(Compilation compilation) =>
        compilation.GetTypeByMetadataName("Microsoft.Xrm.Sdk.Entity");

    private static bool IsEntityLikeOrConstrained(ITypeSymbol type, INamedTypeSymbol entityBase)
    {
        // Concrete/class case
        if (type is INamedTypeSymbol named)
            return named.DerivesFromOrEquals(entityBase);

        // Generic parameter case: any class/interface constraint can be checked
        if (type is ITypeParameterSymbol tp)
            return tp.ConstraintTypes.Any(ct =>
                ct is INamedTypeSymbol n && n.DerivesFromOrEquals(entityBase));

        return false;
    }
}
#nullable restore