#nullable enable
namespace XrmTools.Helpers;

using Microsoft.CodeAnalysis;
using System;
using System.Linq;

public static class AttributeDataHelper
{
    public static T? GetValue<T>(this AttributeData attributeData, string argumentName)
    {
        if (attributeData == null) throw new ArgumentNullException(nameof(attributeData));

        var namedArgument = attributeData.NamedArguments.FirstOrDefault(arg => arg.Key == argumentName);
        if (namedArgument.Value.Value != null)
        {
            return (T?)namedArgument.Value.Value;
        }

        return default;
    }
}