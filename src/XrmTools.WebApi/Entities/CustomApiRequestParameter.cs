#nullable enable
namespace XrmTools.WebApi.Entities;
using Newtonsoft.Json;
using System;
using System.Text.Json.Serialization;
using XrmTools.WebApi.Entities.Attributes;
using XrmTools.WebApi.Types;

[EntityMetadata("customapirequestparameter", "customapirequestparameters")]
public class CustomApiRequestParameter : Entity<CustomApiRequestParameter>
{
    public string? DisplayName { get; set; }
    public string? Name { get; set; }
    public string? UniqueName { get; set; }
    public string? Description { get; set; }
    public CustomApiFieldType Type { get; set; }
    public string? LogicalEntityName { get; set; }
    public bool IsOptional { get; set; } = false;
    [JsonPropertyName("customapiresponsepropertyid")]
    [JsonProperty("customapiresponsepropertyid")]
    public override Guid? Id { get; set; }
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public string? TypeName { get; set; }
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public string? FullTypeName { get; set; }
}
#nullable restore