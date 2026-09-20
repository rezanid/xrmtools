#nullable enable
namespace XrmTools.DataverseExplorer.Services;

using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.DataverseExplorer.Models;

/// <summary>Export a provider to add a root without changing the tree or view model.</summary>
internal interface IExplorerCategoryProvider
{
    int Order { get; }
    CategoryNode CreateRoot();
}

internal static class ExplorerCategory
{
    public static CategoryNode Create(string id, string name, ImageMoniker icon,
        System.Func<CancellationToken, Task<IEnumerable<ExplorerNodeBase>>> load)
    {
        var root = new CategoryNode { Id = id, DisplayName = name, ImageMoniker = icon };
        root.SetArtifactCategory(id);
        root.LoadChildrenAsync = async token =>
        {
            var children = await load(token);
            token.ThrowIfCancellationRequested();
            root.Children.Clear();
            foreach (var child in children)
            {
                child.Parent = root;
                root.Children.Add(child);
            }
        };
        return root;
    }
}

[Export(typeof(IExplorerCategoryProvider))]
[method: ImportingConstructor]
internal sealed class PluginPackagesCategoryProvider(IExplorerDataService data) : IExplorerCategoryProvider
{
    public int Order => 0;
    public CategoryNode CreateRoot() => ExplorerCategory.Create("Packages", "Plugin Packages", KnownMonikers.NuGet,
        async token => await data.LoadPackagesAsync(token));
}

[Export(typeof(IExplorerCategoryProvider))]
[method: ImportingConstructor]
internal sealed class PluginAssembliesCategoryProvider(IExplorerDataService data) : IExplorerCategoryProvider
{
    public int Order => 10;
    public CategoryNode CreateRoot() => ExplorerCategory.Create("Assemblies", "Plugin Assemblies", KnownMonikers.Assembly,
        async token => await data.LoadAssembliesAsync(token));
}

[Export(typeof(IExplorerCategoryProvider))]
[method: ImportingConstructor]
internal sealed class TablesCategoryProvider(IExplorerDataService data) : IExplorerCategoryProvider
{
    public int Order => 20;
    public CategoryNode CreateRoot() => ExplorerCategory.Create("Tables", "Tables", KnownMonikers.Table,
        async token => await data.LoadTablesAsync(token));
}
