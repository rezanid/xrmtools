#nullable enable
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

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
    EntityReference? SdkMessageId { get; set; }
    bool AsyncAutoDelete { get; set; }
    string? Description { get; set; }
    string? FilteringAttributes { get; set; }
    int? InvocationSource { get; set; }
    string? MessageName { get; set; }
    string? Name { get; set; }
    string? PrimaryEntityName { get; set; }
    object? CustomConfiguration { get; set; }
    int? Mode { get; set; }
    int? Rank { get; set; }
    Stages? Stage { get; set; }
    int? StateCode { get; set; }
    int? SupportedDeployment { get; set; }
}

public interface IMessageProcessingStepConfig : IMessageProcessingStepEntity
{
    ICollection<PluginStepImageConfig> Images { get; set; }
    EntityMetadata? PrimaryEntityDefinition { get; set; }
    public string? StageName { get; }
}

[EntityLogicalName(EntityLogicalName)]
public class PluginStepConfig : TypedEntity<PluginStepConfig>, IMessageProcessingStepConfig
{
    public const string EntityLogicalName = "sdkmessageprocessingstep";

    //TODO: Do we need to support secure config?
    // Unsupported for now:
    // - SecureConfig
    // - ImpersonatingUserFullName

    #region IMessageProcessingStepConfig-only Properties
    /// <summary>
    /// Contains the metadata for the primary entity. Attributes are filtered to only include the attributes 
    /// that are used in the message processing step.
    /// </summary>
    public EntityMetadata? PrimaryEntityDefinition { get; set; }
    public ICollection<PluginStepImageConfig> Images { get; set; } = [];
    public object? CustomConfiguration { get; set; }
    public string? StageName
    {
        get => Stage switch
        {
            Stages.PreValidation => "PreValidation",
            Stages.PreOperation => "PreOperation",
            Stages.MainOperation => "MainOperation",
            Stages.PostOperation => "PostOperation",
            null => null,
            _ => throw new InvalidOperationException($"Unknown stage: {Stage}")
        };
    }
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

    public static LinkEntity LinkWithImages(string[] columns, JoinOperator join = JoinOperator.LeftOuter)
        => new(EntityLogicalName, PluginStepImageConfig.EntityLogicalName, "sdkmessageprocessingstepid", "sdkmessageprocessingstepid", join)
        {
            EntityAlias = PluginStepImageConfig.EntityLogicalName,
            Columns = new ColumnSet(columns)
        };

    public static LinkEntity LinkWithImages(
        Expression<Func<PluginStepImageConfig, object>> columnsExpression, JoinOperator join = JoinOperator.LeftOuter)
        => LinkWithImages(PluginStepImageConfig.GetColumnsFromExpression(columnsExpression), join);

    public PluginStepConfig() : base(EntityLogicalName) { }
}
#nullable restore