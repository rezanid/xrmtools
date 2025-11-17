#nullable enable
namespace XrmTools.Settings;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using System.Collections.Generic;
using System.Threading.Tasks;

public class ProjectSettings(ProjectStorageType storageType) : IAsyncXrmToolsSettings
{
    public IEnumerable<string> Keys => XrmSettingKeys.All;
    public bool IsDirty { get; set; }

    public async Task<string?> EnvironmentUrlAsync() => await GetSettingAsync(XrmSettingKeys.EnvironmentUrl);
    public async Task<string?> ConnectionStringAsync() => await GetSettingAsync(XrmSettingKeys.DataverseConnectionString);
    public async Task<string?> PluginTemplateFilePathAsync() => await GetSettingAsync(XrmSettingKeys.DataversePluginTemplateFilePath);
    public async Task<string?> EntityTemplateFilePathAsync() => await GetSettingAsync(XrmSettingKeys.DataverseEntityTemplateFilePath);
    public async Task<string?> GlobalOptionSetsTemplateFilePathAsync() => await GetSettingAsync(XrmSettingKeys.DataverseGlobalOptionSetsTemplateFilePath);
    public async Task<string?> GlobalOptionSetsFilePathAsync() => await GetSettingAsync(XrmSettingKeys.DataverseGlobalOptionSetsFilePath);
    public async Task<string?> FetchXmlTemplateFilePathAsync() => await GetSettingAsync(XrmSettingKeys.DataverseFetchXmlTemplateFilePath);
    public async Task<bool> EnvironmentUrlAsync(string? value) => await SetSettingAsync(XrmSettingKeys.EnvironmentUrl, value);
    public async Task<bool> ConnectionStringAsync(string? value) => await SetSettingAsync(XrmSettingKeys.DataverseConnectionString, value);
    public async Task<bool> PluginTemplateFilePathAsync(string? value) => await SetSettingAsync(XrmSettingKeys.DataversePluginTemplateFilePath, value);
    public async Task<bool> EntityTemplateFilePathAsync(string? value) => await SetSettingAsync(XrmSettingKeys.DataverseEntityTemplateFilePath, value);

    private async Task<string?> GetSettingAsync(string name)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var proj = await VS.Solutions.GetActiveProjectAsync();
        if (proj == null) return null;
        return await proj.GetAttributeAsync(name);
    }

    private async Task<bool> SetSettingAsync(string name, string? value)
    {
        var proj = await VS.Solutions.GetActiveProjectAsync();
        if (proj == null) return false;
        if (value is null)
        {
            return await proj.RemoveAttributeAsync(name);
        }
        else
        {
            return await proj.TrySetAttributeAsync(name, value, storageType);
        }
    }
}
#nullable restore