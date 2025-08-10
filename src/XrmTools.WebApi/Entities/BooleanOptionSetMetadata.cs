namespace XrmTools.WebApi.Entities;

using XrmTools.WebApi.Entities.Attributes;

[EntityMetadata("BooleanOptionSetMetadata", "BooleanOptionSetDefinitions")]
public sealed class BooleanOptionSetMetadata : OptionSetMetadataBase
{
    public OptionMetadata? TrueOption { get; set; }

    public OptionMetadata? FalseOption { get; set; }

    public BooleanOptionSetMetadata() { }

    public BooleanOptionSetMetadata(OptionMetadata trueOption, OptionMetadata falseOption)
    {
        TrueOption = trueOption;
        FalseOption = falseOption;
    }
}
