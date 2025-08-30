using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using XrmGenTest;
using XrmTools.Meta.Attributes;

[assembly: Entity("attachment", BaseType = typeof(BaseEntity<Attachment>))]
[assembly: Entity("account", BaseType = typeof(BaseEntity<Account>))]
[assembly: Entity("contact")]
   
namespace XrmGenTest
{
    public class BaseEntity<T> : Entity where T : BaseEntity<T>, new()
    {
        protected static readonly string _entityLogicalName = GetEntityLogicalName();
        protected static readonly Dictionary<string, string> AttributeSchemaNameMapping = [];
        protected static readonly object _attributeSchemaNameMappingLock = new();

        public BaseEntity() : base(_entityLogicalName) { }
        public BaseEntity(Guid id) : base(_entityLogicalName, id) { }
        public BaseEntity(string keyName, object keyValue) : base(_entityLogicalName, keyName, keyValue) { }
        public BaseEntity(KeyAttributeCollection keyAttributes) : base(_entityLogicalName, keyAttributes) { }
        public BaseEntity(string logicalName) : base(logicalName) { }
        public BaseEntity(string logicalName, Guid id) : base(logicalName, id) { }
        public BaseEntity(string logicalName, string keyName, object keyValue) : base(logicalName, keyName, keyValue) { }
        public BaseEntity(string logicalName, KeyAttributeCollection keyAttributes) : base(logicalName, keyAttributes) { }

        public Entity ToEntity()
        {
            return new Entity(LogicalName)
            {
                Id = Id,
                Attributes = Attributes,
                RowVersion = RowVersion,
                KeyAttributes = KeyAttributes
            };
        }

        public static class ColumnSet
        {
            public static Microsoft.Xrm.Sdk.Query.ColumnSet AllColumns => new(true);
            public static Microsoft.Xrm.Sdk.Query.ColumnSet NoColumns => new(false);
            public static Microsoft.Xrm.Sdk.Query.ColumnSet Of(Expression<Func<T, object>> expression) =>
                expression.Body switch
                {
                    NewExpression newExpr => new([.. newExpr.Arguments.OfType<MemberExpression>().Select(GetMemberAttributeLogicalName)]),
                    MemberExpression memberExpr => new([GetMemberAttributeLogicalName(memberExpr)]),
                    UnaryExpression { Operand: MemberExpression memberExpr } => new([GetMemberAttributeLogicalName(memberExpr)]),
                    _ => NoColumns
                };
            public static Microsoft.Xrm.Sdk.Query.ColumnSet Of(params Expression<Func<T, object>>[] expressions) =>
                new([.. expressions.SelectMany(e => Of(e).Columns)]);
            public static string ColumnName(Expression<Func<T, object>> expression) =>
                expression.Body switch
                {
                    MemberExpression memberExpr => GetMemberAttributeLogicalName(memberExpr),
                    UnaryExpression { Operand: MemberExpression memberExpr } => GetMemberAttributeLogicalName(memberExpr),
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

            return
                _entityLogicalName
                + "."
                + (attributeLogicalName?.LogicalName ?? throw new InvalidOperationException($"AttributeLogicalNameAttribute not found on {memberExpression.Member.Name}"));
        }

        public static QueryExpression CreateQuery(Expression<Func<T, object>> expression)
            => new(_entityLogicalName) { ColumnSet = ColumnSet.Of(expression) };
        public static QueryExpression CreateQuery(params Expression<Func<T, object>>[] expressions)
            => new(_entityLogicalName) { ColumnSet = ColumnSet.Of(expressions) };

        /// <summary>
        /// Creates an entity of type <typeparamref name="T"/> and populates its attributes with values
        /// from the given <see cref="Entity"/> that match the specified alias.
        /// </summary>
        /// <remarks>This method uses <typeparamref name="T"/>'s predefined logical name as the alias to
        /// find aliased attributes in the <paramref name="entity"/>. If you need to use a different alias,
        /// call <see cref="FromAliasedAttributes(Entity, string)"/> with the desired alias.</remarks>
        /// </remarks>
        /// <param name="entity">The <see cref="Entity"/> object containing the aliased attributes.</param>
        /// <returns>An instance of type <typeparamref name="T"/> if any aliased attribute is found; otherwise, <see langword="null"/>.</returns>
        public static T? FromAliasedAttributes(Entity entity) => FromAliasedAttributes(entity, _entityLogicalName);

        /// <summary>
        /// Creates an instance of type <typeparamref name="T"/> and populates its attributes with values
        /// from the given <see cref="Entity"/> that match the specified alias.
        /// </summary>
        /// <remarks>This method extracts attributes from the provided <paramref name="entity"/> where the keys 
        /// start with the specified <paramref name="aliasPrefix"/> followed by a period ("."). The extracted  attributes are
        /// added to the <c>Attributes</c> property of the created <typeparamref name="T"/> instance, with the alias prefix
        /// removed from the keys.</remarks>
        /// <param name="entity">The <see cref="Entity"/> containing the attributes to extract.</param>
        /// <param name="aliasPrefix">The alias prefix used to filter attributes. Only attributes with keys starting with <paramref name="aliasPrefix"/>
        /// followed by a period (".") will be considered.</param>
        /// <returns>An instance of <typeparamref name="T"/> with its attributes populated from the matching  attributes in the
        /// <paramref name="entity"/>. Returns <see langword="null"/> if no matching  attributes are found.</returns>
        public static T? FromAliasedAttributes(Entity entity, string aliasPrefix)
        {
            var prefix = aliasPrefix + ".";
            var t = new T();
            foreach (var attribute in entity.Attributes)
            {
                if (attribute.Key.StartsWith(prefix) && attribute.Value is AliasedValue aliasedValue)
                {
                    t.Attributes.Add(attribute.Key[prefix.Length..], aliasedValue.Value);
                }
            }
            return t.Attributes.Count == 0 ? null : t;
        }

        private static string GetMemberAttributeLogicalName(MemberExpression memberExpr)
        {
            var memberName = memberExpr.Member.Name;

            if (memberName == "Id") return _entityLogicalName + "id";

            if (AttributeSchemaNameMapping.TryGetValue(memberName, out var logicalName))
            {
                return logicalName;
            }

            // This should never happen, but if it does, we can still try to get the logical name
            lock (_attributeSchemaNameMappingLock)
            {
                if (AttributeSchemaNameMapping.TryGetValue(memberName, out logicalName))
                {
                    return logicalName;
                }
                else
                {
                    logicalName = memberExpr.Member.GetCustomAttribute<AttributeLogicalNameAttribute>()?.LogicalName;
                    if (string.IsNullOrEmpty(logicalName))
                    {
                        throw new InvalidOperationException($"{nameof(AttributeLogicalNameAttribute)} not found on {memberExpr.Member.Name}");
                    }
                    AttributeSchemaNameMapping[memberName] = logicalName!;
                }
            }

            return logicalName!;
        }
    }

    public partial class Account : BaseEntity<Account>
    {
        public static partial class Meta
        {
            public static partial class OptionSets
            {
                public static string[] GetCategoryNames() => Enum.GetNames(typeof(Category));
            }
        }
    }

}