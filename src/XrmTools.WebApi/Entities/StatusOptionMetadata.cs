namespace XrmTools.WebApi.Entities;
using XrmTools.WebApi.Entities.Attributes;

[EntityMetadata("StatusOptionMetadata", "StatusOptionDefinitions")]
public sealed class StatusOptionMetadata : OptionMetadata
{
    public int? State { get; set; }

    public string? TransitionData { get; set; }

    public StatusOptionMetadata() { }

    public StatusOptionMetadata(int value, int? state) : base(value)
    {
        State = state;
    }
}