#nullable enable
namespace XrmTools.UI;

using Community.VisualStudio.Toolkit;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
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
        if (environment == null || !environment.IsValid) { return (null, null); }
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