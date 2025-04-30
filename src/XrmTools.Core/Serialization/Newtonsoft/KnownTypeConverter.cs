namespace XrmTools.Serialization;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using XrmTools.Core.Helpers;

internal class KnownTypeConverter : JsonConverter
{
    private readonly Type _baseType;
    private static readonly ConcurrentDictionary<string, Type> AllKnownTypes = new();//new SuffixEqualityComparer());
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _typePropertiesCache = [];
    private static readonly ConcurrentDictionary<Type, Func<object>> AllKnownTypeFactories = [];

    public KnownTypeConverter(Type baseType)
    {
        _baseType = baseType;
        CacheKnownTypesAndFactories(_baseType);
    }
    public override bool CanRead => true;
    public override bool CanWrite => false;

    private void CacheKnownTypesAndFactories(Type type)
    {
        if (typeof(System.Collections.IEnumerable).IsAssignableFrom(type) &&
            type.IsGenericType)
        {
            var elementType = type.GetGenericArguments()[0];
            CacheKnownTypesAndFactories(elementType);
            return;
        }
        else if (type.IsArray)
        {
            var elementType = type.GetElementType();
            CacheKnownTypesAndFactories(elementType);
            return;
        }

        if (type.IsValueType || type.IsPrimitive || type == typeof(string) || !type.CanCreateInstanceUsingDefaultConstructor())
        {
            return;
        }
        if (_typePropertiesCache.ContainsKey(type))
        {
            return;
        }

        // Check if property has KnownTypeAttribute and cache
        var knownTypeAttributes = type.GetCustomAttributes<KnownTypeAttribute>(false).ToArray();
        foreach (var attribute in knownTypeAttributes)
        {
            if (attribute.Type != null && !AllKnownTypes.ContainsKey(attribute.Type.Name))
            {
                AllKnownTypes.TryAdd(attribute.Type.Name, attribute.Type);
                AllKnownTypeFactories.TryAdd(attribute.Type, Expression.Lambda<Func<object>>(Expression.New(attribute.Type)).Compile());
            }
        }
        if (knownTypeAttributes.Length > 0)
        {
            AllKnownTypes.TryAdd(type.Name, type);
            AllKnownTypeFactories.TryAdd(type, Expression.Lambda<Func<object>>(Expression.New(type)).Compile());
        }

        // Cache properties for the type
        var properties = type.GetProperties();
        if (properties == null) { return; }
        _typePropertiesCache.TryAdd(type, properties);

        foreach (var property in properties)
        {
            CacheKnownTypesAndFactories(property.PropertyType);
            //// Check if property has KnownTypeAttribute and cache
            //foreach (var attribute in property.PropertyType.GetCustomAttributes<KnownTypeAttribute>(false))
            //{
            //    if (attribute.Type != null && !AllKnownTypes.ContainsKey(attribute.Type.Name))
            //    {
            //        AllKnownTypes.TryAdd(attribute.Type.Name, attribute.Type);
            //        AllKnownTypeFactories.TryAdd(type, Expression.Lambda<Func<object>>(Expression.New(type)).Compile());
            //    }
            //}

            //// If the property is a collection, handle the element type
            //if (typeof(System.Collections.IEnumerable).IsAssignableFrom(property.PropertyType) &&
            //    property.PropertyType.IsGenericType)
            //{
            //    var elementType = property.PropertyType.GetGenericArguments()[0];
            //    CacheKnownTypesAndFactories(elementType);
            //}
            //// If the property is an array, handle the element type
            //else if (property.PropertyType.IsArray)
            //{
            //    var elementType = property.PropertyType.GetElementType();
            //    CacheKnownTypesAndFactories(elementType);
            //}
            //// For complex property types, recurse into them
            //else if (!property.PropertyType.IsPrimitive && property.PropertyType != typeof(string))
            //{
            //    CacheKnownTypesAndFactories(property.PropertyType);
            //}
        }
    }

    public override bool CanConvert(Type objectType) => _baseType.IsAssignableFrom(objectType);

    //TODO: Not Tested
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }

        var type = value.GetType();
        writer.WriteStartObject();
        writer.WritePropertyName("$type");
        writer.WriteValue(type.Name);

        if (!_typePropertiesCache.TryGetValue(type, out var properties))
        {
            properties = type.GetProperties();
            _typePropertiesCache[type] = properties;
        }

        foreach (var property in properties)
        {
            writer.WritePropertyName(property.Name);
            serializer.Serialize(writer, property.GetValue(value));
        }
        writer.WriteEndObject();
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            return null;
        }

        return objectType.IsArray
            ? ReadJsonArray(reader, objectType.GetElementType(), serializer)
            : ReadJsonObject(reader, objectType, existingValue, serializer);
    }

    private object ReadJsonArray(JsonReader reader, Type elementType, JsonSerializer serializer)
    {
        var jarray = JArray.Load(reader);
        var array = Array.CreateInstance(elementType, jarray.Count);
        var index = 0;

        foreach (var item in jarray)
        {
            var typeAnnotation = item["@odata.type"]?.ToString().TrimStart('#').Replace("Microsoft.Dynamics.CRM.Complex", "");
            if (string.IsNullOrEmpty(typeAnnotation))
            {
                array.SetValue(item.ToObject(elementType), index++);
            }
            else if (AllKnownTypes.TryGetValue(typeAnnotation, out var knownType))
            {
                var obj = AllKnownTypeFactories[knownType]();
                serializer.Populate(item.CreateReader(), obj);
                array.SetValue(obj, index++);
            }
            else
            {
                CacheKnownTypesAndFactories(elementType);
                throw new InvalidOperationException("We should not be here!");
            }
        }

        return array;
    }

    private object ReadJsonArrayOld(JsonReader reader, Type objectType, JsonSerializer serializer)
    {
        var knownTypes = objectType.GetCustomAttributes<KnownTypeAttribute>(false).ToList();
        var jarray = JArray.Load(reader);

        var array = Array.CreateInstance(objectType, jarray.Count);
        var index = 0;
        foreach (var item in jarray)
        {
            var typeAnnotation = item["@odata.type"]?.ToString().TrimStart('#');

            if (string.IsNullOrEmpty(typeAnnotation))
            {
                array.SetValue(item.ToObject(objectType), index++);
            }
            else
            {
                //var type = knownTypes.FirstOrDefault(attr => typeAnnotation.EndsWith(attr.Type.Name));
                var type = FirstKnownTypeAttribute(knownTypes, typeAnnotation);
                var obj = Activator.CreateInstance(type.Type);
                serializer.Populate(item.CreateReader(), obj);
                array.SetValue(obj, index++);
            }
        }

        return array;
    }

    private object ReadJsonObject(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var knownTypes = objectType.GetCustomAttributes<KnownTypeAttribute>(false).ToArray();
        var jsonObject = JObject.Load(reader);

        if (knownTypes.Length == 0)
        {
            return existingValue ?? jsonObject.ToObject(objectType);
        }

        var typeAnnotation = jsonObject["@odata.type"]?.ToString().TrimStart('#');
        if (string.IsNullOrEmpty(typeAnnotation))
        {
            return existingValue ?? jsonObject.ToObject(objectType);
        }

        //var type = knownTypes.FirstOrDefault(attr => typeAnnotation.EndsWith(attr.Type.Name));
        var type = FirstKnownTypeAttribute(knownTypes, typeAnnotation);
        var obj = AllKnownTypeFactories.TryGetValue(type.Type, out var activator) ? activator() : Activator.CreateInstance(type.Type);
        serializer.Populate(jsonObject.CreateReader(), obj);

        return obj;
    }

    private static KnownTypeAttribute FirstKnownTypeAttribute(IEnumerable<KnownTypeAttribute> types, string typeAnnotation)
    {
        foreach (var attrib in types)
        {
            if (typeAnnotation.EndsWith(attrib.Type.Name))
            {
                return attrib;
            }
        }
        return null;
    }

    private bool IsPolymorphicType(Type type) => type.GetCustomAttributes<KnownTypeAttribute>(true).Any(attr => attr.Type != type);

}

public class SuffixEqualityComparer : IEqualityComparer<string>
{
    public bool Equals(string x, string y)
    {
        if (x == null || y == null)
        {
            return false;
        }

        return y.EndsWith(x, StringComparison.Ordinal);
    }

    public int GetHashCode(string obj)
    {
        if (obj == null)
        {
            throw new ArgumentNullException(nameof(obj));
        }

        // Return the hash code of the last character of the string to align with the suffix comparison.
        return obj.Length > 0 ? obj[^1].GetHashCode() : 0;
    }
}