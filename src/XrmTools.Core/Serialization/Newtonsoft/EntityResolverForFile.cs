#nullable enable
namespace XrmTools.Core.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Reflection;
using XrmTools.Meta.Attributes.Serialization;

public class EntityResolverForFile : IgnoreEntityPropertiesResolver
{
    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        var property = base.CreateProperty(member, memberSerialization);
        if (member.GetCustomAttribute<JsonPrimaryKeyAttribute>() is JsonPrimaryKeyAttribute attribute)
        {
            property.PropertyName = attribute.Name;
        }
        return property;
    }
}
#nullable restore