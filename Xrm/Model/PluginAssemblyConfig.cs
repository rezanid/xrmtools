#nullable enable
namespace XrmGen.Xrm.Model;

using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing.Text;
using System.Runtime.Serialization;

public class PluginAssemblyConfig
{
    private string? removePrefixes;

    public Guid Id { get; set; }
    public required string Name { get; set; }
    public int IsolationMode { get; set; }
    public int SourceType { get; set; }
    public string? Version { get; set; }
    public required IEnumerable<Plugin> PluginTypes { get; set; }
    
    /// <summary>
    /// List of all extra entities (and their comma-delimited attributes) that will be generated.
    /// </summary>
    public IEnumerable<EntityConfig>? Entities { get; set; }

    /// <summary>
    /// Dynamically generated from the PluginTypes.
    /// </summary>
    public IEnumerable<EntityMetadata>? EntityDefinitions { get; set; }

    public string? RemovePrefixes
    {
        get => removePrefixes;
        set
        {
            removePrefixes = value;
            if (value == null && RemovePrefixesCollection.Count == 0) { return; }
            RemovePrefixesCollection = new ReadOnlyCollection<string>(value?.Split(',') ?? []);
        }
    }

    [IgnoreDataMember]
    public IReadOnlyCollection<string> RemovePrefixesCollection { private set; get; } = [];
}

public class EntityConfig
{
    public required string LogicalName { get; set; }
    public string? Attributes { get; set; }
}
#nullable restore