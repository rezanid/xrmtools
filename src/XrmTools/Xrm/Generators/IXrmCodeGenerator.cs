#nullable enable
namespace XrmTools.Xrm.Generators;
using System.Runtime.InteropServices;
using System.ComponentModel.DataAnnotations;
using XrmTools.Meta.Model.Configuration;
using XrmTools.FetchXml.Model;

[Guid(PackageGuids.guidXrmPluginCodeGeneratorString)]
[ComVisible(true)]
internal interface IXrmCodeGenerator
{
    public XrmCodeGenConfig? Config { get; set; }
    ValidationResult IsValid(PluginAssemblyConfig inputModel);
    //TODO: Consider replacing the following two methods with
    // a single method that accepts an object and remove `IsValid`
    // and maybe also `Config` can be sent directly to the method.
    string GenerateCode(PluginAssemblyConfig inputModel);
    string GenerateCode(FetchQuery inputModel);
}
#nullable restore