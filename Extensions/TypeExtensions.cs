#nullable enable
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace XrmGen.Extensions;

internal static class TypeExtensions
{
    internal static string GetEntityLogicalName(this Type t)
    {
        var entityLogicalName = t.GetCustomAttributes(typeof(EntityLogicalNameAttribute), true).FirstOrDefault() as EntityLogicalNameAttribute;
        return entityLogicalName?.LogicalName ?? throw new InvalidOperationException($"EntityLogicalNameAttribute not found on {t.Name}");
    }

    /// <summary>
    /// Get the AttributeLogicalName of a property. Example Usage: <code>var logicalName = GetAttributeLogicalName((PluginType x) => x.Name);</code>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="instance"></param>
    /// <param name="expression"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    internal static string GetAttributeLogicalName<T>(this T instance, Expression<Func<T, object>> expression)
    {
        if (expression.Body is not MemberExpression memberExpression)
        {
            throw new ArgumentException("Expression must be a MemberExpression", nameof(expression));
        }
        var attributeLogicalName = memberExpression.Member.GetCustomAttributes(typeof(AttributeLogicalNameAttribute), true).FirstOrDefault() as AttributeLogicalNameAttribute;
        return attributeLogicalName?.LogicalName ?? throw new InvalidOperationException($"AttributeLogicalNameAttribute not found on {memberExpression.Member.Name}");
    }
}
#nullable restore