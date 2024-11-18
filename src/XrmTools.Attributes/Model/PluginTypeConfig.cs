﻿#nullable enable
namespace XrmTools.Xrm.Model;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.Json.Serialization;

public interface IPluginTypeEntity
{
    Guid? PluginTypeId { get; set; }
    string? Description { get; set; }
    string? FriendlyName { get; set; }
    string? Name { get; set; }
    string? TypeName { get; set; }
    string? WorkflowActivityGroupName { get; set; }
}

public interface IPluginTypeConfig : IPluginTypeEntity
{
    string? Namespace { get; set; }
    ICollection<PluginStepConfig> Steps { get; set; }
}

[EntityLogicalName(EntityLogicalName)]
public class PluginTypeConfig : TypedEntity<PluginTypeConfig>, IPluginTypeConfig
{
    public const string EntityLogicalName = "plugintype";

    [JsonPropertyName("Id")]
    [JsonProperty("Id")]
    [AttributeLogicalName("plugintypeid")]
    public Guid? PluginTypeId
    {
        get => TryGetAttributeValue("plugintypeid", out Guid value) ? value : null;
        set => this["plugintypeid"] = value;
    }

    [AttributeLogicalName("pluginassemblyid")]
    public EntityReference? PluginAssemblyId
    {
        get => TryGetAttributeValue("pluginassemblyid", out EntityReference value) ? value : null;
        set => this["pluginassemblyid"] = value;
    }
    [AttributeLogicalName("name")]
    public string? Name
    {
        get => TryGetAttributeValue("name", out string? value) ? value : null;
        set => this["name"] = value;
    }
    [AttributeLogicalName("description")]
    public string? Description
    {
        get => TryGetAttributeValue("description", out string value) ? value : null;
        set => this["description"] = value;
    }
    [AttributeLogicalName("friendlyname")]
    public string? FriendlyName
    {
        get => TryGetAttributeValue("friendlyname", out string? value) ? value : null;
        set => this["friendlyname"] = value;
    }
    [AttributeLogicalName("typename")]
    public string? TypeName
    {
        get => TryGetAttributeValue("typename", out string value) ? value : null;
        set => this["typename"] = value;
    }
    [AttributeLogicalName("workflowactivitygroupname")]
    public string? WorkflowActivityGroupName
    {
        get => TryGetAttributeValue("workflowactivitygroupname", out string value) ? value : null;
        set => this["workflowactivitygroupname"] = value;
    }

    [JsonProperty(Order = 1)]
    [JsonPropertyOrder(1)]
    public ICollection<PluginStepConfig> Steps { get; set; } = [];

    public string? Namespace { get; set; }

    public static LinkEntity LinkWithSteps(ColumnSet columns, JoinOperator join = JoinOperator.LeftOuter)
        => new (EntityLogicalName, PluginStepConfig.EntityLogicalName, "plugintypeid", "plugintypeid", join)
        {
            EntityAlias = PluginStepConfig.EntityLogicalName,
            Columns = columns
        };

    public static LinkEntity LinkWithSteps(string[] columns, JoinOperator join = JoinOperator.LeftOuter)
        => new(EntityLogicalName, PluginStepConfig.EntityLogicalName, "plugintypeid", "plugintypeid", join)
        {
            EntityAlias = PluginStepConfig.EntityLogicalName,
            Columns = new ColumnSet(columns)
        };

    public static LinkEntity LinkWithSteps(Expression<Func<PluginStepConfig, object>> columnsExpression, JoinOperator join = JoinOperator.LeftOuter)
        => LinkWithSteps(PluginStepConfig.Select.From(columnsExpression), join);

    public static QueryExpression QueryWithSteps(string[] columns, string[] stepColumns, JoinOperator join)
        => new(PluginStepConfig.EntityLogicalName)
        {
            ColumnSet = new ColumnSet(columns),
            LinkEntities = { LinkWithSteps(stepColumns, join) }
        };

    public static QueryExpression QueryWithSteps(
        Expression<Func<PluginTypeConfig, object>> columnsExpression,
        Expression<Func<PluginStepConfig, object>> stepColumnsExpression,
        JoinOperator join)
        => new(EntityLogicalName)
        {
            ColumnSet = Select.From(columnsExpression),
            LinkEntities = { LinkWithSteps(stepColumnsExpression, join) }
        };

    public PluginTypeConfig() : base(EntityLogicalName) { }
}
#nullable restore