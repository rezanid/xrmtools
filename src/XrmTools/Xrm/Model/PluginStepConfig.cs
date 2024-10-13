#nullable enable
namespace XrmTools.Xrm.Model;

using HandyControl.Tools.Extension;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.Json.Serialization;

public enum PluginStepStates
{
    Active = 0,
    Inactive = 1
}

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

public enum SupportedDeployments
{
    ServerOnly = 0,
    ClientForOutlook = 1,
    Both = 2
}

public interface IMessageProcessingStepEntity
{
    Guid? PluginStepId { get; set; }
    Guid? SdkMessageId { get; set; }
    bool AsyncAutoDelete { get; set; }
    string? Description { get; set; }
    string? FilteringAttributes { get; set; }
    int? InvocationSource { get; set; }
    //TODO: MessageName does not exist, instead there is SdkMessageId
    string? MessageName { get; set; }
    string? Name { get; set; }
    //TODO: PrimaryEntityName does not exist, instead there is EventHandlerTypeCode (maybe?)
    string? PrimaryEntityName { get; set; }
    object? CustomConfiguration { get; set; }
    int? Mode { get; set; }
    int? Rank { get; set; }
    Stages? Stage { get; set; }
    PluginStepStates? State { get; set; }
    SupportedDeployments? SupportedDeployment { get; set; }
    //TODO: ImpersonatingUserFullname does not exist, instead there is ImpersonatingUserId
    //string? ImpersonatingUserFullname { get; set; }
    string? WorkflowActivityGroupName { get; set; }
    string? FriendlyName { get; set; }
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
    [JsonProperty(Order = 1)]
    [JsonPropertyOrder(1)]
    public ICollection<PluginStepImageConfig> Images { get; set; } = [];
    public object ActionDefinition { get; set; }
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
            _ => $"Unknown stage: {Stage}"
        };
    }
    #endregion

    #region IMessageProcessingStepEntity Properties
    [AttributeLogicalName("sdkmessageprocessingstepid")]
    [JsonPropertyName("Id")]
    [JsonProperty("Id")]
    public Guid? PluginStepId
    {
        get => TryGetAttributeValue("sdkmessageprocessingstepid", out Guid value) ? value : null;
        set => this["sdkmessageprocessingstepid"] = value;
    }
    /// <summary>
    /// Required
    /// </summary>
    [AttributeLogicalName("name")]
    public string? Name
    {
        get => TryGetAttributeValue("name", out string value) ? value : null;
        set => this["name"] = value;
    }
    [AttributeLogicalName("friendlyname")]
    public string? FriendlyName
    {
        get => TryGetAttributeValue("friendlyname", out string value) ? value : null;
        set => this["friendlyname"] = value;
    }
    [AttributeLogicalName("workflowactivitygroupname")]
    public string? WorkflowActivityGroupName
    {
        get => TryGetAttributeValue("workflowactivitygroupname", out string value) ? value : null;
        set => this["workflowactivitygroupname"] = value;
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
    /// <summary>
    /// Required, Picklist
    /// </summary>
    [AttributeLogicalName("mode")]
    public int? Mode 
    { 
        get => TryGetAttributeValue("mode", out int? value) ? value : null;
        set => this["mode"] = value;
    }
    //TODO: PrimaryEntityName does not exist, instead there is EventHandlerTypeCode (maybe?)
    //[AttributeLogicalName("primaryentityname")]
    public string? PrimaryEntityName { get; set; }
    //{
    //    get => TryGetAttributeValue("primaryentityname", out string value) ? value : null;
    //    set => this["primaryentityname"] = value;
    //}
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
    public Guid? SdkMessageId 
    { 
        get => TryGetAttributeValue("sdkmessageid", out EntityReference? value) ? value.Id : null;
        set
        {
            if (value == null)
            {
                this["sdkmessageid"] = null;
                return;
            }
            var currentValue = GetAttributeValue<EntityReference>("sdkmessageid");
            currentValue ??= new EntityReference("sdkmessageid", value.Value);
            currentValue.Id = value.Value;
            this["sdkmessageid"] = currentValue;
        }
    }
    //TODO: MessageName does not exist, instead there is SdkMessageId
    public string? MessageName
    {
        get => TryGetAttributeValue("sdkmessageid", out EntityReference? value) ? value?.Name : null;
        set
        {
            if (value == null)
            {
                this["sdkmessageid"] = null;
                return;
            }
            var currentValue = GetAttributeValue<EntityReference>("sdkmessageid");
            currentValue ??= new EntityReference("sdkmessageid", Guid.Empty);
            currentValue.Name = value;
            this["sdkmessageid"] = currentValue;
        }
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
    public PluginStepStates? State 
    {
        get => TryGetAttributeValue("statecode", out OptionSetValue option) ? (PluginStepStates)option.Value : null;
        set => this["statecode"] = value == null ? null : new OptionSetValue((int)value);
    }
    /// <summary>
    /// Required, Picklist
    /// </summary>
    [AttributeLogicalName("supporteddeployment")]
    public SupportedDeployments? SupportedDeployment 
    { 
        get => TryGetAttributeValue("supporteddeployment", out OptionSetValue option) ? (SupportedDeployments)option.Value : null;
        set => this["supporteddeployment"] = value == null ? null : new OptionSetValue((int)value);
    }
    //TODO: The following field does not exist:
    //[AttributeLogicalName("impersonatinguserfullname")]
    //public string? ImpersonatingUserFullname
    //{
    //    get => TryGetAttributeValue("impersonatinguserfullname", out string value) ? value : null;
    //    set => this["impersonatinguserfullname"] = value;
    //}
    #endregion

    public static LinkEntity LinkWithImages(ColumnSet columns, JoinOperator join = JoinOperator.LeftOuter)
        => new(EntityLogicalName, PluginStepImageConfig.EntityLogicalName, "sdkmessageprocessingstepid", "sdkmessageprocessingstepid", join)
        {
            EntityAlias = PluginStepImageConfig.EntityLogicalName,
            Columns = columns
        };

    public static LinkEntity LinkWithImages(string[] columns, JoinOperator join = JoinOperator.LeftOuter)
        => new(EntityLogicalName, PluginStepImageConfig.EntityLogicalName, "sdkmessageprocessingstepid", "sdkmessageprocessingstepid", join)
        {
            EntityAlias = PluginStepImageConfig.EntityLogicalName,
            Columns = new ColumnSet(columns)
        };

    public static LinkEntity LinkWithImages(
        Expression<Func<PluginStepImageConfig, object>> columnsExpression, JoinOperator join = JoinOperator.LeftOuter)
        => LinkWithImages(PluginStepImageConfig.Select.From(columnsExpression), join);

    public PluginStepConfig() : base(EntityLogicalName) { }
}
#nullable restore