namespace XrmTools.Meta.Model.Configuration;

using System.Collections.Generic;
using System.Text.Json.Serialization;
using XrmTools.Meta.Attributes;
using XrmTools.Model.Configuration;
using XrmTools.WebApi.Entities;

/// <summary>
/// Expands the SdkMessageProcessingStep to include additional properties used by attributes
/// for plugin registration and code generation.
/// </summary>
public class PluginStepConfig : SdkMessageProcessingStep//, IMessageProcessingStepConfig
{
    public string? PrimaryEntityName { get; set; }

    /// <summary>
    /// Contains the metadata for the primary entity. Attributes are filtered to only include the attributes 
    /// that are used in the message processing step.
    /// </summary>
    public EntityMetadata? PrimaryEntityDefinition { get; set; }

    /// <summary>
    /// This is an alias for <see cref="Rank"/> to match the <see cref="StepAttribute"/> property.
    /// </summary>
    [JsonIgnore]
    public int? ExecutionOrder
    {
        get => Rank;
        set => Rank = value;
    }

    [JsonIgnore]
    public string? MessageName { get; set; }

    [JsonIgnore]
    public PluginStepStates? State
    {
        get => (PluginStepStates?)StateCode;
        set => StateCode = (int?)value;
    }

    public string? StageName => Stage switch
    {
        Stages.PreValidation => "PreValidation",
        Stages.PreOperation => "PreOperation",
        Stages.MainOperation => "MainOperation",
        Stages.PostOperation => "PostOperation",
        null => null,
        _ => $"Unknown stage: {Stage}"
    };

    public new ICollection<PluginStepImageConfig> Images { get; set; } = [];
}