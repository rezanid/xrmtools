namespace XrmTools.WebApi.Entities;

using Newtonsoft.Json;
using System;
using System.Text.Json.Serialization;
using XrmTools.WebApi.Entities.Attributes;

[EntityMetadata("solution", "solutions")]
public class Solution : Entity<Solution>
{
    [JsonPropertyName("solutionid")]
    [JsonProperty("solutionid")]
    public override Guid? Id { get; set; }
    /// <summary>
    /// ThDisplay: Name
    /// </summary>
    public string? UniqueName { get; set; }
    /// <summary>
    /// Display: Display Name
    /// </summary>
    public string? FriendlyName { get; set; }
    public string? Description { get; set; }
    [JsonPropertyName("publisherid")]
    [JsonProperty("publisherid")]
    public Publisher? Publisher { get; set; }
}
