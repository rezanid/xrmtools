﻿using System.Runtime.InteropServices;
#nullable enable
namespace XrmTools.Xrm.Generators;

using XrmTools.Xrm.Model;

[Guid(PackageGuids.guidXrmPluginCodeGeneratorString)]
[ComVisible(true)]
public interface IXrmPluginCodeGenerator
{
    public XrmCodeGenConfig? Config { get; set; }
    (bool, string) IsValid(PluginAssemblyConfig plugin);
    string GenerateCode(PluginAssemblyConfig plugin);
}
#nullable restore