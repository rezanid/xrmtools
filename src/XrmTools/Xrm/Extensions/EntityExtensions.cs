#nullable enable
namespace XrmTools.Xrm.Extensions;

using Microsoft.Xrm.Sdk;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

internal static class EntityExtensions
{
    internal static Entity? FromAliasedAttributes([DisallowNull] this Entity entity, string alias)
    {
        var prefix = alias + ".";
        var dealiased = entity.Attributes
            .Where(a => a.Key.StartsWith(alias, StringComparison.OrdinalIgnoreCase))
            .Aggregate(new Entity(), (acc, a) =>
            {
                if (a.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) && a.Value is AliasedValue aliasedValue)
                {
                    acc[a.Key[prefix.Length..]] = aliasedValue.Value;
                }
                return acc;
            });
        return entity.Attributes.Count > 0 ? entity : null;
    }
}
#nullable disable