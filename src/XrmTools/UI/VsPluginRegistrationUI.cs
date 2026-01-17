#nullable enable
namespace XrmTools.UI;

using Community.VisualStudio.Toolkit;
using System.Threading.Tasks;

internal sealed class VsPluginRegistrationUI : IPluginRegistrationUI
{
    public async Task<bool> ConfirmDeleteRemovedPluginsAsync(System.Collections.Generic.IEnumerable<string> removedTypeNames)
    {
        var removedPluginNames = string.Join(", ", removedTypeNames);
        return await VS.MessageBox.ShowConfirmAsync(Vsix.Name, "Looks like you have removed the following plugins. Continuing will remove these plugins from Dataverse too. Is that ok?\r\n" + removedPluginNames);
    }
    public async Task<bool> ConfirmUnregsiterAssemblyAsync(string assemblyName)
    {
        return await VS.MessageBox.ShowConfirmAsync(Vsix.Name, "Are you sure you want to unregister the plugin assembly '" + assemblyName + "' from Dataverse?");
    }
}
#nullable restore