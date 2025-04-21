namespace XrmTools.Meta.Model;
using Newtonsoft.Json;
using System;
using System.Text.Json.Serialization;

public record SdkMessageFilter(
    [property: JsonProperty("primaryobjecttypecode"), JsonPropertyName("primaryobjecttypecode")] 
    string PrimaryObjectTypeCode,
    [property: JsonProperty("sdkmessagefilterid"), JsonPropertyName("sdkmessagefilterid")] 
    Guid Id,
    [property: JsonProperty("iscustomprocessingstepallowed"), JsonPropertyName("iscustomprocessingstepallowed")] 
    bool IsCustomProcessingStepAllowed,
    [property: JsonProperty("sdkmessageid"), JsonPropertyName("sdkmessageid")] 
    Guid SdkMessageId);