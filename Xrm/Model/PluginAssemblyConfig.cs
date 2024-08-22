#nullable enable

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Metadata;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace XrmGen.Xrm.Model;

public interface IPluginAssemblyConfig : IPluginAssemblyEntity
{
    ICollection<EntityConfig> Entities { get; set; }
    ICollection<EntityMetadata> EntityDefinitions { get; set; }
    string? RemovePrefixes { get; set; }
    IReadOnlyCollection<string> RemovePrefixesCollection { get; }
}

public interface IPluginAssemblyEntity
{
    int? IsolationMode { get; set; }
    int? Major { get; set; }
    int? Minor { get; set; }
    string? Name { get; set; }
    ICollection<PluginTypeConfig> PluginTypes { get; set; }
    string? PublicKeyToken { get; set; }
    EntityReference? SolutionId { get; set; }
    int? SourceType { get; set; }
    string? Version { get; set; }
}

[EntityLogicalName(EntityLogicalName)]
public class PluginAssemblyConfig : TypedEntity<PluginAssemblyConfig>, IPluginAssemblyConfig, INotifyPropertyChanged
{
    public const string EntityLogicalName = "pluginassembly";

    private ICollection<PluginTypeConfig> pluginTypes = [];

    #region IPluginAssemblyConfig-only Properties

    private string? removePrefixes;

    /// <summary>
    /// List of all extra entities (and their comma-delimited attributes) that will be generated.
    /// </summary>
    public ICollection<EntityConfig>? Entities { get; set; }

    /// <summary>
    /// Dynamically generated from the PluginTypes.
    /// </summary>
    [IgnoreDataMember]
    public ICollection<EntityMetadata>? EntityDefinitions { get; set; }

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

    [JsonProperty("PluginTypes")]
    public ICollection<PluginTypeConfig> PluginTypes
    {
        get => pluginTypes;
        set
        {
            pluginTypes = value;
            OnPropertyChanged(nameof(PluginTypes));
        }
    }

    #endregion

    #region IPluginAssemblyEntity Properties
    [AttributeLogicalName("name")]
    public string? Name
    {
        get => TryGetAttributeValue("name", out string? value) ? value : null;
        set => this["name"] = value;
    }

    [AttributeLogicalName("isolationmode")]
    public int? IsolationMode
    {
        get => TryGetAttributeValue("isolationmode", out int? value) ? value : null;
        set => this["isolationmode"] = value;
    }

    [AttributeLogicalName("sourcetype")]
    public int? SourceType
    {
        get => TryGetAttributeValue("sourcetype", out int? value) ? value : null;
        set => this["sourcetype"] = value;
    }

    [AttributeLogicalName("major")]
    public int? Major
    {
        get => TryGetAttributeValue("major", out int? value) ? value : null;
        set => this["major"] = value;
    }

    [AttributeLogicalName("minor")]
    public int? Minor
    {
        get => TryGetAttributeValue("minor", out int? value) ? value : null;
        set => this["minor"] = value;
    }

    [AttributeLogicalName("publickeytoken")]
    public string? PublicKeyToken
    {
        get => TryGetAttributeValue("publickeytoken", out string? value) ? value : null;
        set => this["publickeytoken"] = value;
    }

    [AttributeLogicalName("solutionid")]
    public EntityReference? SolutionId
    {
        get => TryGetAttributeValue("solutionid", out EntityReference? value) ? value : null;
        set => this["solutionid"] = value;
    }

    [AttributeLogicalName("version")]
    public string? Version
    {
        get => TryGetAttributeValue("version", out string? value) ? value : null;
        set => this["version"] = value;
    }
    #endregion

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public PluginAssemblyConfig() : base(EntityLogicalName) { }
}

public class EntityConfig
{
    public string? LogicalName { get; set; }
    public string? Attributes { get; set; }
}
#nullable restore