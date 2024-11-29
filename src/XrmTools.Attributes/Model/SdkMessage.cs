namespace XrmTools.Meta.Model;
using Newtonsoft.Json;
using System;
using System.Text.Json.Serialization;

public class SdkMessage
{
    [JsonProperty("name"), JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonProperty("isprivate"), JsonPropertyName("isprivate")]
    public bool IsPrivate { get; set; }
    [JsonProperty("executeprivilegename"), JsonPropertyName("executeprivilegename")]
    public string ExecutePrivilegeName { get; set; }
    [JsonProperty("isvalidforexecuteasync"), JsonPropertyName("isvalidforexecuteasync")]
    public bool? IsValidForExecuteAsync { get; set; }
    [JsonProperty("autotransact"), JsonPropertyName("autotransact")]
    public bool AutoTransact { get; set; }
    [JsonProperty("introducedversion"), JsonPropertyName("introducedversion")]
    public string IntroducedVersion { get; set; }
    [JsonProperty("sdkmessageid"), JsonPropertyName("sdkmessageid")]
    public Guid Id { get; set; }
}