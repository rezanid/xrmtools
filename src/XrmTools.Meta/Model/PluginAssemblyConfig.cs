#nullable enable
namespace XrmTools.Xrm.Model;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Metadata;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using XrmTools.Meta.Attributes.Serialization;
using XrmTools.Meta.Attributes;

public interface IPluginAssemblyConfig : IPluginAssemblyEntity
{
    WebApi.Entities.Solution? Solution { get; set; }
    ICollection<EntityConfig> Entities { get; set; }
    ICollection<EntityMetadata>? EntityDefinitions { get; set; }
    CodeGenReplacePrefixConfig ReplacePrefixes { get; set; }
    string? FilePath { get; set; }
}

public interface IPluginAssemblyEntity
{
    IsolationModes? IsolationMode { get; set; }
    Guid? PluginAssemblyId { get; set; }
    string? Name { get; set; }
    ICollection<PluginTypeConfig> PluginTypes { get; set; }
    string? PublicKeyToken { get; set; }
    SourceTypes? SourceType { get; set; }
    string? Version { get; set; }
    WebApi.Entities.PluginPackage? Package { get; set; }
}

[EntityLogicalName(EntityLogicalName)]
public class PluginAssemblyConfig : TypedEntity<PluginAssemblyConfig>, IPluginAssemblyConfig, INotifyPropertyChanged
{
    public const string EntityLogicalName = "pluginassembly";
    public const string EntitySetName = "pluginassemblies";

    public override string GetEntitySetName() => EntitySetName;

    private ICollection<PluginTypeConfig> pluginTypes = [];

    #region IPluginAssemblyConfig-only Properties

    /// <summary>
    /// List of all extra entities (and their comma-delimited attributes) that will be generated.
    /// </summary>
    public ICollection<EntityConfig> Entities { get; set; } = [];

    /// <summary>
    /// Dynamically generated from the PluginTypes.
    /// </summary>
    [IgnoreDataMember]
    public ICollection<EntityMetadata>? EntityDefinitions { get; set; }

    [JsonPropertyOrder(2)]
    [JsonProperty(Order = 1)]
    public CodeGenReplacePrefixConfig ReplacePrefixes { get; set; } = new();

    [IgnoreDataMember]
    public string? FilePath { get; set; }

    [JsonPropertyOrder(1)]
    //[JsonProperty("PluginTypes", Order = 1)]
    [JsonProperty("pluginassembly_plugintype", Order = 1)]
    public ICollection<PluginTypeConfig> PluginTypes
    {
        get => pluginTypes;
        set
        {
            pluginTypes = value;
            OnPropertyChanged(nameof(PluginTypes));
        }
    }

    public WebApi.Entities.PluginPackage? Package { get; set; }

    public WebApi.Entities.Solution? Solution { get; set; }
    #endregion

    #region IPluginAssemblyEntity Properties
    [AttributeLogicalName("pluginassemblyid")]
    [JsonPrimaryKey]
    public Guid? PluginAssemblyId
    {
        get => TryGetAttributeValue("pluginassemblyid", out Guid value) ? value : null;
        set
        {
            this["pluginassemblyid"] = value;
            Id = value == null ? Guid.Empty : value.Value;
        }
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


    [AttributeLogicalName("version")]
    public string? Version
    {
        get => TryGetAttributeValue("version", out string? value) ? value : null;
        set => this["version"] = value;
    }

    [AttributeLogicalName("content")]
    public string? Content
    {
        get => TryGetAttributeValue("content", out string? value) ? value : null;
        set => this["content"] = value;
    }
    #endregion

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public PluginAssemblyConfig() : base(EntityLogicalName) { }
    public PluginAssemblyConfig(string filePath) : base(EntityLogicalName) => FilePath = filePath;
}

public class EntityConfig
{
    public string? LogicalName { get; set; }
    public string? AttributeNames { get; set; }
}
#nullable restore