#nullable enable
namespace XrmTools.Helpers;

using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using XrmTools.Xrm.Model;
using XrmTools.Xrm.Serialization;

internal static class StringHelpers
{
    // System.Text.Json Configuration:
    private static readonly JsonSerializerOptions jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver
        {
            Modifiers = { DefaultValueModifier } //, IgnoreInheritedEntityPropertiesModifier }
        },
        Converters = { new IgnoreEntityPropertiesConverter() }
    };
    // Newtonsoft Configuration:
    private static readonly JsonSerializerSettings jsonSerializerSettings = new()
    {
        TypeNameHandling = TypeNameHandling.Auto,
        NullValueHandling = NullValueHandling.Ignore,
        ContractResolver = new IgnoreEntityPropertiesResolver()
    };

    public static string LastSegment(this string str, char separator = '.')
    {
        var lastSeparator = str.LastIndexOf(separator);
        return lastSeparator == -1 ? str : str[(lastSeparator + 1)..];
    }

    public static string FirstSegment(this string str, char separator = '.')
    {
        var firstSeparator = str.IndexOf(separator);
        return firstSeparator == -1 ? str : str[..firstSeparator];
    }

    /// <summary>
    /// Satisfy NRT checks by ensuring a null string is never propagated
    /// </summary>
    /// <remarks>
    /// Various legacy APIs still return nullable strings (even if, in practice they
    /// never will actually be null) so we can use this extension to keep the NRT
    /// checks quiet</remarks>
    public static string EmptyWhenNull(this string? str) => str ?? string.Empty;

    public static T DeserializeJson<T>([DisallowNull] this string json, bool useNewtonsoft = false)
        => useNewtonsoft ?
        JsonConvert.DeserializeObject<T>(json, jsonSerializerSettings)! :
        System.Text.Json.JsonSerializer.Deserialize<T>(json, jsonSerializerOptions)!;

    public static string SerializeJson([DisallowNull] this object obj, bool useNewtonsoft = false) 
        => useNewtonsoft ?
        JsonConvert.SerializeObject(obj, jsonSerializerSettings) :
        System.Text.Json.JsonSerializer.Serialize(obj, jsonSerializerOptions);

    private static void DefaultValueModifier([DisallowNull]JsonTypeInfo type_info)
    {
        foreach (var property in type_info.Properties)
        {
            if (typeof(ICollection).IsAssignableFrom(property.PropertyType))
            {
                property.ShouldSerialize = (_, val) => val is ICollection collection && collection.Count > 0;
            }
            if (property.PropertyType.DeclaringType == typeof(Entity))
            {
                property.ShouldSerialize = (_,_) => false;
            }
        }
    }

    /// <summary>
    /// This method is written for demonstration only. The problem in this approach is that `DeclaringType` is always
    /// `null` which seems to be a bug, so we have to rely on a fully custom `TypeConversion` unfortunately.
    /// </summary>
    /// <param name="typeInfo"></param>
    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Demonstration Only")]
    private static void IgnoreInheritedEntityPropertiesModifier([DisallowNull] JsonTypeInfo typeInfo)
    {
        if (IsSubclassOfRawGeneric(typeof(TypedEntity<>), typeInfo.Type))
        {
            foreach (var property in typeInfo.Properties)
            {
                if (property.PropertyType.DeclaringType != typeInfo.Type)
                {
                    property.ShouldSerialize = (_, _) => false;
                }
            }
        }
    }

    private static bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
    {
        while (toCheck != null && toCheck != typeof(object))
        {
            var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
            if (generic == cur)
            {
                return true;
            }
            toCheck = toCheck.BaseType;
        }
        return false;
    }
}
#nullable restore