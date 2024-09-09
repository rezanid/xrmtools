#nullable enable
namespace XrmGen.Xrm.Generators;

using Microsoft.Xrm.Sdk.Metadata;
using System.Runtime.InteropServices;

[Guid(PackageGuids.guidXrmEntityCodeGeneratorString)]
[ComVisible(true)]
public interface IXrmEntityCodeGenerator
{
    public XrmCodeGenConfig? Config { get; set; }
    (bool, string) IsValid(EntityMetadata plugin);
    string GenerateCode(EntityMetadata entityMetadata);
}
#nullable restore