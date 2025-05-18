namespace XrmTools.Helpers;

using Community.VisualStudio.Toolkit;
using System.Threading.Tasks;

internal static class PhysicalFileHelper
{
    public static async Task<bool> IsXrmPluginFileAsync(this PhysicalFile file)
    {
        var generator = await file.GetAttributeAsync("Generator").ConfigureAwait(false);
        if (generator != PluginCodeGenerator.Name)
        {
            return false;
        }

        var pluginAttr = await file.GetAttributeAsync("IsXrmPlugin").ConfigureAwait(false);
        return bool.TryParse(pluginAttr, out var isXrmPlugin) && isXrmPlugin;
    }

    public static async Task<bool> IsXrmEntityFileAsync(this PhysicalFile file)
    {
        var generator = await file.GetAttributeAsync("Generator").ConfigureAwait(false);
        if (generator != EntityCodeGenerator.Name)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Checks if the file is a Xrm file. This includes both XrmPlugin and XrmEntity files.
    /// </summary>
    public static async Task<bool> IsXrmFileAsync(this PhysicalFile file)
    {
        var generator = await file.GetAttributeAsync("Generator").ConfigureAwait(false);
        return generator is PluginCodeGenerator.Name or EntityCodeGenerator.Name;
    }
}
