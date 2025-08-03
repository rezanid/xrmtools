namespace XrmTools.WebApi.Entities;

using XrmTools.WebApi.Entities.Attributes;

[EntityMetadata("StateOptionMetadata", "StateOptionDefinitions")]
public sealed class StateOptionMetadata : OptionMetadata
{
    public int? DefaultStatus { get; set; }

    public string? InvariantName { get; set; }
}
