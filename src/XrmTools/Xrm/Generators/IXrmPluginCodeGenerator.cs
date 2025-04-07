using System.Runtime.InteropServices;
#nullable enable
namespace XrmTools.Xrm.Generators;

using System.ComponentModel.DataAnnotations;
using XrmTools.Xrm.Model;

[Guid(PackageGuids.guidXrmPluginCodeGeneratorString)]
[ComVisible(true)]
public interface IXrmPluginCodeGenerator
{
    public XrmCodeGenConfig? Config { get; set; }
    ValidationResult IsValid(PluginAssemblyConfig plugin);
    string GenerateCode(PluginAssemblyConfig plugin);
}
#nullable restore