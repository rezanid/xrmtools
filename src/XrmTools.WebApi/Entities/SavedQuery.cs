#nullable enable
namespace XrmTools.WebApi.Entities;

using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using XrmTools.WebApi.Entities.Attributes;
using XrmTools.WebApi.Types;

[EntityMetadata("savedquery", "savedqueries")]
public class SavedQuery : Component<SavedQuery>
{
    [JsonProperty("savedqueryid"), JsonPropertyName("savedqueryid")]
    public override Guid? Id { get; set; }
    [JsonProperty("name"), JsonPropertyName("name")]
    public required string Name { get; set; }
    public string? Description { get; set; }
    [Description("Type the column name that will be used to group the results from the data collected across multiple records from a system view.")]
    [DisplayName("Advanced Group By")]
    public string? AdvancedGroupBy { get; set; }
    public ManagedBooleanProperty? CanBeDeleted { get; set; }
    [Description("Contains the columns and sorting criteria for the view, stored in XML format.")]
    public string? ColumnSetXml { get; set; }
    public string? ConditionalFormatting { get; set; }
    public bool? EnableCrossPartition { get; set; }
    public string? FetchXml { get; set; }
    public bool? IsCustom { get; set; }
    public ManagedBooleanProperty? IsCustomizable { get; set; }
    [Description("Tells whether the record is part of a managed solution.")]
    public bool? IsDefault { get; set; }
    public bool? IsQuickFindQuery { get; set; }
    [ReadOnly(true)]
    public bool? IsUserDefined { get; set; }
    /// <summary>For internal use only.</summary>
    [ReadOnly(true)]
    public string? LayoutXml { get; set; }
    public string? LayoutJson { get; set; }
    public string? OfflineSqlQuery { get; set; }
    public int? QueryType { get; set; }
    [DisplayName("Entity Name")]
    public string? ReturnedTypeCode { get; set; }
    public string? RoleDisplayConditionsXml { get; set; }
    public SavedQueryStateCodes? StateCode { get; set; }
    public SavedQueryStatsusCodes? StatusCode { get; set; }
}

public enum SavedQueryStateCodes
{
    Active = 0,
    Inactive = 1
}

public enum SavedQueryStatsusCodes
{
    Active = 1,
    Inactive = 2
}

#nullable restore