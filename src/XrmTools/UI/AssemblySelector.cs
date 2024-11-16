#nullable enable
namespace XrmTools.UI;

using Community.VisualStudio.Toolkit;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using XrmTools.Environments;
using XrmTools.Logging.Compatibility;
using XrmTools.Options;
using XrmTools.Resources;
using XrmTools.Xrm;
using XrmTools.Xrm.Model;

public interface IAssemblySelector
{
    Task<(PluginAssemblyConfig? config, string? filename)> ChooseAssemblyAsync();
}

[Export(typeof(IAssemblySelector))]
[method: ImportingConstructor]
public class AssemblySelector(
    [Import] IEnvironmentProvider environmentProvider, 
    [Import] IXrmSchemaProviderFactory schemaProviderFactory, 
    [Import] ILogger<AssemblySelector> logger) : IAssemblySelector
{
    public async Task<(PluginAssemblyConfig? config, string? filename)> ChooseAssemblyAsync()
    {
        var environment = await environmentProvider.GetActiveEnvironmentAsync();
        if (environment == null || !environment.IsValid) 
        {
            var options = await GeneralOptions.GetLiveInstanceAsync();
            await VS.MessageBox.ShowAsync(Strings.EnvironmentErrorNoneSelected, options.CurrentEnvironmentStorage switch
            {
                SettingsStorageTypes.Options => Strings.EnvironmentErrorNoneSelectedOptions,
                SettingsStorageTypes.Solution or SettingsStorageTypes.SolutionUser => Strings.EnvironmentErrorNoneSelectedSolution,
                SettingsStorageTypes.Project or SettingsStorageTypes.ProjectUser => Strings.EnvironmentErrorNoneSelectedProject,
                _ => throw new System.NotImplementedException()
            }, Microsoft.VisualStudio.Shell.Interop.OLEMSGICON.OLEMSGICON_WARNING);
            return (null, null); 
        }
        var schemaProvider = schemaProviderFactory?.GetOrNew(environment);
        if (schemaProvider == null)
        {
            logger.LogWarning(string.Format(Strings.EnvironmentInvalid, environment));
            return (null, null);
        }
        if (!schemaProvider.IsReady)
        {
            logger.LogWarning(string.Format(Strings.EnvironmentConnectionFailed, environment));
            logger.LogWarning(string.IsNullOrEmpty(schemaProvider.LastError) ? "No error detected in Dataverse provider." : "Last Error: " + schemaProvider.LastError);
            await VS.MessageBox.ShowErrorAsync(Strings.EnvironmentConnectionErrorUITitle, string.Format(Strings.EnvironmentConnectionErrorUIDesc, environment));
            return (null, null);
        }
        var dialog = new AssemblySelectionDialog(schemaProvider);
        if (dialog.ShowDialog() == true)
        {
            var viewmodel = (AssemblySelectionViewModel)dialog.DataContext;
            return (viewmodel.SelectedAssembly, viewmodel.FileName);
        }
        return (null, null);
    }
}
#nullable restore