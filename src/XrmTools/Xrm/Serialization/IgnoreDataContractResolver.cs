#nullable enable
namespace XrmTools.Xrm.Serialization;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Reflection;
using Microsoft.Xrm.Sdk;

public class IgnoreEntityPropertiesResolver : DefaultContractResolver
{
    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        var property = base.CreateProperty(member, memberSerialization);
        // Check if the property is declared in the base Entity class
        if (typeof(Entity).IsAssignableFrom(member.DeclaringType) && member.DeclaringType == typeof(Entity))
        {
            property.Ignored = true;
        }
        else
        {
            // Allow properties in derived classes to be serialized
            property.Ignored = false; 
        }
        return property;
    }
}

#nullable restore