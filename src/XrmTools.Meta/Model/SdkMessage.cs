#nullable enable
namespace XrmTools.Meta.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using XrmTools.WebApi.Entities;
using XrmTools.WebApi.Entities.Attributes;

[EntityMetadata("sdkmessage", "sdkmessages")]
public class SdkMessage : Component<SdkMessage>
{
    [JsonProperty("sdkmessageid"), JsonPropertyName("sdkmessageid")]
    public override Guid? Id { get; set; }

    [JsonProperty("name"), JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonProperty("isprivate"), JsonPropertyName("isprivate")]
    public bool IsPrivate { get; set; }

    [JsonProperty("executeprivilegename"), JsonPropertyName("executeprivilegename")]
    public string? ExecutePrivilegeName { get; set; }

    [JsonProperty("workflowenabled"), JsonPropertyName("workflowenabled")]
    public bool? WorkflowSdkStepEnabled { get; set; }

    [JsonProperty("isvalidforexecuteasync"), JsonPropertyName("isvalidforexecuteasync")]
    public bool? IsValidForExecuteAsync { get; set; }

    [JsonProperty("autotransact"), JsonPropertyName("autotransact")]
    public bool AutoTransact { get; set; }

    [JsonProperty("introducedversion"), JsonPropertyName("introducedversion")]
    public string? IntroducedVersion { get; set; }

    [JsonProperty("customizationleve"), JsonPropertyName("customizationlevel")]
    public int? CustomizationLevel { get; set; }

    [JsonProperty("availability"), JsonPropertyName("availability")]
    public int? Availability { get; set; }

    [JsonProperty("categoryname"), JsonPropertyName("categoryname")]
    public string? CategoryName { get; set; }

    [JsonProperty("sdkmessageid_sdkmessagefilter"), JsonPropertyName("sdkmessageid_sdkmessagefilter")]
    public SdkMessageFilter[] Filters { get; set; } = [];

    [JsonProperty("message_sdkmessagepair"), JsonPropertyName("message_sdkmessagepair")]
    public List<SdkMessagePair> Pairs { get; set; } = [];

    private static readonly IReadOnlyDictionary<string, ImageMessagePropertyName[]> ImageMessageProperties =
        new Dictionary<string, ImageMessagePropertyName[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["Send"] = [new ("EmailId", "Sent Entity Id")],
            ["Merge"] = [
                new ("Target", "Parent Entity", "Entity into which the data from the Child Entity is being merged."),
                new ("SubordinateId", "Child Entity", "Entity that is being merged into the Parent Entity.")
            ],
            ["Route"] = [new ("Target", "Routed Entity", null)],
            ["Assign"] = [new ("Target", "Assigned Entity")],
            ["Create"] = [new ("id", "Created Entity")],
            ["Delete"] = [new ("Target", "Deleted Entity")],
            ["Update"] = [new ("Target", "Updated Entity")],
            ["SetState"] = [new ("EntityMoniker", "Entity")],
            ["CreateMultiple"] = [new ("Ids", "Created Entities")],
            ["DeliverPromote"] = [new ("EmailId", "Delivered E-mail Id")],
            ["UpdateMultiple"] = [new ("Targets", "Updated Entities")],
            ["DeliverIncoming"] = [new ("EmailId", "Delivered E-mail Id")],
            ["ExecuteWorkflow"] = [new ("Target", "Workflow Entity", null)],
            ["SetStateDynamicEntity"] = [new ("EntityMoniker", "Entity")],
            ["OnExternalUpdated"] = []
        };

    internal IReadOnlyList<ImageMessagePropertyName> MessagePropertyNames =>
        ImageMessageProperties.TryGetValue(Name, out var messageNames)
        ? messageNames
        : [];

    internal bool IsFilteringAttributesSupported => Name is "Create" or "Update" or "CreateMultiple" or "CreateMultiple" or "OnExternalUpdated";
}

internal record ImageMessagePropertyName(
    string Name, string ShortDescription, string? Description = null);
#nullable restore
