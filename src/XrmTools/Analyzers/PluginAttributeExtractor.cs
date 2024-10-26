#nullable enable
namespace XrmTools.Analyzers;

using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using XrmTools.Helpers;
using XrmTools.Meta.Attributes;
using XrmTools.Meta.Model;
using XrmTools.Xrm.Model;

public interface IAttributeExtractor
{
    IEnumerable<PluginTypeConfig> ExtractAttributes(INamedTypeSymbol typeSymbol);
}

public class AttributeExtractor : IAttributeExtractor
{
    public IEnumerable<PluginTypeConfig> ExtractAttributes(INamedTypeSymbol typeSymbol)
    {
        PluginTypeConfig? lastPluginConfig = null;
        PluginStepConfig? lastStepConfig = null;
        var pluginTypeConfigs = new List<PluginTypeConfig>();

        foreach (var attributeData in typeSymbol.GetAttributes())
        {
            switch (attributeData.AttributeClass?.ToDisplayString())
            {
                case var name when name == typeof(PluginAttribute).FullName:
                    lastPluginConfig = CreatePluginTypeConfig(typeSymbol, attributeData);
                    if (lastPluginConfig != null)
                        pluginTypeConfigs.Add(lastPluginConfig);
                    break;

                case var name when name == typeof(StepAttribute).FullName:
                    if (lastPluginConfig != null)
                    {
                        lastStepConfig = CreatePluginStepConfig(attributeData);
                        if (lastStepConfig != null)
                            lastPluginConfig.Steps.Add(lastStepConfig);
                    }
                    break;

                case var name when name == typeof(ImageAttribute).FullName:
                    if (lastStepConfig != null)
                    {
                        var imageConfig = CreatePluginImageConfig(attributeData);
                        if (imageConfig != null)
                            lastStepConfig.Images.Add(imageConfig);
                    }
                    break;
            }
        }

        return pluginTypeConfigs;
    }

    private PluginTypeConfig CreatePluginTypeConfig(INamedTypeSymbol typeSymbol, AttributeData attributeData)
    {
        var pluginTypeConfig = new PluginTypeConfig
        {
            TypeName = typeSymbol.Name,
            Name = attributeData.GetValue<string>(nameof(PluginAttribute.Name)) ?? typeSymbol.Name,
            FriendlyName = attributeData.GetValue<string>(nameof(PluginAttribute.FriendlyName)) ?? typeSymbol.Name,
            Description = attributeData.GetValue<string>(nameof(PluginAttribute.Description)),
            WorkflowActivityGroupName = attributeData.GetValue<string>(nameof(PluginAttribute.WorkflowActivityGroupName))
        };

        if (attributeData.GetValue<string>(nameof(PluginAttribute.Id)) is string pluginTypeId)
        {
            pluginTypeConfig.PluginTypeId = Guid.Parse(pluginTypeId);
        }

        return pluginTypeConfig;
    }

    private PluginStepConfig CreatePluginStepConfig(AttributeData attributeData)
    {
        const int messageNameIndex = 0;
        const int primaryEntityNameIndex = 1;
        const int filteringAttributesIndex = 2;
        const int stageIndex = 3;
        const int modeIndex = 4;

        var pluginStepConfig = new PluginStepConfig
        {
            MessageName = ((MessageNames)((int)attributeData.ConstructorArguments[messageNameIndex].Value!)).ToString(),
            PrimaryEntityName = attributeData.ConstructorArguments[primaryEntityNameIndex].Value!.ToString(),
            FilteringAttributes = attributeData.ConstructorArguments[filteringAttributesIndex].Value!.ToString(),
            Stage = (Stages)(int)attributeData.ConstructorArguments[stageIndex].Value!,
            Mode = (ExecutionMode)(int)attributeData.ConstructorArguments[modeIndex].Value!
        };

        if (attributeData.GetValue<string?>(nameof(StepAttribute.Id)) is string stepId)
        {
            pluginStepConfig.PluginStepId = Guid.Parse(stepId);
        }

        pluginStepConfig.Rank = attributeData.GetValue<int?>(nameof(StepAttribute.ExecutionOrder)) ?? pluginStepConfig.Rank;
        pluginStepConfig.ImpersonatingUserFullname = attributeData.GetValue<string?>(nameof(StepAttribute.ImpersonatingUserFullname));
        pluginStepConfig.AsyncAutoDelete = attributeData.GetValue<bool?>(nameof(StepAttribute.AsyncAutoDelete)) ?? pluginStepConfig.AsyncAutoDelete;
        pluginStepConfig.Description = attributeData.GetValue<string?>(nameof(StepAttribute.Description));
        pluginStepConfig.CustomConfiguration = attributeData.GetValue<string?>(nameof(StepAttribute.Configuration));
        pluginStepConfig.CanBeBypassed = attributeData.GetValue<bool?>(nameof(StepAttribute.CanBeBypassed)) ?? pluginStepConfig.CanBeBypassed;
        pluginStepConfig.Name = attributeData.GetValue<string?>(nameof(StepAttribute.Name)) ?? pluginStepConfig.Name;
        pluginStepConfig.State = (PluginStepStates?)attributeData.GetValue<int?>(nameof(StepAttribute.State)) ?? pluginStepConfig.State;
        pluginStepConfig.SupportedDeployment = (SupportedDeployments?)attributeData.GetValue<int?>(nameof(StepAttribute.SupportedDeployment)) ?? pluginStepConfig.SupportedDeployment;

        return pluginStepConfig;
    }

    private PluginStepImageConfig CreatePluginImageConfig(AttributeData attributeData)
    {
        const int imageTypeIndex = 0;
        const int attributesIndex = 1;
        const int nameIndex = 2;

        var type = (ImageTypes)(int)attributeData.ConstructorArguments[imageTypeIndex].Value!;
        var attributes = attributeData.ConstructorArguments.Length > attributesIndex ? attributeData.ConstructorArguments[attributesIndex].Value!.ToString()! : string.Empty;
        var name = attributeData.ConstructorArguments.Length > nameIndex ? attributeData.ConstructorArguments[nameIndex].Value!.ToString() : type.ToString();

        var imageConfig = new PluginStepImageConfig
        {
            ImageType = type,
            ImageAttributes = attributes,
            Name = name,
            PluginStepImageId = attributeData.GetValue<string?>(nameof(ImageAttribute.Id)) is string imageId ? Guid.Parse(imageId) : default,
            Description = attributeData.GetValue<string?>(nameof(ImageAttribute.Description)),
            EntityAlias = attributeData.GetValue<string?>(nameof(ImageAttribute.EntityAlias)),
            MessagePropertyName = attributeData.GetValue<string?>(nameof(ImageAttribute.MessagePropertyName))
        };

        return imageConfig;
    }
}