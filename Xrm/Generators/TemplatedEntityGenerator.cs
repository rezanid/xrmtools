using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.ComponentModel.Composition;
using System.Text;

namespace XrmGen.Xrm.Generators;

[Export(typeof(IEntityGenerator))]
public class TemplatedEntityGenerator : IEntityGenerator
{
    public CodeGenConfig Config { set; private get; }

    public (bool, string) IsValid(EntityMetadata plugin)
    {
        if (plugin is null) { throw new ArgumentNullException(nameof(plugin)); }
        if (string.IsNullOrWhiteSpace(plugin.LogicalName)) { return (false, Resources.Strings.EntityGenerator_NoLogicalName); }
        return (true, string.Empty);
    }

    public void GenerateCode(StringBuilder builder, EntityMetadata entityMetadata, string suggestedNamespace)
    {
        return;
    }
}
