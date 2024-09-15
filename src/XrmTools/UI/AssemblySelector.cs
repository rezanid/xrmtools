#nullable enable
namespace XrmGen.UI;

using Community.VisualStudio.Toolkit;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using XrmGen.Xrm;
using XrmGen.Xrm.Model;

internal static class AssemblySelector
{
    public static async Task<(PluginAssemblyConfig? config, string? filename)> ChooseAssemblyAsync(
        IXrmSchemaProviderFactory? schemaProviderFactory, ILogger logger)
    {
        var url = await GetProjectPropertyAsync("EnvironmentUrl");
        if (string.IsNullOrWhiteSpace(url)) { return (null, null); }
        var schemaProvider = schemaProviderFactory?.GetOrNew(url!);
        if (schemaProvider == null)
        {
            logger.LogWarning(url + " used in your EnvironmentUrl build property is not a valid environment URL.");
            return (null, null);
        }
        if (!schemaProvider.IsReady)
        {
            logger.LogWarning($"Connection has failed to the environment: {url}");
            logger.LogWarning(string.IsNullOrEmpty(schemaProvider.LastError) ? "No error detected in Dataverse provider." : "Last Error: " + schemaProvider.LastError);
            await VS.MessageBox.ShowErrorAsync("Dataverse Connection", $"Connection has failed to the environment: {url} check the Output window for more information.");
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