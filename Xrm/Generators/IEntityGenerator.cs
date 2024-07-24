using Microsoft.Xrm.Sdk.Metadata;
using System.Text;

namespace XrmGen.Xrm.Generators;

public interface IEntityGenerator
{
    public CodeGenConfig Config { set; }
    (bool, string) IsValid(EntityMetadata plugin);
    void GenerateCode(StringBuilder builder, EntityMetadata entityMetadata, string suggestedNamespace);
}
