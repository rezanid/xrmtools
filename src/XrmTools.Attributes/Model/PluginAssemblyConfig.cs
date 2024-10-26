#nullable enable
namespace XrmTools.Xrm.Model;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Metadata;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using XrmTools.Meta.Model;

public interface IPluginAssemblyConfig : IPluginAssemblyEntity
{
    ICollection<EntityConfig>? Entities { get; set; }
    ICollection<EntityMetadata>? EntityDefinitions { get; set; }
    string? RemovePrefixes { get; set; }
    IReadOnlyCollection<string> RemovePrefixesCollection { get; }
}

public interface IPluginAssemblyEntity
{
    IsolationModes? IsolationMode { get; set; }
    Guid? PluginAssemblyId { get; set; }
    string? Name { get; set; }
    ICollection<PluginTypeConfig> PluginTypes { get; set; }
    string? PublicKeyToken { get; set; }
    EntityReference? SolutionId { get; set; }
    SourceTypes? SourceType { get; set; }
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

    [JsonPropertyOrder(2)]
    [JsonProperty(Order = 1)]
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

    [JsonPropertyOrder(1)]
    [JsonProperty("PluginTypes", Order = 1)]
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
    [AttributeLogicalName("pluginassemblyid")]
    [JsonPropertyName("Id")]
    [JsonProperty("Id")]
    public Guid? PluginAssemblyId
    {
        get => TryGetAttributeValue("pluginassemblyid", out Guid value) ? value : null;
        set => this["pluginassemblyid"] = value;
    }
    [AttributeLogicalName("name")]
    public string? Name
    {
        get => TryGetAttributeValue("name", out string? value) ? value : null;
        set => this["name"] = value;
    }

    [AttributeLogicalName("isolationmode")]
    public IsolationModes? IsolationMode
    {
        get => TryGetAttributeValue("isolationmode", out OptionSetValue option) ? (IsolationModes)option.Value : null;
        set => this["isolationmode"] = value == null ? null : new OptionSetValue((int)value);
    }

    [AttributeLogicalName("sourcetype")]
    public SourceTypes? SourceType
    {
        get => TryGetAttributeValue("sourcetype", out OptionSetValue option) ? (SourceTypes)option.Value : null;
        set => this["sourcetype"] = value == null ? null : new OptionSetValue((int)value);
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

    public event PropertyChangedEventHandler? PropertyChanged;

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