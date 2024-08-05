#nullable enable
using Microsoft.Xrm.Sdk.Metadata;

namespace XrmGen.Xrm.Model;

public class MessageProcessingStepImage
{
    public string? Id { get; set; }
    public required string Name { get; set; }
    public required string Attributes { get; set; }
    public required string EntityAlias { get; set; }
    public int ImageType { get; set; }
    public required string MessagePropertyName { get; set; }
    public EntityMetadata? MessagePropertyMetadata { get; set; }
}
#nullable restore