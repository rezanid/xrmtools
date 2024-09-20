#nullable enable
namespace XrmTools.UI;

using Community.VisualStudio.Toolkit;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using XrmTools.Xrm;
using XrmTools.Xrm.Model;

internal interface IAssemblySelector
{
    Task<(PluginAssemblyConfig config, string filename)> ChooseAssemblyAsync();
}

internal class AssemblySelector(IEnvironmentProvider environmentProvider, IXrmSchemaProviderFactory schemaProviderFactory, ILogger<AssemblySelector> logger) : IAssemblySelector
{
    public async Task<(PluginAssemblyConfig? config, string? filename)> ChooseAssemblyAsync()
    {
        var environment = await environmentProvider.GetActiveEnvironmentAsync();
        if (environment == null || !environment.IsValid()) { return (null, null); }
        var schemaProvider = schemaProviderFactory?.GetOrNew(environment);
        if (schemaProvider == null)
        {
            logger.LogWarning($"{environment} is not a valid environment.");
            return (null, null);
        }
        if (!schemaProvider.IsReady)
        {
            logger.LogWarning($"Connection has failed to the environment: {environment.Url}");
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

    private static async Task<string?> GetProjectPropertyAsync(string propertyName)
    {
        var item = await VS.Solutions.GetActiveProjectAsync();
        return item == null ? null : await item.GetAttributeAsync(propertyName);
    }
}
#nullable restore