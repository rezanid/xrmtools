#nullable enable
namespace XrmTools.Settings;
using System.Collections.Generic;

public interface IXrmToolsSettings
{
    IEnumerable<string> Keys { get; }
    bool IsDirty { get; }
    string? EnvironmentUrl();
    string? ConnectionString();
    string? PluginTemplateFilePath();
    string? EntityTemplateFilePath();
    bool EnvironmentUrl(string value);
    bool ConnectionString(string value);
    bool PluginTemplateFilePath(string value);
    bool EntityTemplateFilePath(string value);
    string? Get(string name);
    bool Set(string name, string? value);
}
#nullable restore