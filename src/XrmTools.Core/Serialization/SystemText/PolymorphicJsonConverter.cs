namespace XrmTools.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

public class PolymorphicJsonConverter<TBase> : JsonConverter<TBase>
{
    private static readonly PolymorphicTypeResolver<TBase> Resolver = PolymorphicTypeResolver<TBase>.Instance;

    // Cache for stripped options
    private static readonly ConditionalWeakTable<JsonSerializerOptions, JsonSerializerOptions> _strippedOptionsCache = new();

    public override bool CanConvert(Type typeToConvert) => typeof(TBase).IsAssignableFrom(typeToConvert);

    public override TBase Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        var derivedType = Resolver.ResolveType(root) ?? typeToConvert;// throw new JsonException($"Unable to resolve derived type for {typeof(TBase).Name}.");
        var json = root.GetRawText();

        var newOptions = GetStrippedOptions(options);

        return (TBase)JsonSerializer.Deserialize(json, derivedType, newOptions);
    }

    public override void Write(Utf8JsonWriter writer, TBase value, JsonSerializerOptions options)
    {
        var newOptions = GetStrippedOptions(options);

        JsonSerializer.Serialize(writer, value, value.GetType(), newOptions);
    }

    private JsonSerializerOptions GetStrippedOptions(JsonSerializerOptions options)
    {
        return _strippedOptionsCache.GetValue(options, opts =>
        {
            var clone = new JsonSerializerOptions(opts);
            var thisConverter = clone.Converters.FirstOrDefault(c => c.GetType() == this.GetType());
            if (thisConverter != null)
                clone.Converters.Remove(thisConverter);
            return clone;
        });
    }
}

public class PolymorphicTypeResolver<TBase>
{
    public static readonly PolymorphicTypeResolver<TBase> Instance = new();

    private readonly Type _baseType;
    private readonly List<Type> _derivedTypes;
    private readonly Dictionary<Type, HashSet<string>> _typeProperties;
    private readonly Dictionary<string, Type> _uniquePropertyToType;
    private readonly object _lock = new();

    private PolymorphicTypeResolver()
    {
        _baseType = typeof(TBase);
        _derivedTypes = GetDerivedTypes(_baseType).ToList();
        _typeProperties = [];
        _uniquePropertyToType = [];

        BuildPropertyMaps();
    }

    private static IEnumerable<Type> GetDerivedTypes(Type baseType)
    {
        var attrs = baseType.GetCustomAttributes(typeof(KnownTypeAttribute), false)
            .Cast<KnownTypeAttribute>();
        foreach (var attr in attrs)
            yield return attr.Type;
    }

    private void BuildPropertyMaps()
    {
        // Get all public instance property names for each type
        var allTypes = new[] { _baseType }.Concat(_derivedTypes).ToList();
        foreach (var type in allTypes)
        {
            var props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Select(p => p.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? p.Name)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            _typeProperties[type] = props;
        }

        // For each derived type, find unique properties
        foreach (var derived in _derivedTypes)
        {
            var uniqueProps = _typeProperties[derived]
                .Except(_typeProperties[_baseType])
                .Except(_derivedTypes.Where(t => t != derived).SelectMany(t => _typeProperties[t]))
                .ToList();

            foreach (var prop in uniqueProps)
                _uniquePropertyToType[prop] = derived;
        }
    }

    public Type? ResolveType(JsonElement json)
    {
        foreach (var prop in json.EnumerateObject())
        {
            if (_uniquePropertyToType.TryGetValue(prop.Name, out var type))
                return type;
        }
        return null;
    }
}