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

    [JsonIgnore]
    public ICollection<EntityMetadata>? EntityDefinitions { get; set; }

    [JsonIgnore]
    public string? FilePath { get; set; }


    [JsonPropertyOrder(1)]
    public CodeGenReplacePrefixConfig ReplacePrefixes { get; set; } = new();

    public Solution? Solution { get; set; }

    public new ICollection<PluginTypeConfig> PluginTypes { get; set; } = [];
}