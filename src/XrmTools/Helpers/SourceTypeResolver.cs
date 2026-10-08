#nullable enable
namespace XrmTools.Helpers;

using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed record SourceTypeMatch(INamedTypeSymbol Symbol, Compilation Compilation);

/// <summary>Resolves only types owned by solution projects, excluding metadata references.</summary>
internal static class SourceTypeResolver
{
    internal static async Task<IReadOnlyList<SourceTypeMatch>> FindAsync(Solution solution, string typeName, string? assemblyName, CancellationToken token)
    {
        var matches = new List<SourceTypeMatch>();
        foreach (var project in solution.Projects.Where(p => assemblyName == null || string.Equals(p.AssemblyName, assemblyName, StringComparison.OrdinalIgnoreCase)))
        {
            token.ThrowIfCancellationRequested();
            var compilation = await project.GetCompilationAsync(token).ConfigureAwait(false);
            var symbol = compilation?.Assembly.GetTypeByMetadataName(typeName);
            if (symbol != null && symbol.Locations.Any(l => l.IsInSource)) matches.Add(new SourceTypeMatch(symbol, compilation!));
        }
        return matches;
    }
}
