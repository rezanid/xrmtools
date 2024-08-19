#nullable enable
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;

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

public interface IMessageProcessingStepEntity
{
    bool AsyncAutoDelete { get; set; }
    string? Description { get; set; }
    string? FilteringAttributes { get; set; }
    int? InvocationSource { get; set; }
    string? MessageName { get; set; }
    int? Mode { get; set; }
    string? Name { get; set; }
    string? PrimaryEntityName { get; set; }
    int? Rank { get; set; }
    EntityReference? SdkMessageId { get; set; }
    Stages? Stage { get; set; }
    int? StateCode { get; set; }
    int? SupportedDeployment { get; set; }
}

public interface IMessageProcessingStepConfig : IMessageProcessingStepEntity
{
    ICollection<MessageProcessingStepImage> Images { get; set; }
    EntityMetadata? PrimaryEntityDefinition { get; set; }
    /// <summary>
    /// Secure Config
    /// </summary>
    object? CustomConfiguration { get; set; }
}

[EntityLogicalName(EntityLogicalName)]
public class MessageProcessingStep : TypedEntity<MessageProcessingStep>, IMessageProcessingStepConfig
{
    public const string EntityLogicalName = "sdkmessageprocessingstep";

    #region IMessageProcessingStepConfig-only Properties
    /// <summary>
    /// Contains the metadata for the primary entity. Attributes are filtered to only include the attributes 
    /// that are used in the message processing step.
    /// </summary>
    public EntityMetadata? PrimaryEntityDefinition { get; set; }
    public ICollection<MessageProcessingStepImage> Images { get; set; } = [];
    public object? CustomConfiguration { get; set; }
    #endregion

    #region IMessageProcessingStepEntity Properties
    /// <summary>
    /// Required
    /// </summary>
    [AttributeLogicalName("name")]
    public string? Name
    {
        get => TryGetAttributeValue("name", out string value) ? value : null;
        set => this["name"] = value;
    }
    /// <summary>
    /// Required, Boolean
    /// </summary>
    [AttributeLogicalName("asyncautodelete")]
    public bool AsyncAutoDelete
    {
        get => TryGetAttributeValue(FilteringAttributes, out bool value) && value;
        set => this["asyncautodelete"] = value;
    }
    [AttributeLogicalName("description")]
    public string? Description 
    { 
        get => TryGetAttributeValue("description", out string value) ? value : null;
        set => this["description"] = value;
    }
    [AttributeLogicalName("filteringattributes")]
    public string? FilteringAttributes 
    {
        get => TryGetAttributeValue("filteringattributes", out string value) ? value : null;
        set => this["filteringattributes"] = value;
    }
    [AttributeLogicalName("invocationsource")]
    public int? InvocationSource 
    { 
        get => TryGetAttributeValue("invocationsource", out int value) ? value : null;
        set => this["invocationsource"] = value;
    }
    [AttributeLogicalName("messagename")]
    public string? MessageName 
    { 
        get => TryGetAttributeValue("messagename", out string value) ? value : null;
        set => this["messagename"] = value;
    }
    /// <summary>
    /// Required, Picklist
    /// </summary>
    [AttributeLogicalName("mode")]
    public int? Mode 
    { 
        get => TryGetAttributeValue("mode", out int? value) ? value : null;
        set => this["mode"] = value;
    }
    [AttributeLogicalName("primaryentityname")]
    public string? PrimaryEntityName 
    { 
        get => TryGetAttributeValue("primaryentityname", out string value) ? value : null;
        set => this["primaryentityname"] = value;
    }
    /// <summary>
    /// Required, Integer
    /// </summary>
    [AttributeLogicalName("rank")]
    public int? Rank 
    { 
        get => TryGetAttributeValue("rank", out int value) ? value : null;
        set => this["rank"] = value;
    }
    /// <summary>
    /// Required, Lookup
    /// </summary>
    [AttributeLogicalName("sdkmessageid")]
    public EntityReference? SdkMessageId 
    { 
        get => TryGetAttributeValue("sdkmessageid", out EntityReference? value) ? value : null;
        set => this["sdkmessageid"] = value;
    }
    /// <summary>
    /// Required, Picklist
    /// </summary>
    [AttributeLogicalName("stage")]
    public Stages? Stage 
    { 
        get => TryGetAttributeValue("stage", out OptionSetValue option) ? (Stages)option.Value : null;
        set => this["stage"] = value == null ? null : new OptionSetValue((int)value);
    }
    /// <summary>
    /// Required, State
    /// </summary>
    [AttributeLogicalName("statecode")]
    public int? StateCode 
    {
        get => TryGetAttributeValue("statecode", out int? value) ? value : null;
        set => this["statecode"] = value;
    }
    /// <summary>
    /// Required, Picklist
    /// </summary>
    [AttributeLogicalName("supporteddeployment")]
    public int? SupportedDeployment 
    { 
        get => TryGetAttributeValue("supporteddeployment", out int? value) ? value : 0;
        set => this["supporteddeployment"] = value;
    }
    #endregion

    public MessageProcessingStep() : base(EntityLogicalName) {}
}
#nullable restore