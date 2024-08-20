#nullable enable
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace XrmGen.Xrm.Model;

public interface IPluginTypeEntity
{
    string? Description { get; set; }
    string? FriendlyName { get; set; }
    string? Name { get; set; }
    string? TypeName { get; set; }
    string? WorkflowActivityGroupName { get; set; }
}

public interface IPluginTypeConfig : IPluginTypeEntity
{
    ICollection<PluginStepConfig> Steps { get; set; }
}

[EntityLogicalName(EntityLogicalName)]
public class PluginTypeConfig : TypedEntity<PluginTypeConfig>, IPluginTypeConfig
{
    public const string EntityLogicalName = "plugintype";

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

    public ICollection<PluginStepConfig> Steps { get; set; } = [];

    public static LinkEntity LinkWithSteps(string[] columns, JoinOperator join)
        => new("plugintype", "sdkmessageprocessingstep", "plugintypeid", "plugintypeid", join)
        {
            EntityAlias = "steps",
            Columns = new ColumnSet(columns)
        };

    public static QueryExpression QueryWithSteps(string[] columns, string[] stepColumns, JoinOperator join)
        => new("plugintype")
        {
            ColumnSet = new ColumnSet(columns),
            LinkEntities = { LinkWithSteps(stepColumns, join) }
        };

    public static LinkEntity LinkWithSteps(Expression<Func<PluginTypeConfig, object>> columnsExpression, JoinOperator join)
    {
        var columns = GetColumnsFromExpression(columnsExpression);
        return new LinkEntity("plugintype", "sdkmessageprocessingstep", "plugintypeid", "plugintypeid", join)
        {
            EntityAlias = "steps",
            Columns = new ColumnSet(columns)
        };
    }

    public static QueryExpression QueryWithSteps(
        Expression<Func<PluginTypeConfig, object>> columnsExpression,
        Expression<Func<PluginStepConfig, object>> stepColumnsExpression,
        JoinOperator join)
    {
        var columns = GetColumnsFromExpression(columnsExpression);
        var stepColumns = PluginStepConfig.GetColumnsFromExpression(stepColumnsExpression);
        return new QueryExpression("plugintype")
        {
            ColumnSet = new ColumnSet(columns),
            LinkEntities = { LinkWithSteps(stepColumns, join) }
        };
    }

    public PluginTypeConfig() : base(EntityLogicalName) { }
}
#nullable restore