#nullable enable
namespace XrmTools.Analyzers;

using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Language.Prediction;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using XrmTools.Helpers;
using XrmTools.Meta.Attributes;
using XrmTools.Meta.Model;
using XrmTools.Xrm.Model;

internal interface ICSharpXrmMetaParser
{
    IEnumerable<EntityConfig> ParseEntitiyConfigs(IEnumerable<AttributeData> entityAttributes);

    PluginTypeConfig? ParsePluginConfig(INamedTypeSymbol pluginType, Compilation compilation);

    PluginAssemblyConfig? ParsePluginAssemblyConfig(Compilation compilation);
    EntityConfig ParseEntityConfig(AttributeData entityAttribute);
}

[Export(typeof(ICSharpXrmMetaParser))]
[method: ImportingConstructor]
internal class CSharpXrmMetaParser(
    ICSharpDependencyAnalyzer dependencyAnalyzer,
    IDependencyPreparation dependencyPreparation) : ICSharpXrmMetaParser
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

    public PluginAssemblyConfig? ParsePluginAssemblyConfig(Compilation compilation)
    {
        IAssemblySymbol assemblySymbol = compilation.Assembly;
        AttributeData? assemblyAttribute = null;
        AttributeData? solutionAttribute = null;
        AttributeData? codeGenPrefixAttribute = null;
        //List<EntityConfig> entityConfigs = [];
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
            else if (attr.AttributeClass?.ToDisplayString() == typeof(CodeGenReplacePrefixesAttribute).FullName)
            {
                codeGenPrefixAttribute = attr;
            }
            else
            {
                // Not interested in other attributes.
            }
        }

        var pluginAssemblyConfig = new PluginAssemblyConfig
        {
            Name = assemblySymbol.Name,
            Version = assemblySymbol.Identity.Version.ToString(),
            PublicKeyToken = assemblySymbol.Identity.PublicKeyToken.ToHexString(),
            SourceType = assemblyAttribute?.GetValue<int?>(nameof(PluginAssemblyAttribute.SourceType)) is int sourceType
                ? (SourceTypes)sourceType : PluginAssemblyAttribute.DefaultSourceType,
            IsolationMode = assemblyAttribute?.GetValue<int?>(nameof(PluginAssemblyAttribute.IsolationMode)) is int isolationMode
                ? (IsolationModes)isolationMode : PluginAssemblyAttribute.DefaultIsolationMode,
        };

        if (solutionAttribute != null)
        {
            var solution = ReflectionHelper.SetPropertiesFromAttribute(new WebApi.Entities.Solution(), solutionAttribute);
            pluginAssemblyConfig.Solution = solution;
        }

        if (codeGenPrefixAttribute != null)
        {
            pluginAssemblyConfig.ReplacePrefixes.SetPropertiesFromAttribute(codeGenPrefixAttribute);
        }

        if (assemblyAttribute == null) return pluginAssemblyConfig;

        return pluginAssemblyConfig.SetPropertiesFromAttribute(assemblyAttribute);
    }

    public IEnumerable<EntityConfig> ParseEntitiyConfigs(IEnumerable<AttributeData> entityAttributes)
    {
        foreach (var attribute in entityAttributes)
        {
            if (attribute.AttributeClass?.ToDisplayString() != typeof(EntityAttribute).FullName)
                continue;
            yield return ParseEntityConfig(attribute);
        }
    }

    public EntityConfig ParseEntityConfig(AttributeData entityAttribute)
        => new EntityConfig().SetPropertiesFromAttribute(entityAttribute);

    public PluginTypeConfig? ParsePluginConfig(INamedTypeSymbol typeSymbol, Compilation compilation)
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
                    else
                    {
                        if (string.IsNullOrWhiteSpace(customApi.DisplayName))
                        {
                            customApi.DisplayName = customApi.Name ?? customApi.UniqueName;
                        }
                        if (string.IsNullOrWhiteSpace(customApi.Name))
                        {
                            customApi.Name = customApi.DisplayName is string displayName ? displayName.Replace(" ", "") : customApi.UniqueName;
                        }
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
                    customApi.RequestTypeName = innerSymbol.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat);
                    foreach (var member in innerSymbol.GetMembers())
                    {
                        // we are only interested in properties.
                        if (member is IPropertySymbol innerProperty && innerProperty.DeclaredAccessibility is Accessibility.Public or Accessibility.Internal or Accessibility.ProtectedAndInternal)
                        {
                            var requestParameterAttr = innerProperty.GetAttributes()
                                .FirstOrDefault(x => x.AttributeClass?.ToDisplayString() == typeof(CustomApiRequestParameterAttribute).FullName);

                            var requestParameter = new WebApi.Entities.CustomApiRequestParameter()
                            {
                                Name = innerProperty.Name,
                                UniqueName = innerProperty.Name,
                                DisplayName = innerProperty.Name,
                                Type = CustomApiFieldTypeMapping.TryGetValue(innerProperty.Type.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat).TrimEnd('?'), out var fieldType)
                                    ? fieldType 
                                    : innerProperty.Type.TypeKind == TypeKind.Enum 
                                        ? WebApi.Types.CustomApiFieldType.Picklist 
                                        : WebApi.Types.CustomApiFieldType.String,
                                TypeName = innerProperty.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
                                FullTypeName = innerProperty.Type.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
                                IsOptional = innerProperty.Type.NullableAnnotation == NullableAnnotation.Annotated
                            };

                            // If the propety has an attribute, values from the attribute will override the default values.
                            if (requestParameterAttr != null)
                            {
                                foreach (var namedArg in requestParameterAttr.NamedArguments)
                                {
                                    switch (namedArg.Key)
                                    {
                                        case nameof(CustomApiRequestParameterAttribute.IsOptional) when namedArg.Value.Value is bool isOptional:
                                            requestParameter.IsOptional = requestParameter.IsOptional || isOptional;
                                            break;
                                        case nameof(CustomApiRequestParameterAttribute.UniqueName) when namedArg.Value.Value is string uniqueName:
                                            requestParameter.UniqueName = uniqueName;
                                            break;
                                        case nameof(CustomApiRequestParameterAttribute.DisplayName) when namedArg.Value.Value is string displayName:
                                            requestParameter.DisplayName = displayName;
                                            break;
                                        case nameof(CustomApiRequestParameterAttribute.Description) when namedArg.Value.Value is string description:
                                            requestParameter.Description = description;
                                            break;
                                        case nameof(CustomApiRequestParameterAttribute.LogicalEntityName) when namedArg.Value.Value is string logicalEntityName:
                                            requestParameter.LogicalEntityName = logicalEntityName;
                                            break;
                                    }
                                }
                            }

                            customApi.RequestParameters.Add(requestParameter);
                        }
                    }
                }

                var responseAttribute = innerSymbol.GetAttributes().FirstOrDefault(x => x.AttributeClass?.ToDisplayString() == typeof(CustomApiResponseAttribute).FullName);
                if (responseAttribute != null)
                {
                    customApi.ResponseTypeName = innerSymbol.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat);
                    foreach (var member in innerSymbol.GetMembers())
                    {
                        // we are only interested in properties.
                        if (member is IPropertySymbol innerProperty && innerProperty.DeclaredAccessibility is Accessibility.Public or Accessibility.Internal or Accessibility.ProtectedAndInternal)
                        {
                            var responsePropertyAttr = innerProperty.GetAttributes()
                                .FirstOrDefault(x => x.AttributeClass?.ToDisplayString() == typeof(CustomApiResponsePropertyAttribute).FullName);

                            var responseProperty = new WebApi.Entities.CustomApiResponseProperty()
                            {
                                Name = innerProperty.Name,
                                UniqueName = innerProperty.Name,
                                DisplayName = innerProperty.Name,
                                Type = CustomApiFieldTypeMapping.TryGetValue(innerProperty.Type.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat), out var fieldType)
                                    ? fieldType 
                                    : innerProperty.Type.TypeKind == TypeKind.Enum 
                                        ? WebApi.Types.CustomApiFieldType.Picklist 
                                        : WebApi.Types.CustomApiFieldType.String,
                                TypeName = innerProperty.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
                                FullTypeName = innerProperty.Type.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
                            };

                            // If the propety has an attribute, values from the attribute will override the default values.
                            if (responsePropertyAttr != null)
                            {
                                foreach (var namedArg in responsePropertyAttr.NamedArguments)
                                {
                                    switch (namedArg.Key)
                                    {
                                        case nameof(CustomApiResponsePropertyAttribute.UniqueName) when namedArg.Value.Value is string uniqueName:
                                            responseProperty.UniqueName = uniqueName;
                                            break;
                                        case nameof(CustomApiResponsePropertyAttribute.DisplayName) when namedArg.Value.Value is string displayName:
                                            responseProperty.DisplayName = displayName;
                                            break;
                                        case nameof(CustomApiResponsePropertyAttribute.Description) when namedArg.Value.Value is string description:
                                            responseProperty.Description = description;
                                            break;
                                        case nameof(CustomApiResponsePropertyAttribute.LogicalEntityName) when namedArg.Value.Value is string logicalEntityName:
                                            responseProperty.LogicalEntityName = logicalEntityName;
                                            break;
                                    }
                                }
                            }

                            customApi.ResponseProperties.Add(responseProperty);
                        }
                    }
                }
            }
            pluginConfig.CustomApi = customApi;
        }

        if (pluginConfig == null) return null;

        var dependencyGraph = dependencyAnalyzer.Analyze(typeSymbol, compilation);
        if (dependencyGraph != null)
        {
            dependencyPreparation.Prepare(dependencyGraph);
        }

        pluginConfig.DependencyGraph = dependencyGraph;

        return pluginConfig;
    }

    private PluginTypeConfig CreatePluginTypeConfig(INamedTypeSymbol typeSymbol, AttributeData attributeData)
    {
        var typeName = typeSymbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);
        var plugin = new PluginTypeConfig()
        {
            Namespace = typeSymbol.ContainingNamespace.ToDisplayString(),
            BaseTypeName = typeSymbol.BaseType?.Name,
            BaseTypeNamespace = typeSymbol.BaseType?.ContainingNamespace.ToDisplayString(),
            BaseTypeMethodNames = typeSymbol.BaseType?.GetMembers()
                .Where(m => m.Kind == SymbolKind.Method && m.DeclaredAccessibility is Accessibility.Public or Accessibility.ProtectedAndInternal or Accessibility.Internal)
                .Select(m => m.Name).ToList() ?? [],
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
    {
        var imageConfig = ReflectionHelper.SetPropertiesFromAttribute(new PluginStepImageConfig(), attributeData);

        // TODO: This extra code is because ImageAttribute.Attributes name does not match ImageConfig.ImageAttribute property, so
        // ReflectionHelper.SetPropertiesFromAttribute is not able to set it automatically. In future when we completely mode to
        // Web API, this code can be removed and the method becomes just one line.
        if (attributeData.ConstructorArguments.Length > 1)
        {
            var attributes = attributeData.ConstructorArguments[1].Value;
            imageConfig.ImageAttributes = attributeData.ConstructorArguments[1].Value as string;
            return imageConfig;
        }

        imageConfig.ImageAttributes = attributeData.NamedArguments.FirstOrDefault(a => a.Key == "Attributes").Value.Value as string;

        return imageConfig;
    }
}
#nullable restore