#nullable enable
namespace XrmTools;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using XrmTools.WebApi.Entities;

/// <summary>
/// Helpers that make generated member names valid C# identifiers within the context of the
/// type that contains them.
/// </summary>
internal static class CodeGenNaming
{
    /// <summary>
    /// Default suffix appended to a generated member (property) name when it would otherwise be
    /// identical to the name of its enclosing type. C# forbids a member from having the same name as
    /// the type that contains it (compiler error CS0542), which happens when a Dataverse column
    /// shares the logical/schema name of its table (e.g. a table "xxx_postalcode" with a column
    /// "xxx_postalcode"). Can be overridden per-assembly via <c>[assembly: CodeGenNameCollisionSuffix(...)]</c>.
    /// </summary>
    private const string DefaultEnclosingTypeCollisionSuffix = "Value";

    /// <summary>
    /// Ensures no attribute's schema name (which becomes a generated property name) is identical to
    /// the entity's schema name (which becomes the enclosing type name). Colliding attributes are
    /// renamed by appending <paramref name="suffix"/> (defaulting to "Value" when null/empty),
    /// disambiguating with a numeric suffix if the resulting name is already taken by another
    /// attribute.
    /// <para>
    /// This must run after the entity and attribute schema names have been formatted (prefixes
    /// removed and first letter capitalized), so the comparison reflects the names that will
    /// actually be emitted.
    /// </para>
    /// </summary>
    public static void ResolveEnclosingTypeNameCollisions(EntityMetadata? entityDefinition, IEnumerable<AttributeMetadata>? attributes, string? suffix = null)
    {
        if (entityDefinition?.SchemaName is not { Length: > 0 } typeName || attributes is null) return;

        var effectiveSuffix = string.IsNullOrWhiteSpace(suffix) ? DefaultEnclosingTypeCollisionSuffix : suffix!.Trim();

        var attributeList = attributes as IList<AttributeMetadata> ?? attributes.ToList();

        var usedNames = new HashSet<string>(StringComparer.Ordinal) { typeName };
        foreach (var attribute in attributeList)
        {
            if (attribute.SchemaName is { Length: > 0 } schemaName)
            {
                usedNames.Add(schemaName);
            }
        }

        foreach (var attribute in attributeList)
        {
            if (!string.Equals(attribute.SchemaName, typeName, StringComparison.Ordinal)) continue;

            var candidate = typeName + effectiveSuffix;
            var counter = 1;
            while (usedNames.Contains(candidate))
            {
                candidate = typeName + effectiveSuffix + counter.ToString(CultureInfo.InvariantCulture);
                counter++;
            }

            attribute.SchemaName = candidate;
            usedNames.Add(candidate);
        }
    }
}
#nullable restore
