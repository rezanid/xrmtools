namespace XrmTools.Serialization;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

internal class PolymorphicContractResolver : DefaultContractResolver
{
    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        var property = base.CreateProperty(member, memberSerialization);

        if (IsPolymorphicType(property.PropertyType))
        {
            property.Converter = new KnownTypeConverter(property.PropertyType);
        }

        return property;
    }

    private bool IsPolymorphicType(Type type) => type.IsArray ?
        type.GetElementType().GetCustomAttributes<KnownTypeAttribute>(true).Any(attr => attr.Type != type) :
        type.GetCustomAttributes<KnownTypeAttribute>(true).Any(attr => attr.Type != type);
}
