#nullable enable
using System.Runtime.InteropServices;
namespace XrmTools.Xrm.Generators;

using System.ComponentModel.DataAnnotations;
using XrmTools.Xrm.Model;

[Guid(PackageGuids.guidXrmPluginCodeGeneratorString)]
[ComVisible(true)]
internal interface IXrmCodeGenerator
{
    public XrmCodeGenConfig? Config { get; set; }
    ValidationResult IsValid(PluginAssemblyConfig plugin);
    string GenerateCode(PluginAssemblyConfig plugin);
}
#nullable restore