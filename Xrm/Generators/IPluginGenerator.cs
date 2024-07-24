using System.Text;
using XrmGen.Xrm.Model;

namespace XrmGen.Xrm.Generators;

public interface IPluginGenerator
{
    public CodeGenConfig Config { set; }
    (bool, string) IsValid(PluginAssembly plugin);
    void GenerateCode(StringBuilder builder, PluginAssembly plugin, string suggestedNamespace);
}