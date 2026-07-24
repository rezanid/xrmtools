#nullable enable
namespace XrmTools.UI;

using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using System.Collections.Generic;
using System.Threading.Tasks;
using XrmTools.Options;

internal sealed class VsPluginRegistrationUI : IPluginRegistrationUI
{
    public async Task<RemovedPluginsDecision> ConfirmRemovedPluginsAsync(IReadOnlyList<RemovedPluginSummary> removedPlugins)
    {
        if (removedPlugins is null || removedPlugins.Count == 0)
        {
            return RemovedPluginsDecision.Delete;
        }

        var options = await GeneralOptions.GetLiveInstanceAsync();

        // When prompting is disabled, proceed with the authoritative default (delete) so that code
        // remains the single source of truth.
        if (!options.PromptBeforeDeletingRemovedPluginsAtProjectLevel)
        {
            return RemovedPluginsDecision.Delete;
        }

        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dialog = new RemovedPluginsDialog(removedPlugins);
        dialog.ShowModal();
        return dialog.Decision;
    }

    public async Task<bool> ConfirmUnregsiterAssemblyAsync(string assemblyName)
    {
        return await VS.MessageBox.ShowConfirmAsync(Vsix.Name, "Are you sure you want to unregister the plugin assembly '" + assemblyName + "' from Dataverse?");
    }
}
#nullable restore