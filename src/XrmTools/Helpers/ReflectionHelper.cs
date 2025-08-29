#nullable enable
namespace XrmTools.Helpers;

using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Reflection;

public static class ReflectionHelper
{
    /// <summary>
    /// Sets the properties of the specified instance based on the provided attribute data.
    /// </summary>
    /// <remarks>This method uses reflection to match attribute constructor arguments and named arguments to
    /// the properties of the instance. Only public, writable properties are set. The method handles conversion for
    /// array, enum, and Guid types, as well as other compatible types.</remarks>
    /// <typeparam name="T">The type of the instance, which must be a class with a parameterless constructor.</typeparam>
    /// <param name="instance">The instance whose properties are to be set.</param>
    /// <param name="attributeData">The attribute data containing the values to set on the instance properties.</param>
    /// <returns>The instance with its properties set according to the attribute data.</returns>
    public static T SetPropertiesFromAttribute<T>(this T instance, AttributeData attributeData) where T : class, new()
    {
        if (instance == null) throw new ArgumentNullException(nameof(instance));
        if (attributeData == null) throw new ArgumentNullException(nameof(attributeData));

        if (attributeData.AttributeConstructor != null)
        {
            for (int i = 0; i < attributeData.ConstructorArguments.Length; i++)
            {
                var argumentName = attributeData.AttributeConstructor.Parameters[i].Name;
                SetProperty(instance, argumentName, attributeData.ConstructorArguments[i]);
            }
        }

        foreach (var namedArgument in attributeData.NamedArguments)
        {
            SetProperty(instance, namedArgument.Key, namedArgument.Value);
        }

        return instance;
    }

    /// <summary>
    /// Sets the properties of the specified instance based on the provided attribute data. You can provide a name mapping to map
    /// attribute argument names to property names.
    /// </summary>
    /// <remarks>This method uses reflection to match attribute constructor arguments and named arguments to
    /// the properties of the instance. Only public, writable properties are set. The method handles conversion for
    /// array, enum, and Guid types, as well as other compatible types.</remarks>
    /// <typeparam name="T">The type of the instance, which must be a class with a parameterless constructor.</typeparam>
    /// <param name="instance">The instance whose properties are to be set.</param>
    /// <param name="attributeData">The attribute data containing the values to set on the instance properties.</param>
    /// <returns>The instance with its properties set according to the attribute data.</returns>
    public static T SetPropertiesFromAttribute<T>(this T instance, AttributeData? attributeData, Dictionary<string, string> nameMapping) where T : class, new()
    {
        if (instance == null) throw new ArgumentNullException(nameof(instance));
        if (attributeData == null) throw new ArgumentNullException(nameof(attributeData));

        if (attributeData.AttributeConstructor != null)
        {
            for (int i = 0; i < attributeData.ConstructorArguments.Length; i++)
            {
                var argumentName = attributeData.AttributeConstructor.Parameters[i].Name;
                var propertyName = nameMapping.TryGetValue(argumentName, out var mappedName) ? mappedName : argumentName;
                SetProperty(instance, propertyName, attributeData.ConstructorArguments[i]);
            }
        }

        foreach (var namedArgument in attributeData.NamedArguments)
        {
            var propertyName = nameMapping.TryGetValue(namedArgument.Key, out var mappedName) ? mappedName : namedArgument.Key;
            SetProperty(instance, namedArgument.Key, namedArgument.Value);
        }
        return instance;
    }

    private static void SetProperty<T>(T instance, string propertyName, TypedConstant value)
    {
        var property = typeof(T).GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);
        if (property != null && property.CanWrite && !value.IsNull)
        {
            if (value.Kind == TypedConstantKind.Array)
            {
                var elementType = property.PropertyType.GetElementType();
                if (elementType != null)
                {
                    var array = Array.CreateInstance(elementType, value.Values.Length);
                    for (int j = 0; j < value.Values.Length; j++)
                    {
                        var elementValue = Convert.ChangeType(value.Values[j].Value, elementType);
                        array.SetValue(elementValue, j);
                    }
                    property.SetValue(instance, array);
                }
            }
            else if (value.Kind == TypedConstantKind.Enum)
            {
                var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                if (targetType.IsEnum)
                {
                    var enumValue = Enum.ToObject(targetType, value.Value);
                    property.SetValue(instance, enumValue);
                }
            }
            else if ((property.PropertyType == typeof(Guid) || property.PropertyType == typeof(Guid?)))
            {
                if (Guid.TryParse(value.Value as string, out var guidValue))
                {
                    property.SetValue(instance, guidValue);
                }
            }
            else if (property.PropertyType == value.Value!.GetType())
            {
                property.SetValue(instance, value.Value);
            }
            else if (value.Kind == TypedConstantKind.Type)
            {
                property.SetValue(instance, (value.Value as INamedTypeSymbol)?.ToDisplayString());
            }
            else
            {
                try
                {
                    var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                    var convertedValue = Convert.ChangeType(value.Value, targetType);
                    property.SetValue(instance, convertedValue);
                }
                catch (InvalidCastException)
                {
                    // Skip invalid conversions
                }
            }
        }
    }
}
#nullable restore