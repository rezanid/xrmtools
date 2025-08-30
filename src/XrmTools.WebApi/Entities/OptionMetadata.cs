namespace XrmTools.WebApi.Entities;

using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using XrmTools.WebApi.Entities.Attributes;
using XrmTools.WebApi.Types;

[KnownType(typeof(StateOptionMetadata))]
[KnownType(typeof(StatusOptionMetadata))]
[EntityMetadata("OptionMetadata", "OptionDefinitions")]
public class OptionMetadata : MetadataBase
{
    public int? Value { get; set; }

    public Label? Label { get; set; }

    public Label? Description { get; set; }

    public string? Color { get; set; }

    public bool? IsManaged { get; internal set; }

    public string? ExternalValue { get; set; }

    [JsonPropertyOrder(91)]
    public int[] ParentValues { get; set; } = [];

    /// <summary>
    /// This is used for EntityNameAttribute option value to indicates to which entity
    /// it points this property is a readonly property whose value is populated at runtime
    /// </summary>
    public string? Tag { get; set; }

    public bool IsHidden { get; set; }

    public OptionMetadata()
    {
        ExternalValue = string.Empty;
    }

    public OptionMetadata(int value)
        : this()
    {
        Value = value;
    }

    public OptionMetadata(int value, IEnumerable<int> parentOptionValues)
        : this(null, value, parentOptionValues)
    {
    }

    public OptionMetadata(Label? label, int? value)
    {
        Label = label;
        Value = value;
    }

    public OptionMetadata(Label? label, int? value, IEnumerable<int> parentOptionValues)
    {
        Label = label;
        Value = value;
        ExternalValue = string.Empty;
        if (parentOptionValues != null)
        {
            ParentValues = (parentOptionValues as int[]) ?? parentOptionValues.ToArray();
        }
    }
}