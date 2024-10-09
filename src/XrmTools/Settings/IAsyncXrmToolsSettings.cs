#nullable enable
namespace XrmTools.Settings;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IAsyncXrmToolsSettings
{
    IEnumerable<string> Keys { get; }
    bool IsDirty { get; }
    Task<string?> EnvironmentUrlAsync();
    Task<string?> ConnectionStringAsync();
    Task<string?> PluginTemplateFilePathAsync();
    Task<string?> EntityTemplateFilePathAsync();
    Task<bool> EnvironmentUrlAsync(string value);
    Task<bool> ConnectionStringAsync(string value);
    Task<bool> PluginTemplateFilePathAsync(string value);
    Task<bool> EntityTemplateFilePathAsync(string value);
}
#nullable restore