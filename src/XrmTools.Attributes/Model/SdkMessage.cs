#nullable enable
namespace XrmTools.Meta.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

public record SdkMessage(
    [property: JsonProperty("name"), JsonPropertyName("name")]
        string Name,
    [property: JsonProperty("isprivate"), JsonPropertyName("isprivate")]
        bool IsPrivate,
    [property: JsonProperty("executeprivilegename"), JsonPropertyName("executeprivilegename")]
        string ExecutePrivilegeName,
    [property: JsonProperty("isvalidforexecuteasync"), JsonPropertyName("isvalidforexecuteasync")]
        bool? IsValidForExecuteAsync,
    [property: JsonProperty("autotransact"), JsonPropertyName("autotransact")]
        bool AutoTransact,
    [property: JsonProperty("introducedversion"), JsonPropertyName("introducedversion")]
        string IntroducedVersion,
    [property: JsonProperty("sdkmessageid"), JsonPropertyName("sdkmessageid")]
        Guid Id,
    [property: JsonProperty("sdkmessageid_sdkmessagefilter"), JsonPropertyName("sdkmessageid_sdkmessagefilter")]
        SdkMessageFilter[] Filters)
{
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
}

internal record ImageMessagePropertyName(
    string Name, string ShortDescription, string? Description = null);
#nullable restore
