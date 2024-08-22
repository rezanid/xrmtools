namespace XrmGen.Xrm.Extensions;

using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal static class EntityExtensions
{
    internal static Entity FromAlias(this Entity entity, string alias)
    {
        if (entity.LogicalName == alias)
        {
            return entity;
        }

        if (entity.Contains(alias))
        {
            return entity.GetAttributeValue<Entity>(alias);
        }
        var prefix = alias + ".";
        var dealiased = entity.Attributes
            .Where(a => a.Key.StartsWith(alias, StringComparison.OrdinalIgnoreCase))
            .Aggregate(new Entity(), (acc, a) =>
            {
                var key = a.Key;
                if (key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    key = key.Substring(prefix.Length);
                }
                acc[key] = a.Value;
                return acc;
            });

        throw new InvalidOperationException($"Entity does not contain an attribute with alias '{alias}'");
    }
}
