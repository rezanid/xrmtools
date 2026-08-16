namespace XrmTools.Model.Configuration;

using XrmTools.WebApi.Entities;

public class PluginStepImageConfig : SdkMessageProcessingStepImage
{
    public EntityMetadata? MessagePropertyDefinition { get; set; }
}
