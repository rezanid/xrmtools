#nullable enable
namespace XrmTools.OData;

using System;
using System.Collections.Generic;

internal static class ODataBuiltInVariables
{
    public static IReadOnlyDictionary<string, (Func<string> Evaluate, string Description)> All { get; } =
        new Dictionary<string, (Func<string>, string)>(StringComparer.Ordinal)
        {
            ["$guid"] = (() => Guid.NewGuid().ToString("D"), "Generates a fresh GUID for each use when the request is sent.")
        };
}
