using System;
using System.Collections.Generic;
using System.Linq;
using Humanizer;
using Scriban.Runtime;
using XrmGen.Helpers;

namespace XrmGen.Xrm.Generators;

/// <summary>
/// Contains a cache of extension methods that can be used in Scriban templates. You can easily expose 
/// other extension methods by adding a new enum value and a corresponding method to fetch the types.
/// </summary>
public static class ScribanExtensionCache
{
    public enum KnownAssemblies
    {
        Humanizr,
    }

    private static readonly Dictionary<KnownAssemblies, ScriptObject> CachedResults = [];

    public static ScriptObject GetHumanizrMethods() => GetOrCreate(
        KnownAssemblies.Humanizr,
        () =>
        {
            //force a load of the DLL otherwise we won't see the types
            "force load".Humanize();
            return AppDomain.CurrentDomain
                .GetAssemblies()
                .Single(a => a.FullName.EmptyWhenNull().Contains("Humanizer"))
                .GetTypes()
                .Where(t => t.Name.EndsWith("Extensions", StringComparison.OrdinalIgnoreCase))
                .ToArray();
        });

    private static ScriptObject GetOrCreate(KnownAssemblies name, Func<IEnumerable<Type>> typeFetcher)
    {
        if (CachedResults.TryGetValue(name, out var scriptObject))
        {
            return scriptObject;
        }

        scriptObject = [];
        foreach (var extensionClass in typeFetcher())
        {
            scriptObject.Import(extensionClass);
        }

        CachedResults[name] = scriptObject;

        return scriptObject;
    }
}

