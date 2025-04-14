#nullable enable
namespace XrmTools.Xrm.Model;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.Json.Serialization;
using XrmTools.Meta.Model;

public interface IMessageProcessingStepEntity
{
    Guid? PluginStepId { get; set; }
    Guid? SdkMessageId { get; set; }
    bool AsyncAutoDelete { get; set; }
    string? Description { get; set; }
    string? FilteringAttributes { get; set; }
    int? InvocationSource { get; set; }
    string? MessageName { get; set; }
    string? Name { get; set; }
    string? PrimaryEntityName { get; set; }
    string? Configuration { get; set; }
    ExecutionMode? Mode { get; set; }
    int? Rank { get; set; }
    Stages? Stage { get; set; }
    int? ExecutionOrder { get; set; }
    PluginStepStates? State { get; set; }
    SupportedDeployments? SupportedDeployment { get; set; }
    EntityReference? ImpersonatingUserId { get; set; }
    public string? ImpersonatingUserFullname { get; set; }
    string? WorkflowActivityGroupName { get; set; }
    bool? CanBeBypassed { get; set;}
}

public interface IMessageProcessingStepConfig : IMessageProcessingStepEntity
{
    ICollection<PluginStepImageConfig> Images { get; set; }
    EntityMetadata? PrimaryEntityDefinition { get; set; }
    string? StageName { get; }
    object? ActionDefinition { get; set; }
    SdkMessage? Message { get; set; } 
}

[EntityLogicalName(EntityLogicalName)]
public class PluginStepConfig : TypedEntity<PluginStepConfig>, IMessageProcessingStepConfig
{
    //Fields not present in the entity:
    // - PrimaryEntityName
    // - ImpersonatingUserFullname (ImpersonatingUserId is present)
    // - MessageName (SdkMessageId is present)

    public const string EntityLogicalName = "sdkmessageprocessingstep";
    public override string GetEntitySetName() => "sdkmessageprocessingsteps";

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
    [JsonProperty("sdkmessageprocessingstepid_sdkmessageprocessingstepimage", Order = 1)]
    [JsonPropertyOrder(1)]
    public ICollection<PluginStepImageConfig> Images { get; set; } = [];
    public object? ActionDefinition { get; set; }
    public string? Configuration { get; set; }
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
    public SdkMessage? Message { get; set; }
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
    public ExecutionMode? Mode 
    {
        get => TryGetAttributeValue("mode", out OptionSetValue? value) ? (ExecutionMode?)value?.Value : null;
        set => this["mode"] = value == null ? null : new OptionSetValue((int)value);
    }

    [AttributeLogicalName("executionorder")]
    public int? ExecutionOrder 
    { 
        get => TryGetAttributeValue("executionorder", out int value) ? value : null;
        set => this["executionorder"] = value;
    }
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
        get => TryGetAttributeValue("sdkmessageid", out EntityReference? value) ? value?.Id : null;
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
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    [AttributeLogicalName("impersonatinguserid")]
    public EntityReference? ImpersonatingUserId
    {
        get => TryGetAttributeValue("impersonatinguserid", out EntityReference value) ? value : null;
        set => this["Impersonatinguserid"] = value;
    }
    [AttributeLogicalName("impersonatinguserfullname")]
    public string? ImpersonatingUserFullname
    {
        get => ImpersonatingUserId?.Name;
        set
        {
            var impersonatingUserId = ImpersonatingUserId ?? new EntityReference("systemuser", Guid.Empty);
            impersonatingUserId.Name = value;
            ImpersonatingUserId = impersonatingUserId;
        }
    }
    [AttributeLogicalName("canbebypassed")]
    public bool? CanBeBypassed
    {
        get => TryGetAttributeValue("canbebypassed", out bool value) && value;
        set => this["canbebypassed"] = value;
    }
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