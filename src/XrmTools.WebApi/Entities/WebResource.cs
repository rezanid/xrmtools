#nullable enable
namespace XrmTools.WebApi.Entities;

using Newtonsoft.Json;
using System;
using System.Text.Json.Serialization;
using XrmTools.WebApi.Entities.Attributes;
using XrmTools.WebApi.Types;

[EntityMetadata("webresource", "webresourceset")]
public sealed class WebResource : Component<WebResource>
{
    [JsonPropertyName("webresourceid")]
    [JsonProperty("webresourceid")]
    public override Guid? Id { get; set; }

    public string? Name { get; set; }
    public string? DisplayName { get; set; }
    public string? Description { get; set; }
    public string? Content { get; set; }
    public WebResourceType? WebResourceType { get; set; }
    public int? LanguageCode { get; set; }
}
#nullable restore
