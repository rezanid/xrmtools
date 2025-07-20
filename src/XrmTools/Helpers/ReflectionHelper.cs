#nullable enable
namespace XrmTools.Helpers;

using Microsoft.CodeAnalysis;
using System;
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
        if (attributeData.AttributeConstructor == null || attributeData.AttributeConstructor.Parameters.Length == 0)
            return instance;

        void SetProperty(string propertyName, TypedConstant value)
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
                    var enumValue =
                        Nullable.GetUnderlyingType(property.PropertyType) is null ?
                        Enum.ToObject(property.PropertyType, value.Value) :
                        Enum.ToObject(Nullable.GetUnderlyingType(property.PropertyType), value.Value);
                    property.SetValue(instance, enumValue);
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
                else
                {
                    property.SetValue(instance, Convert.ChangeType(value.Value, property.PropertyType));
                }
            }
        }

        for (int i = 0; i < attributeData.ConstructorArguments.Length; i++)
        {
            var argumentName = attributeData.AttributeConstructor.Parameters[i].Name;
            SetProperty(argumentName, attributeData.ConstructorArguments[i]);
        }

        foreach (var namedArgument in attributeData.NamedArguments)
        {
            SetProperty(namedArgument.Key, namedArgument.Value);
        }

        return instance;
    }
}
#nullable restore