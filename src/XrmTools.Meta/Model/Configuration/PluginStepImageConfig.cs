namespace XrmTools.Model.Configuration;

using XrmTools.WebApi.Entities;

public class PluginStepImageConfig : SdkMessageProcessingStepImage//, IMessageProcessingStepImageConfig
{
    public EntityMetadata? MessagePropertyDefinition { get; set; }
}
