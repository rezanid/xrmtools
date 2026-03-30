#nullable enable
namespace XrmTools.WebApi.Entities;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using XrmTools.WebApi.Entities.Attributes;
using XrmTools.WebApi.Types;

[EntityMetadata("systemform", "systemforms")]
public class SystemForm : Entity<SystemForm>
{
    // This entity doesn't have 'createdon' or 'modifiedon' fields.

    [JsonProperty("formid"), JsonPropertyName("formid")]
    public override Guid? Id { get; set; }
    [JsonProperty("name"), JsonPropertyName("name")]
    public required string Name { get; set; }
    public string? Description { get; set; }
    [DisplayName("Can Be Deleted")]
    public ManagedBooleanProperty? CanBeDeleted { get; set; }
    [ReadOnly(true)]
    [DisplayName("Component State")]
    /// <summary>For internal use only.</summary>
    public ComponentState? ComponentState { get; set; }
    [DisplayName("Form State")]
    public FormActivationState? FormActivationState { get; set; }
    /// <summary>
    /// Json representation of the form layout.
    /// </summary>
    public string? FormJson { get; set; }
    /// <summary>
    /// Specifies whether this form is in the updated UI layout in Microsoft Dynamics CRM 2015 or Microsoft Dynamics CRM Online 2015 Update.
    /// </summary>
    [DisplayName("AIR Refreshed")]
    public FormPresentation? FormPresentation { get; set; }
    /// <summary>
    /// XML representation of the form layout.
    /// </summary>
    [ReadOnly(true)]
    public string? FormXml { get; set; }
    [DisplayName("Customizable")]
    public ManagedBooleanProperty? IsCustomizable { get; set; }
    /// <summary>
    /// Information that specifies whether the dashboard is enabled for desktop.
    /// </summary>
    [DisplayName("Default Form")]
    public bool? IsDefault { get; set; }
    [DisplayName("Is Desktop Enabled")]
    public bool? IsDeskOpenEnabled { get; set; }
    [DisplayName("State")]
    public bool? IsManaged { get; set; }
    [DisplayName("Is Tabled Enabled")]
    public bool? IsTabletEnabled { get; set; }
    [DisplayName("Entity Name")]
    public string? ObjectTypeCode { get; set; }
    public DateTimeOffset? PublishedOn { get; set; }
    public Guid? SolutionId { get; set; }
    [DisplayName("Form Type")]
    public FormViewType? Type { get; set; }
    public string? UniqueName { get; set; }
}

[Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
public enum FormViewType
{
    Dashboard = 0,
    AppointmentBook = 1,
    Main = 2,
    MiniCampaignBO = 3,
    Preview = 4,
    Mobile_Express = 5,
    QuickViewForm = 6,
    QuickCreate = 7,
    Dialog = 8,
    TaskFlowForm = 9,
    InteractionCentricDashboard = 10,
    Card = 11,
    Main_InteractiveExperience = 12,
    ContextualDashboard = 13,
    Other = 100,
    MainBackup = 101,
    AppointmentBookBackup = 102,
    PowerBI_Dashboard = 103,
}

[Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
public enum FormActivationState
{
    Inactive = 0,
    Active = 1
}

[Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
public enum FormPresentation
{
    ClassicForm = 0,
    AirForm = 1,
    ConvertedICForm = 2
}

#nullable restore
