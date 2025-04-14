﻿#nullable enable
namespace XrmTools.Analyzers;

using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using XrmTools.Helpers;
using XrmTools.Meta.Attributes;
using XrmTools.Meta.Model;
using XrmTools.WebApi;
using XrmTools.WebApi.Messages;
using XrmTools.Xrm.Model;

internal interface ICSharpXrmMetaParser
{
    IEnumerable<EntityConfig> ParseEntitieConfigsFrom(IEnumerable<AttributeData> entityAttributes);

    PluginTypeConfig? ParsePluginConfig(INamedTypeSymbol pluginType);

    PluginAssemblyConfig? ParsePluginAssemblyConfig(IAssemblySymbol assembly);
}

[Export(typeof(ICSharpXrmMetaParser))]
internal class CSharpXrmMetaParser : ICSharpXrmMetaParser
{
    private static readonly Dictionary<string, WebApi.Types.CustomApiFieldType> CustomApiFieldTypeMapping = new()
    {
        ["bool"] = WebApi.Types.CustomApiFieldType.Boolean,
        ["DateTime"] = WebApi.Types.CustomApiFieldType.DateTime,
        ["decimal"] = WebApi.Types.CustomApiFieldType.Decimal,
        ["Entity"] = WebApi.Types.CustomApiFieldType.Entity,
        ["EntityCollection"] = WebApi.Types.CustomApiFieldType.EntityCollection,
        ["EntityReference"] = WebApi.Types.CustomApiFieldType.EntityReference,
        ["float"] = WebApi.Types.CustomApiFieldType.Float,
        ["int"] = WebApi.Types.CustomApiFieldType.Integer, 
        ["Money"] = WebApi.Types.CustomApiFieldType.Money,
        ["OptionSetValue"] = WebApi.Types.CustomApiFieldType.Picklist,
        ["string"] = WebApi.Types.CustomApiFieldType.String,
        ["string[]"] = WebApi.Types.CustomApiFieldType.StringArray,
        ["Guid"] = WebApi.Types.CustomApiFieldType.Guid
    };

    public PluginAssemblyConfig? ParsePluginAssemblyConfig(IAssemblySymbol assemblySymbol)
    {
        AttributeData? assemblyAttribute = null;
        AttributeData? solutionAttribute = null;
        foreach (var attr in assemblySymbol.GetAttributes())
        {
            if (attr.AttributeClass?.ToDisplayString() == typeof(PluginAssemblyAttribute).FullName)
            {
                assemblyAttribute = attr;
            }
            else if (attr.AttributeClass?.ToDisplayString() == typeof(SolutionAttribute).FullName)
            {
                solutionAttribute = attr;
            }
        }

        if (assemblyAttribute == null) return null;

        var pluginAssemblyConfig = new PluginAssemblyConfig
        {
            Name = assemblySymbol.Name,
            Version = assemblySymbol.Identity.Version.ToString(),
            PublicKeyToken = assemblySymbol.Identity.PublicKeyToken.ToHexString(),
            SourceType = assemblyAttribute.GetValue<int?>(nameof(PluginAssemblyAttribute.SourceType)) is int sourceType
                ? (SourceTypes)sourceType : PluginAssemblyAttribute.DefaultSourceType,
            IsolationMode = assemblyAttribute.GetValue<int?>(nameof(PluginAssemblyAttribute.IsolationMode)) is int isolationMode
                ? (IsolationModes)isolationMode : PluginAssemblyAttribute.DefaultIsolationMode,
        };

        if (solutionAttribute != null)
        {
            var solution = ReflectionHelper.SetPropertiesFromAttribute(new WebApi.Entities.Solution(), solutionAttribute);
            pluginAssemblyConfig.Solution = solution;

        }

        return pluginAssemblyConfig.SetPropertiesFromAttribute(assemblyAttribute);
    }

    public IEnumerable<EntityConfig> ParseEntitieConfigsFrom(IEnumerable<AttributeData> entityAttributes)
    {
        const int entityNameIndex = 0;
        foreach (var attribute in entityAttributes)
        {
            yield return new EntityConfig
            {
                Attributes = attribute.GetValue<string>(nameof(EntityAttribute.AttributeNames)),
                LogicalName = attribute.ConstructorArguments[entityNameIndex].Value!.ToString(),
            };
        }
    }

    public PluginTypeConfig? ParsePluginConfig(INamedTypeSymbol typeSymbol)
    {
        WebApi.Entities.CustomApi? customApi = null;
        PluginTypeConfig? pluginConfig = null;
        PluginStepConfig? lastStepConfig = null;

        foreach (var attributeData in typeSymbol.GetAttributes())
        {
            switch (attributeData.AttributeClass?.ToDisplayString())
            {
                case var name when name == typeof(CustomApiAttribute).FullName:
                    customApi = new WebApi.Entities.CustomApi();
                    customApi.SetPropertiesFromAttribute(attributeData);

                    if (string.IsNullOrWhiteSpace(customApi.DisplayName) && string.IsNullOrWhiteSpace(customApi.Name) && string.IsNullOrWhiteSpace(customApi.UniqueName))
                    {
                        customApi.DisplayName = customApi.Name = customApi.UniqueName = typeSymbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);
                    }
                    else if (string.IsNullOrWhiteSpace(customApi.Name) && string.IsNullOrWhiteSpace(customApi.UniqueName))
                    {
                        customApi.Name = customApi.UniqueName = customApi.DisplayName!.Replace(" ", "");
                    }
                    else if (string.IsNullOrWhiteSpace(customApi.UniqueName))
                    {
                        customApi.UniqueName = customApi.Name;
                    }

                    break;
                case var name when name == typeof(PluginAttribute).FullName:
                    pluginConfig = CreatePluginTypeConfig(typeSymbol, attributeData);
                    if (pluginConfig == null) return null;
                    break;

                case var name when name == typeof(StepAttribute).FullName:
                    if (pluginConfig != null)
                    {
                        lastStepConfig = CreatePluginStepConfig(attributeData);
                        if (lastStepConfig != null)
                        {
                            if (string.IsNullOrWhiteSpace(lastStepConfig.Name)) 
                            {
                                lastStepConfig.Name = 
                                    string.IsNullOrWhiteSpace(lastStepConfig.PrimaryEntityName) ?
                                    $"{pluginConfig.Name} : {lastStepConfig.StageName} {lastStepConfig.MessageName}" :
                                    $"{pluginConfig.Name} : {lastStepConfig.StageName} {lastStepConfig.MessageName} of {lastStepConfig.PrimaryEntityName}";
                            }
                            lastStepConfig.ExecutionOrder ??= 1;
                            pluginConfig.Steps.Add(lastStepConfig);
                        }
                    }
                    break;

                case var name when name == typeof(ImageAttribute).FullName:
                    if (lastStepConfig != null)
                    {
                        var imageConfig = CreatePluginImageConfig(attributeData);
                        if (imageConfig != null)
                        {
                            if (string.IsNullOrWhiteSpace(imageConfig.Name))
                            {
                                imageConfig.Name = lastStepConfig.PrimaryEntityName
                                    + (imageConfig.ImageType.HasValue ? Enum.GetName(typeof(ImageTypes), imageConfig.ImageType) : "");
                            }
                            if (string.IsNullOrWhiteSpace(imageConfig.EntityAlias))
                            {
                                imageConfig.EntityAlias = imageConfig.Name;
                            }
                            lastStepConfig.Images.Add(imageConfig);
                        }
                    }
                    break;
            }
        }

        if (pluginConfig != null && customApi != null)
        {
            // typeSymbol is a class that can potentially contain a class with CustomApiRequestAttribute.
            // If that's so, extract all attributes from the class and add them to the customApi's RequestParameters.
            foreach (var innerSymbol in typeSymbol.GetTypeMembers())
            {
                var requestAttribute = innerSymbol.GetAttributes().FirstOrDefault(x => x.AttributeClass?.ToDisplayString() == typeof(CustomApiRequestAttribute).FullName);
                if (requestAttribute != null)
                {
                    foreach (var member in innerSymbol.GetMembers())
                    {
                        // we are only interested in properties.
                        if (member is IPropertySymbol innerProperty)
                        {
                            var requestParameter = new WebApi.Entities.CustomApiRequestParameter()
                            {
                                Name = innerProperty.Name,
                                UniqueName = innerProperty.Name,
                                DisplayName = innerProperty.Name,
                                Type = CustomApiFieldTypeMapping.TryGetValue(innerProperty.Type.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat), out var fieldType) ? fieldType : innerProperty.Type.TypeKind == TypeKind.Enum ? WebApi.Types.CustomApiFieldType.Picklist : WebApi.Types.CustomApiFieldType.String,
                                TypeName = innerProperty.Type.Name,
                                FullTypeName = innerProperty.Type.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
                            };
                            customApi.RequestParameters.Add(requestParameter);
                        }
                    }
                }

                var responseAttribute = innerSymbol.GetAttributes().FirstOrDefault(x => x.AttributeClass?.ToDisplayString() == typeof(CustomApiResponseAttribute).FullName);
                if (responseAttribute != null)
                {
                    foreach (var member in innerSymbol.GetMembers())
                    {
                        // we are only interested in properties.
                        if (member is IPropertySymbol innerProperty)
                        {
                            var responseProperty = new WebApi.Entities.CustomApiResponseProperty()
                            {
                                Name = innerProperty.Name,
                                UniqueName = innerProperty.Name,
                                DisplayName = innerProperty.Name,
                                Type = CustomApiFieldTypeMapping.TryGetValue(innerProperty.Type.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat), out var fieldType) ? fieldType : innerProperty.Type.TypeKind == TypeKind.Enum ? WebApi.Types.CustomApiFieldType.Picklist : WebApi.Types.CustomApiFieldType.String,
                                TypeName = innerProperty.Type.Name,
                                FullTypeName = innerProperty.Type.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
                            };
                            customApi.ResponseProperties.Add(responseProperty);
                        }
                    }
                }
            }
            pluginConfig.CustomApi = customApi;
        }

        return pluginConfig;
    }

    private PluginTypeConfig CreatePluginTypeConfig(INamedTypeSymbol typeSymbol, AttributeData attributeData)
    {
        var typeName = typeSymbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);
        var plugin = new PluginTypeConfig()
        {
            Namespace = typeSymbol.ContainingNamespace.ToDisplayString(),
            BaseTypeName = typeSymbol.BaseType?.Name,
            TypeName = typeName,
            // Default values:
            Name = typeName,
            FriendlyName = typeName
        };
        return plugin.SetPropertiesFromAttribute(attributeData);
    }

    private PluginStepConfig CreatePluginStepConfig(AttributeData attributeData)
        => ReflectionHelper.SetPropertiesFromAttribute(new PluginStepConfig(), attributeData);

    private PluginStepImageConfig CreatePluginImageConfig(AttributeData attributeData)
        => ReflectionHelper.SetPropertiesFromAttribute(new PluginStepImageConfig(), attributeData);
}
#nullable restore