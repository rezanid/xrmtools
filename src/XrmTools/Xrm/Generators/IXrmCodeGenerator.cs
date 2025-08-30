#nullable enable
namespace XrmTools.Xrm.Generators;
using System.Runtime.InteropServices;
using System.ComponentModel.DataAnnotations;
using XrmTools.Meta.Model.Configuration;

[Guid(PackageGuids.guidXrmPluginCodeGeneratorString)]
[ComVisible(true)]
internal interface IXrmCodeGenerator
{
    public XrmCodeGenConfig? Config { get; set; }
    ValidationResult IsValid(PluginAssemblyConfig inputModel);
    string GenerateCode(PluginAssemblyConfig inputModel);
}
#nullable restore