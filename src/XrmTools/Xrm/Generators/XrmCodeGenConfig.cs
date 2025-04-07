using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Serialization;

namespace XrmTools.Xrm.Generators;

public class XrmCodeGenConfig : IEquatable<XrmCodeGenConfig>
{
    public required string DefaultNamespace { get; set; }
    public required string TemplateFilePath { get; set; }
    public IEnumerable<EntityConfig> Entities { get; set; }
    public ICollection<EntityMetadata> EntityDefinitions { get; set; }
    public IEnumerable<string> RemovePrefixes { get; set; } = [];

    #region IEquatable
    public bool Equals(XrmCodeGenConfig other)
    {
        if (other == null) return false;
        return DefaultNamespace == other.DefaultNamespace &&
               TemplateFilePath == other.TemplateFilePath &&
               (Entities == null && other.Entities == null ||
                Entities != null && other.Entities != null && Entities.SequenceEqual(other.Entities));
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as XrmCodeGenConfig);
    }

    public override int GetHashCode()
    {
        int hash = 17;
        hash = hash * 23 + (DefaultNamespace?.GetHashCode() ?? 0);
        hash = hash * 23 + (TemplateFilePath?.GetHashCode() ?? 0);
        hash = hash * 23 + (Entities?.Aggregate(0, (acc, entity) => acc + entity.GetHashCode()) ?? 0);
        return hash;
    }
    #endregion
}

public class EntityConfig : IEquatable<EntityConfig>
{
    [YamlMember(Alias = "entity")]
    public string LogicalName { get; set; }
    public string AttributeNames { get; set; }

    #region IEquatable
    public bool Equals(EntityConfig other)
    {
        if (other == null) return false;
        return LogicalName == other.LogicalName &&
               AttributeNames == other.AttributeNames;
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as EntityConfig);
    }

    public override int GetHashCode()
    {
        int hash = 17;
        hash = hash * 23 + (LogicalName?.GetHashCode() ?? 0);
        hash = hash * 23 + (AttributeNames?.GetHashCode() ?? 0);
        return hash;
    }
    #endregion
}
