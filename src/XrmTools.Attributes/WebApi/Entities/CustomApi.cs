#nullable enable
namespace XrmTools.WebApi.Entities;

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using XrmTools.WebApi.Entities.Attributes;

[EntityMetadata("customapi", "customapis")]
public class CustomApi : Entity<CustomApi>
{
    public enum ProcessingStepTypes
    {
        None = 0,
        AsyncOnly = 1,
        SyncAndAsync = 2,
    }

    public enum BindingTypes
    {
        Global = 0,
        Entity = 1,
        EntityCollection = 2,
    }

    public string? DisplayName { get; set; }
    public string? Name { get; set; }
    public string? UniqueName { get; set; }
    public string? Description { get; set; }
    [JsonPropertyName("allowedcustomprocessingsteptype")]
    [JsonProperty("allowedcustomprocessingsteptype")]
    public ProcessingStepTypes StepType { get; set; } = ProcessingStepTypes.None;
    public BindingTypes BindingType { get; set; } = BindingTypes.Global;
    public string? BoundEntityLogicalName { get; set; }
    public string? ExecutePrivilegeName { get; set; }
    public bool IsFunction { get; set; }
    public bool IsPrivate { get; set; }
    [JsonPropertyName("customapiid")]
    [JsonProperty("customapiid")]
    public override Guid? Id { get; set; }
    //public Guid? SolutionId { get; set; }

    [JsonPropertyName("CustomAPIRequestParameters")]
    [JsonProperty("CustomAPIRequestParameters")]
    public ICollection<CustomApiRequestParameter> RequestParameters { get; set; } = [];
    [JsonPropertyName("CustomAPIResponseProperties")]
    [JsonProperty("CustomAPIResponseProperties")]
    public ICollection<CustomApiResponseProperty> ResponseProperties { get; set; } = [];
}
#nullable restore