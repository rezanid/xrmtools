namespace XrmTools.WebApi.Entities;

using XrmTools.WebApi.Entities.Attributes;
using XrmTools.WebApi.Types;

[EntityMetadata("LookupAttributeMetadata", "LookupAttributeDefinitions")]
public sealed class LookupAttributeMetadata : AttributeMetadata
{
    public string[] Targets { get; set; } = [];

    public LookupFormat? Format { get; set; }

    public LookupAttributeMetadata()
    : this(null)
    {
    }

    public LookupAttributeMetadata(LookupFormat? format)
        : base(AttributeTypeCode.Lookup)
    {
        Format = format;
    }
}