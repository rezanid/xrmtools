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
    PluginTypeConfig? ExtractAttributes(INamedTypeSymbol typeSymbol);
}

public class AttributeExtractor : IAttributeExtractor
{
    public PluginTypeConfig? ExtractAttributes(INamedTypeSymbol typeSymbol)
    {
        PluginTypeConfig? pluginConfig = null;
        PluginStepConfig? lastStepConfig = null;

        foreach (var attributeData in typeSymbol.GetAttributes())
        {
            switch (attributeData.AttributeClass?.ToDisplayString())
            {
                case var name when name == typeof(PluginAttribute).FullName:
                    pluginConfig = CreatePluginTypeConfig(typeSymbol, attributeData);
                    if (pluginConfig == null) return null;
                    break;

                case var name when name == typeof(StepAttribute).FullName:
                    if (pluginConfig != null)
                    {
                        lastStepConfig = CreatePluginStepConfig(attributeData);
                        if (lastStepConfig != null)
                            pluginConfig.Steps.Add(lastStepConfig);
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
        return pluginConfig;
    }

    private PluginTypeConfig CreatePluginTypeConfig(INamedTypeSymbol typeSymbol, AttributeData attributeData)
    {
        var pluginTypeConfig = new PluginTypeConfig
        {
            Namespace = typeSymbol.ContainingNamespace.ToDisplayString(),
            TypeName = typeSymbol.Name,
            Name = attributeData.GetValue<string>(nameof(PluginAttribute.Name)) ?? typeSymbol.Name,
            FriendlyName = attributeData.GetValue<string>(nameof(PluginAttribute.FriendlyName)) ?? typeSymbol.Name,
            Description = attributeData.GetValue<string>(nameof(PluginAttribute.Description)),
            WorkflowActivityGroupName = attributeData.GetValue<string>(nameof(PluginAttribute.WorkflowActivityGroupName)),
            PluginTypeId = attributeData.GetValue<string>(nameof(PluginAttribute.Id)) is string pluginTypeId
                ? Guid.Parse(pluginTypeId) : default
        };

        return pluginTypeConfig;
    }

    private PluginStepConfig CreatePluginStepConfig(AttributeData attributeData)
    {
        const int primaryEntityNameIndex = 0;
        const int messageNameIndex = 1;
        const int filteringAttributesIndex = 2;
        const int stageIndex = 3;
        const int modeIndex = 4;
        var argumentCount = attributeData.ConstructorArguments.Length;

        var pluginStepConfig = new PluginStepConfig
        {
            PrimaryEntityName = argumentCount > primaryEntityNameIndex ? attributeData.ConstructorArguments[primaryEntityNameIndex].Value!.ToString() : null,
            MessageName = argumentCount > messageNameIndex ? attributeData.ConstructorArguments[messageNameIndex].Value!.ToString() : null,
            FilteringAttributes = argumentCount > filteringAttributesIndex ? attributeData.ConstructorArguments[filteringAttributesIndex].Value!.ToString() : null,
            Stage = argumentCount > stageIndex ? (Stages)(int)attributeData.ConstructorArguments[stageIndex].Value! : null,
            Mode = argumentCount > modeIndex ? (ExecutionMode)(int)attributeData.ConstructorArguments[modeIndex].Value! : null
        };
        pluginStepConfig.Rank = attributeData.GetValue<int?>(nameof(StepAttribute.ExecutionOrder)) ?? pluginStepConfig.Rank;
        pluginStepConfig.PluginStepId = attributeData.GetValue<string?>(nameof(StepAttribute.Id)) is string stepId
            ? Guid.Parse(stepId) : default;
        pluginStepConfig.ImpersonatingUserFullname = attributeData.GetValue<string?>(nameof(StepAttribute.ImpersonatingUserFullname));
        pluginStepConfig.AsyncAutoDelete = attributeData.GetValue<bool?>(nameof(StepAttribute.AsyncAutoDelete)) 
            ?? pluginStepConfig.AsyncAutoDelete;
        pluginStepConfig.Description = attributeData.GetValue<string?>(nameof(StepAttribute.Description));
        pluginStepConfig.CustomConfiguration = attributeData.GetValue<string?>(nameof(StepAttribute.Configuration));
        pluginStepConfig.CanBeBypassed = attributeData.GetValue<bool?>(nameof(StepAttribute.CanBeBypassed)) 
            ?? pluginStepConfig.CanBeBypassed;
        pluginStepConfig.Name = attributeData.GetValue<string?>(nameof(StepAttribute.Name)) ?? pluginStepConfig.Name;
        pluginStepConfig.State = (PluginStepStates?)attributeData.GetValue<int?>(nameof(StepAttribute.State)) 
            ?? pluginStepConfig.State;
        pluginStepConfig.SupportedDeployment = (SupportedDeployments?)attributeData.GetValue<int?>(nameof(StepAttribute.SupportedDeployment)) 
            ?? pluginStepConfig.SupportedDeployment;
        return pluginStepConfig;
    }

    private PluginStepImageConfig CreatePluginImageConfig(AttributeData attributeData)
    {
        const int imageTypeIndex = 0;
        const int messagePropertyNameIndex = 1;
        const int attributesIndex = 2;

        var argumentCount = attributeData.ConstructorArguments.Length;
        var imageConfig = new PluginStepImageConfig
        {
            ImageType = argumentCount > imageTypeIndex ? (ImageTypes)(int)attributeData.ConstructorArguments[imageTypeIndex].Value! : null,
            MessagePropertyName = argumentCount > messagePropertyNameIndex ? attributeData.ConstructorArguments[messagePropertyNameIndex].Value!.ToString() : null,
            ImageAttributes = argumentCount > attributesIndex ? attributeData.ConstructorArguments[attributesIndex].Value!.ToString() : null,
            PluginStepImageId = attributeData.GetValue<string?>(nameof(ImageAttribute.Id)) is string imageId ? Guid.Parse(imageId) : null,
            Description = attributeData.GetValue<string?>(nameof(ImageAttribute.Description))
        };
        imageConfig.Name = attributeData.GetValue<string?>(nameof(ImageAttribute.Name)) ?? imageConfig.ImageType?.ToString();
        imageConfig.EntityAlias = attributeData.GetValue<string?>(nameof(ImageAttribute.EntityAlias)) ?? imageConfig.ImageType?.ToString();
        return imageConfig;
    }
}