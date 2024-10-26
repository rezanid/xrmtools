#nullable enable
namespace XrmTools.Xrm.Model;

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

public interface ITypedEntity { }

public abstract class TypedEntity<T>(string entityLogicalName) : Entity(entityLogicalName), ITypedEntity where T : Entity, ITypedEntity, new()
{
    public static class Select
    {
        public static ColumnSet AllColumns => new(true);
        public static ColumnSet NoColumns => new(false);
        public static ColumnSet From(Expression<Func<T, object>> expression) =>
            expression.Body switch
            {
                NewExpression newExpr => new(newExpr.Arguments.OfType<MemberExpression>()
                                                          .Select(GetMemberName)
                                                          .ToArray()),
                MemberExpression memberExpr => new([GetMemberName(memberExpr)]),
                UnaryExpression { Operand: MemberExpression memberExpr } => new([GetMemberName(memberExpr)]),
                _ => NoColumns
            };
        public static ColumnSet From(params Expression<Func<T, object>>[] expressions) =>
            new (expressions.SelectMany(e => From(e).Columns).ToArray());
        public static string ColumnName(Expression<Func<T, object>> expression) =>
            expression.Body switch
            {
                MemberExpression memberExpr => GetMemberName(memberExpr),
                UnaryExpression { Operand: MemberExpression memberExpr } => GetMemberName(memberExpr),
                _ => throw new ArgumentException("Invalid expression")
            };
    }


    public static string GetEntityLogicalName()
    {
        var entityType = typeof(T);
        var entityLogicalName = entityType.GetCustomAttributes(typeof(EntityLogicalNameAttribute), true)
            .OfType<EntityLogicalNameAttribute>()
            .FirstOrDefault();

        return entityLogicalName?.LogicalName ?? throw new InvalidOperationException($"EntityLogicalNameAttribute not found on {entityType.Name}");
    }
    
    public static string GetAliasedAttributeName(Expression<Func<T, object>> expression)
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

    public static QueryExpression CreateQuery(Expression<Func<T, object>> expression)
        => new(GetEntityLogicalName()) { ColumnSet = Select.From(expression)};

    public static T? FromAlias(Entity entity)
    {
        string aliasPrefix = GetEntityLogicalName() + ".";
        var t = new T();
        foreach (var attribute in entity.Attributes)
        {
            if (attribute.Key.StartsWith(aliasPrefix) && attribute.Value is AliasedValue aliasedValue)
            {
                t.Attributes.Add(attribute.Key[(aliasPrefix.Length)..], aliasedValue.Value);
            }
        }
        return (t.Attributes.Count == 0) ? null : t;
    }

    private static string GetMemberName(MemberExpression memberExpr) =>
        memberExpr.Member.GetCustomAttribute<AttributeLogicalNameAttribute>()?.LogicalName
        ?? (memberExpr.Member.Name is string name && name == "Id" ? GetEntityLogicalName() + "id" : "");


}
#nullable restore