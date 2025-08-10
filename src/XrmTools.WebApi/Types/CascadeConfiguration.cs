namespace XrmTools.WebApi.Types;
using System.Text.Json.Serialization;

public sealed class CascadeConfiguration
{
    public CascadeType? Assign { get; set; }

    public CascadeType? Delete { get; set; }

    public CascadeType? Archive { get; set; }

    public CascadeType? Merge { get; set; }

    public CascadeType? Reparent { get; set; }

    public CascadeType? Share { get; set; }

    public CascadeType? Unshare { get; set; }

    [JsonPropertyOrder(82)]
    public CascadeType? RollupView { get; set; }

}
