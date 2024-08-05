#nullable enable
using Microsoft.Xrm.Sdk.Metadata;
using System;

namespace XrmGen.Xrm.Model;

public class MessageProcessingStep
{
    private MessageProcessingStepImage[]? _images = null;
    public string? Id { get; set; }
    /// <summary>
    /// Required, String
    /// </summary>
    public string? Name { get; set; }
    /// <summary>
    /// Required, Boolean
    /// </summary>
    public bool AsyncAutoDelete { get; set; } = true;
    public object? CustomConfiguration { get; set; }
    public object? Description { get; set; }
    public string? FilteringAttributes { get; set; }
    public string? ImpersonatingUserFullname { get; set; }
    public int? InvocationSource { get; set; }
    public required string MessageName { get; set; }
    /// <summary>
    /// Required, Picklist
    /// </summary>
    public int Mode { get; set; }
    public string? PrimaryEntityName { get; set; }
    /// <summary>
    /// Contains the metadata for the primary entity. Attributes are filtered to only include the attributes 
    /// that are used in the message processing step.
    /// </summary>
    public EntityMetadata? PrimaryEntityMetadata { get; set; }
    /// <summary>
    /// Required, Integer
    /// </summary>
    public int Rank { get; set; }
    /// <summary>
    /// Required, Lookup
    /// </summary>
    public string? SdkMessageId { get; set; }
    /// <summary>
    /// Required, Picklist
    /// </summary>
    public int Stage { get; set; }
    /// <summary>
    /// Required, State
    /// </summary>
    public int StateCode { get; set; }
    /// <summary>
    /// Required, Picklist
    /// </summary>
    public int SupportedDeployment { get; set; }
    /// <summary>
    /// Required, Uniqueidentifier
    /// </summary>
    public Guid SolutionId { get; set; }
    /// <summary>
    /// Required, Uniqueidentifier
    /// </summary>
    public Guid SdkMessageProcessingStepIdUnique { get; set; }
    /// <summary>
    /// Required, Uniqueidentifier
    /// </summary>
    public Guid SdkMessageProcessingStepId { get; set; }
    public MessageProcessingStepImage[] Images 
    {
        get => _images ??= [];
        set => _images = value;
    }
}
#nullable restore