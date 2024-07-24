using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using XrmGen.Xrm.Model;

namespace XrmGen.Xrm.Generators;

[Export(typeof(IPluginGenerator))]
internal class TemplatedPluginGenerator : IPluginGenerator
{
    public CodeGenConfig Config { set; private get; }

    public (bool, string) IsValid(PluginAssembly pluginAssembly)
    {
        if (pluginAssembly is null) { throw new ArgumentNullException(nameof(pluginAssembly)); }
        if (pluginAssembly.PluginTypes is null || !pluginAssembly.PluginTypes.Any()) { return (false, Resources.Strings.PluginGenerator_NoPluginTypes); }
        return (true, string.Empty);
    }

    public void GenerateCode(StringBuilder builder, PluginAssembly plugin, string suggestedNamespace)
    {
        return;
    }
}
