namespace XrmTools.WebApi.Types;

using System;
using System.Text.Json.Serialization;

public sealed class AssociatedMenuConfiguration
{
    public AssociatedMenuBehavior? Behavior { get; set; }

    public AssociatedMenuGroup? Group { get; set; }

    public Label? Label { get; set; }

    public int? Order { get; set; }

    [JsonPropertyOrder(90)]
    public bool IsCustomizable { get; internal set; }

    [JsonPropertyOrder(90)]
    public string Icon { get; internal set; }

    [JsonPropertyOrder(90)]
    public Guid? ViewId { get; internal set; }

    [JsonPropertyOrder(90)]
    public bool? AvailableOffline { get; internal set; }

    [JsonPropertyOrder(90)]
    public string? MenuId { get; internal set; }

    [JsonPropertyOrder(90)]
    public string? QueryApi { get; internal set; }
}
