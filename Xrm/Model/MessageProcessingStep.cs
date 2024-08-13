#nullable enable
using Microsoft.Xrm.Sdk.Metadata;
using System;

namespace XrmGen.Xrm.Model;

public enum Stages
{
    /// <summary>
    /// pre-validation (not in transaction).
    /// </summary>
    PreValidation = 10,
    /// <summary>
    /// pre-operation (in transaction, ownerid cannot be changed).
    /// </summary>
    PreOperation = 20,
    //
    // Summary:
    //     
    /// <summary>
    /// Main operation (in transaction, only used in Custom APIs).
    /// </summary>
    MainOperation = 30,
    /// <summary>
    /// post-operation (operation executed, but still in transaction).
    /// </summary>
    PostOperation = 40,
    /// <summary>
    /// post-operation, deprecated.
    /// </summary>
    [Obsolete("Deprecated according to MS", true)]
    DepecratedPostOperation = 50
}

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
    public EntityMetadata? PrimaryEntityDefinition { get; set; }
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
    public Stages Stage { get; set; }
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