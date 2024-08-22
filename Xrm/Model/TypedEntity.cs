#nullable enable
namespace XrmGen.Xrm.Model;

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

public interface ITypedEntity { }

public abstract class TypedEntity<T>(string entityLogicalName) : Entity(entityLogicalName), ITypedEntity where T : Entity, ITypedEntity, new()
{
    internal static string[] GetColumnsFromExpression(Expression<Func<T, object>> expression) =>
        expression.Body switch
        {
            NewExpression newExpr => newExpr.Arguments.OfType<MemberExpression>()
                                                      .Select(GetMemberName)
                                                      .ToArray(),
            MemberExpression memberExpr => [GetMemberName(memberExpr)],
            UnaryExpression { Operand: MemberExpression memberExpr } => [GetMemberName(memberExpr)],
            _ => []
        };


    internal static string GetEntityLogicalName()
    {
        var entityType = typeof(T);
        var entityLogicalName = entityType.GetCustomAttributes(typeof(EntityLogicalNameAttribute), true)
            .OfType<EntityLogicalNameAttribute>()
            .FirstOrDefault();

        return entityLogicalName?.LogicalName ?? throw new InvalidOperationException($"EntityLogicalNameAttribute not found on {entityType.Name}");
    }

    internal static string GetAliasedAttributeName(Expression<Func<T, object>> expression)
    {
        if (expression.Body is not MemberExpression memberExpression)
        {
            throw new ArgumentException("Invalid expression");
        }

        // Extract attribute logical name
        var attributeLogicalName = memberExpression.Member.GetCustomAttributes(typeof(AttributeLogicalNameAttribute), true)
            .OfType<AttributeLogicalNameAttribute>()
            .FirstOrDefault();

        // Extract entity logical name
        var entityType = typeof(T);
        var entityLogicalName = entityType.GetCustomAttributes(typeof(EntityLogicalNameAttribute), true)
            .OfType<EntityLogicalNameAttribute>()
            .FirstOrDefault();

        return 
            GetEntityLogicalName()
            + "." 
            + (attributeLogicalName?.LogicalName ?? throw new InvalidOperationException($"AttributeLogicalNameAttribute not found on {memberExpression.Member.Name}"));
    }

    internal static T? FromAlias(Entity entity)
    {
        string alias = GetEntityLogicalName();
        var t = new T();
        foreach (var attribute in entity.Attributes)
        {
            if (attribute.Key.StartsWith(alias))
            {
                t.Attributes.Add(attribute.Key[(alias.Length + 1)..], (attribute.Value as AliasedValue)?.Value);
            }
        }
        if (t.Attributes.Count == 0)
        {
            return null;
        }
        return t;
    }

    private static string GetMemberName(MemberExpression memberExpr) =>
        memberExpr.Member.GetCustomAttribute<AttributeLogicalNameAttribute>()?.LogicalName
        ?? memberExpr.Member.Name;
}
#nullable restore