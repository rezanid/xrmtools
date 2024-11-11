#nullable enable
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.ComponentModel.Composition;

namespace XrmTools.Xrm.Generators;

[Export(typeof(IXrmEntityCodeGenerator))]
public class TemplatedEntityCodeGenerator : IXrmEntityCodeGenerator
{
    public XrmCodeGenConfig? Config { set; get; }

    public (bool, string) IsValid(EntityMetadata plugin)
    {
        if (plugin is null) { throw new ArgumentNullException(nameof(plugin)); }
        if (string.IsNullOrWhiteSpace(plugin.LogicalName)) { return (false, Resources.Strings.EntityGenerator_NoLogicalName); }
        return (true, string.Empty);
    }

    public string GenerateCode(EntityMetadata entityMetadata)
    {
        return "// Goodluck!";
    }
}
#nullable restore