#nullable enable
namespace XrmTools.Xrm.Generators;

using System.Runtime.InteropServices;

[Guid(PackageGuids.guidXrmEntityCodeGeneratorString)]
[ComVisible(true)]
public interface IXrmEntityCodeGenerator
{
    public XrmCodeGenConfig? Config { get; set; }
    (bool, string) IsValid(XrmCodeGenConfig input);
    string GenerateCode(XrmCodeGenConfig input);
}
#nullable restore