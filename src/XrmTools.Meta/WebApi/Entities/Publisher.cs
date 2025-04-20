namespace XrmTools.WebApi.Entities;

using Newtonsoft.Json;
using System;
using System.Text.Json.Serialization;
using XrmTools.WebApi.Entities.Attributes;

[EntityMetadata("publisher", "publishers")]
public class Publisher : Entity<Publisher>
{
    [JsonPropertyName("publisherid")]
    [JsonProperty("publisherid")]
    public override Guid? Id { get; set; }
    public string? CustomizationPrefix { get; set; }
    /// <summary>
    /// Display: Name
    /// </summary>
    public string? UniqueName { get; set; }
    /// <summary>
    /// Display: Display Name
    /// </summary>
    public string? FriendlyName { get; set; }
    public bool? IsReadOnly { get; set; }
    public int? CustomizationOptionValuePrefix { get; set; }
    public EntityReference? OrganizationId { get; set; }
}
