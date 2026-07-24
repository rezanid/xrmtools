#nullable enable
namespace XrmTools.Helpers;

using Community.VisualStudio.Toolkit;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Cheap, non-blocking plugin detection used for command visibility.
/// It never forces a Roslyn compilation. Detection order:
/// 1) The explicit opt-in marker (in-memory MSBuild item metadata).
/// 2) A syntax-only scan for a [Plugin] attribute, preferring the already-parsed
///    syntax tree and only parsing this single file as a last resort.
/// The authoritative semantic (IPlugin) detection remains at execution time.
/// </summary>
internal static class PluginDetectionHelper
{
    private const string PluginAttributeName = "Plugin";
    private const string PluginAttributeFullName = "PluginAttribute";

    public static async Task<bool> LooksLikePluginFileAsync(this PhysicalFile file)
    {
        // Tier 0: honor the explicit opt-in first (cheapest, and authoritative for opted-in files).
        if (await file.IsXrmPluginFileAsync().ConfigureAwait(false))
        {
            return true;
        }

        var document = await FileHelper.GetDocumentAsync(file.FullPath).ConfigureAwait(false);
        if (document is null)
        {
            return false;
        }

        // Tier 1: reuse the already-parsed syntax tree if available (zero extra work).
        if (document.TryGetSyntaxTree(out var tree))
        {
            return HasPluginAttribute(tree.GetRoot());
        }

        // Tier 2: parse this single file only. Still far cheaper than a compilation.
        var root = await document.GetSyntaxRootAsync().ConfigureAwait(false);
        return root is not null && HasPluginAttribute(root);
    }

    internal static bool HasPluginAttribute(SyntaxNode root) =>
        root.DescendantNodes()
            .OfType<AttributeSyntax>()
            .Any(a => MatchesPluginName(a.Name.ToString()));

    internal static bool MatchesPluginName(string name)
    {
        // Handles "Plugin", "PluginAttribute", and namespace-qualified forms.
        var simple = name.Contains('.') ? name[(name.LastIndexOf('.') + 1)..] : name;
        return simple.Equals(PluginAttributeName, StringComparison.Ordinal)
            || simple.Equals(PluginAttributeFullName, StringComparison.Ordinal);
    }
}
