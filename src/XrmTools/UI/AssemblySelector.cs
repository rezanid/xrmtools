#nullable enable
namespace XrmTools.UI;

using System.ComponentModel.Composition;
using System.Threading.Tasks;
using XrmTools.Core.Repositories;
using XrmTools.Logging.Compatibility;
using XrmTools.Meta.Model.Configuration;
using XrmTools.Xrm.Repositories;

internal interface IAssemblySelector
{
    Task<(PluginAssemblyConfig? config, string? filename)> ChooseAssemblyAsync();
}

[Export(typeof(IAssemblySelector))]
[method: ImportingConstructor]
internal class AssemblySelector(
    [Import] IRepositoryFactory repositoryFactory,
    [Import] ILogger<AssemblySelector> logger) : IAssemblySelector
{
    public async Task<(PluginAssemblyConfig? config, string? filename)> ChooseAssemblyAsync()
    {
        var assemblyRepository = await repositoryFactory.CreateRepositoryAsync<IPluginAssemblyRepository>();
        var typeRepository = await repositoryFactory.CreateRepositoryAsync<IPluginTypeRepository>();
        var dialog = new AssemblySelectionDialog(assemblyRepository, typeRepository);
        if (dialog.ShowDialog() == true)
        {
            var viewmodel = (AssemblySelectionViewModel)dialog.DataContext;
            return (viewmodel.SelectedAssembly, viewmodel.FileName);
        }
        return (null, null);
    }
}
#nullable restore