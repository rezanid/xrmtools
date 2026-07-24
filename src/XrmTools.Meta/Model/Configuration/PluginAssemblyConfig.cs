#nullable enable
namespace XrmTools.Meta.Model.Configuration;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using XrmTools.Meta.Model;
using XrmTools.WebApi.Entities;

public sealed class PluginAssemblyConfig : PluginAssembly//, IPluginAssemblyConfig, INotifyPropertyChanged
{
    #region INotifyPropertyChanged Implementation
    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(field, value)) return false;
        field = value;
        if (propertyName is not null) OnPropertyChanged(propertyName);
        return true;
    }
    #endregion

    /// <summary>
    /// List of all extra entities (and their comma-delimited attributes) that will be generated.
    /// </summary>
    public ICollection<EntityConfig> Entities { get; set; } = [];

    /// <summary>
    /// Gets or sets the collection of additional entity that are not part of current
    /// scope. When parsing a C# document, it refers to other entities not in the document.
    /// </summary>
    public ICollection<EntityConfig> OtherEntities { get; set; } = [];

    [JsonIgnore]
    public ICollection<EntityMetadata?>? EntityDefinitions { get; set; }

    [JsonIgnore]
    public ICollection<EntityMetadata>? OtherEntityDefinitions { get; set; }

    [JsonIgnore]
    public ICollection<OptionSetMetadata>? GlobalOptionSetDefinitions { get; set; }

    [JsonIgnore]
    public string? FilePath { get; set; }

    [JsonPropertyOrder(1)]
    public CodeGenReplacePrefixConfig[] ReplacePrefixes { get; set; } = [];

    [JsonPropertyOrder(2)]
    public CodeGenGlobalOptionSetsConfig GlobalOptionSetCodeGen { get; set; } = new();

    [JsonPropertyOrder(3)]
    public CodeGenOrganizationContextConfig? OrganizationContextConfig { get; set; } = new();

    [JsonPropertyOrder(4)]
    public CodeGenNameCollisionConfig NameCollision { get; set; } = new();

    public Solution? Solution { get; set; }

    public new ICollection<PluginTypeConfig> PluginTypes { get; set; } = [];

    public ICollection<PluginTypeConfig> OtherPluginTypes { get; set; } = [];

    /// <summary>
    /// Gets or sets the set of every plugin type name (types implementing
    /// <c>Microsoft.Xrm.Sdk.IPlugin</c>) compiled into the assembly, regardless of whether they are
    /// annotated with XrmTools attributes. This is the authoritative source for deciding whether an
    /// existing Dataverse plugin type still exists in the assembly (and therefore must be kept) or was
    /// genuinely removed/renamed in code (and can be deleted). It is null when the plugin type set
    /// could not be determined (e.g. <c>IPlugin</c> is not resolvable), in which case callers must not
    /// delete any plugin types.
    /// </summary>
    [JsonIgnore]
    public ISet<string>? AssemblyPluginTypeNames { get; set; }
}
#nullable restore