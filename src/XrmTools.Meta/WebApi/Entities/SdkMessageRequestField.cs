﻿namespace XrmTools.WebApi.Entities;

using Newtonsoft.Json;
using System;
using System.Text.Json.Serialization;
using XrmTools.WebApi.Entities.Attributes;

[EntityMetadata("sdkmessagerequestfield", "sdkmessagerequestfields")]
public class SdkMessageRequestField : Component<SdkMessageRequestField>
{
    [JsonPropertyName("sdkmessagerequestfieldid")]
    [JsonProperty("sdkmessagerequestfieldid")]
    public override Guid? Id { get; set; }

    public string? ClrParser { get; set; }
    public int? CustomizationLevel { get; set; } = 0;
    public int? FieldMask { get; set; }
    public string? Name { get; set; }
    public bool? Optional { get; set; }
    public string? ParameterBindingInformation { get; set; }
    public string? Parser { get; set; }
    public int Position { get; set; } = 0;
    public string? PublicName { get; set; }

}
