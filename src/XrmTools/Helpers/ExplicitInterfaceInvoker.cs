#nullable enable
namespace XrmTools.Helpers;

using System;
using System.Collections.Generic;
using System.Reflection;

public class ExplicitInterfaceInvoker<T>
{
    private readonly Dictionary<string, MethodInfo> cache = [];
    private readonly Type baseType = typeof(T);

    private MethodInfo FindMethod(string methodName)
    {
        if (!cache.TryGetValue(methodName, out var method))
        {
            var methods = baseType.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            foreach (var methodInfo in methods)
            {
                if (methodInfo.IsFinal && methodInfo.IsPrivate) //explicit interface implementation
                {
                    if (methodInfo.Name == methodName || methodInfo.Name.EndsWith("." + methodName))
                    {
                        method = methodInfo;
                        break;
                    }
                }
            }

            cache.Add(methodName, method);
        }

        return method;
    }

    public RT Invoke<RT>(T obj, string methodName, params object[] parameters)
    {
        if (obj == null) throw new ArgumentNullException(nameof(obj));
        if (!baseType.IsAssignableFrom(obj.GetType()))
            throw new ArgumentException("Object is not of type " + baseType.Name);
        var method = FindMethod(methodName) ?? throw new InvalidOperationException($"Method '{methodName}' not found.");
        return (RT)method.Invoke(obj, parameters);
    }
}
#nullable restore