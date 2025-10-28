#nullable enable
namespace XrmTools.UI;

using System.ComponentModel.Composition;
using XrmTools.Logging.Compatibility;
using XrmTools.Meta.Model.Configuration;
using XrmTools.Xrm.Repositories;

internal interface IAssemblySelector
{
    (PluginAssemblyConfig? config, string? filename) ChooseAssembly();
}

[Export(typeof(IAssemblySelector))]
[method: ImportingConstructor]
internal class AssemblySelector(
    [Import] IRepositoryFactory repositoryFactory,
    [Import] ILogger<AssemblySelector> logger) : IAssemblySelector
{
    public (PluginAssemblyConfig? config, string? filename) ChooseAssembly()
    {
        var dialog = new AssemblySelectionDialog(repositoryFactory);
        if (dialog.ShowDialog() == true)
        {
            var viewmodel = (AssemblySelectionViewModel)dialog.DataContext;
            return (viewmodel.SelectedAssembly, viewmodel.FileName);
        }
        return (null, null);
    }
}
#nullable restore