using System.Runtime.InteropServices;
#nullable enable
namespace XrmGen.Xrm.Generators;

using XrmGen.Xrm.Model;

[Guid(PackageGuids.guidXrmPluginCodeGeneratorString)]
[ComVisible(true)]
public interface IXrmPluginCodeGenerator
{
    public XrmCodeGenConfig? Config { get; set; }
    (bool, string) IsValid(PluginAssemblyInfo plugin);
    string GenerateCode(PluginAssemblyInfo plugin);
}
#nullable restore