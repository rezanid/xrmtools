using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace XrmGen.Xrm.Generators;

public class CodeGenConfig
{
    public List<EntityConfig> Entities { get; set; }
}

public class EntityConfig
{
    [YamlMember(Alias = "entity")]
    public string LogicalName { get; set; }
    public string AttributeNames { get; set; }
}
