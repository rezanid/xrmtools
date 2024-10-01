#nullable enable
namespace XrmTools.UI;

using Community.VisualStudio.Toolkit;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using XrmTools.Options;
using XrmTools.Resources;
using XrmTools.Xrm;
using XrmTools.Xrm.Model;

internal interface IAssemblySelector
{
    Task<(PluginAssemblyConfig? config, string? filename)> ChooseAssemblyAsync();
}

internal class AssemblySelector(IEnvironmentProvider environmentProvider, IXrmSchemaProviderFactory schemaProviderFactory, ILogger<AssemblySelector> logger) : IAssemblySelector
{
    public async Task<(PluginAssemblyConfig? config, string? filename)> ChooseAssemblyAsync()
    {
        var environment = await environmentProvider.GetActiveEnvironmentAsync();
        if (environment == null || !environment.IsValid) 
        {
            var options = await GeneralOptions.GetLiveInstanceAsync();
            await VS.MessageBox.ShowAsync("No environment selected", options.CurrentEnvironmentStorage switch
            {
                CurrentEnvironmentStorageType.Options => "No environment selected. Please select an environment from Tools > Options > Xrm Tools > Current Environment.",
                CurrentEnvironmentStorageType.Solution or CurrentEnvironmentStorageType.SolutionUser => "No environment selected. Please select an environment from Solution context menu > Set Environment.",
                CurrentEnvironmentStorageType.Project or CurrentEnvironmentStorageType.ProjectUser => "No environment selected. Please select an environment from Project context menu > Set Environment."
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
            await VS.MessageBox.ShowErrorAsync("Dataverse Connection", $"Connection has failed to the environment: {environment.Url} check the Output window for more information.");
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