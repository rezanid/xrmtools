﻿#nullable enable
namespace XrmTools;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextTemplating.VSHost;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.Analyzers;
using XrmTools.Core.Helpers;
using XrmTools.Core.Repositories;
using XrmTools.Helpers;
using XrmTools.Logging.Compatibility;
using XrmTools.Options;
using XrmTools.Resources;
using XrmTools.Settings;
using XrmTools.Xrm.Generators;
using XrmTools.Xrm.Model;
using XrmTools.Xrm.Repositories;

public class XrmCodeGenerator : BaseCodeGeneratorWithSite
{
    public const string Name = Vsix.Name + " Plugin Code Generator";
    public const string Description = "Generates plugin code from .dej.json file.";

    private bool disposed = false;

    [Import]
    IXrmPluginCodeGenerator Generator { get; set; }

    [Import]
    internal IRepositoryFactory RepositoryFactory { get; set; }

    [Import]
    internal ISettingsProvider SettingsProvider { get; set; }

    [Import]
    internal ILogger<XrmCodeGenerator> Logger { get; set; }

    [Import]
    internal ITemplateFinder TemplateFinder { get; set; }

    [Import]
    internal ITemplateFileGenerator TemplateFileGenerator { get; set; }

    [Import]
    internal IXrmMetaDataService XrmMetaDataService { get; set; }

    public override string GetDefaultExtension() => ".cs";

    public XrmCodeGenerator() => SatisfyImports();

    [MemberNotNull(nameof(Generator), nameof(RepositoryFactory), 
        nameof(Logger), nameof(TemplateFinder), nameof(TemplateFileGenerator),
        nameof(SettingsProvider), nameof(XrmMetaDataService))]
    private void SatisfyImports()
    {
        var componentModel = (IComponentModel)Package.GetGlobalService(typeof(SComponentModel));
        componentModel?.DefaultCompositionService.SatisfyImportsOnce(this);
        if (Generator == null) throw new InvalidOperationException(string.Format(Strings.MissingServiceDependency, nameof(XrmCodeGenerator), nameof(Generator)));
        if (RepositoryFactory == null) throw new InvalidOperationException(string.Format(Strings.MissingServiceDependency, nameof(XrmCodeGenerator), nameof(RepositoryFactory)));
        if (SettingsProvider == null) throw new InvalidOperationException(string.Format(Strings.MissingServiceDependency, nameof(XrmCodeGenerator), nameof(SettingsProvider)));
        if (Logger == null) throw new InvalidOperationException(string.Format(Strings.MissingServiceDependency, nameof(XrmCodeGenerator), nameof(Logger)));
        if (TemplateFinder == null) throw new InvalidOperationException(string.Format(Strings.MissingServiceDependency, nameof(XrmCodeGenerator), nameof(TemplateFinder)));
        if (TemplateFileGenerator == null) throw new InvalidOperationException(string.Format(Strings.MissingServiceDependency, nameof(XrmCodeGenerator), nameof(TemplateFileGenerator)));
        if (XrmMetaDataService == null) throw new InvalidOperationException(string.Format(Strings.MissingServiceDependency, nameof(XrmCodeGenerator), nameof(XrmMetaDataService)));
    }

    protected override byte[]? GenerateCode(string inputFileName, string inputFileContent)
    {
        if (string.IsNullOrWhiteSpace(inputFileContent)) { return null; }
        if (Generator is null) { return Encoding.UTF8.GetBytes("// No generator found."); }

        return ThreadHelper.JoinableTaskFactory.Run(async () =>
        {
            var inputModel = await GetInputModelFromFileAsync(inputFileName, inputFileContent);
            if (inputModel == null)
            {
                Logger.LogWarning("Failed to parse input file for code generation. Please review the input file and try again.");
                return null;
            }

            var templateFilePath = GetTemplateFilePath(inputModel);
            // Check if template file exists.
            if (string.IsNullOrEmpty(templateFilePath))
            {
                return Encoding.UTF8.GetBytes("// " + Strings.PluginGenerator_TemplateNotSet);
            }
            if (!File.Exists(templateFilePath))
            {
                return Encoding.UTF8.GetBytes("// " + string.Format(Strings.PluginGenerator_TemplateFileNotFound, templateFilePath));
            }

            Generator.Config = new XrmCodeGenConfig
            {
                //TODO: The GetDefaultNamespace is not required. The FileNamespace is never empty even when not set.
                DefaultNamespace = string.IsNullOrWhiteSpace(FileNamespace) ? GetDefaultNamespace() : FileNamespace,
                TemplateFilePath = templateFilePath
            };

            AddEntityMetadataToPluginDefinition(inputModel!);

            if (GeneralOptions.Instance.LogLevel == LogLevel.Trace)
            {
                // We use Newtonsoft for serialization because it supports polymorphic types
                // Probably through old serialization attributes set on Xrm.Sdk types.
                var serializedConfig = inputModel.SerializeJson(useNewtonsoft: true);
                File.WriteAllText(Path.ChangeExtension(inputFileName, ".model.json"), serializedConfig);
            }

            var validation = Generator.IsValid(inputModel);
            if (validation != ValidationResult.Success)
            {
                return Encoding.UTF8.GetBytes("// " + validation.ErrorMessage);
            }
            return Encoding.UTF8.GetBytes(Generator.GenerateCode(inputModel));
        });
    }

    private async Task<PluginAssemblyConfig?> GetInputModelFromFileAsync(string inputFileName, string inputFileContent)
    {
        if (".cs".Equals(Path.GetExtension(inputFileName), StringComparison.OrdinalIgnoreCase))
        {
            await XrmMetaDataService.ParseAsync(inputFileName);
        }
        else if (".json".Equals(Path.GetExtension(inputFileName), StringComparison.OrdinalIgnoreCase))
        {
            return ParseJsonInputFile(inputFileName, inputFileContent);
        }
        return null;
    }

    private PluginAssemblyConfig? ParseJsonInputFile(string inputFileName, string inputFileContent)
    {
        try
        {
            return inputFileContent.DeserializeJson<PluginAssemblyConfig>();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, Strings.PluginGenerator_DeserializationError, inputFileName);
        }
        return null;
    }

    private string? GetTemplateFilePath(PluginAssemblyConfig config)
    {
        if (config == null) return null;
        bool isTemplatePlugin = config.PluginTypes?.Any() ?? false;
        bool isTemplateEntity = config.Entities?.Any() ?? false;
        if (!isTemplatePlugin && !isTemplateEntity) 
        {
            Logger.LogWarning("Input model for code generation neither contains any plugin nor entity definition.");
            return null;
        }

        var templateFilePath = isTemplatePlugin ? TemplateFinder.FindPluginTemplatePath(InputFilePath) : TemplateFinder.FindEntityTemplatePath(InputFilePath);
        if (templateFilePath != null) return templateFilePath;

        Logger.LogTrace("No template found for " + (isTemplatePlugin ? "plugin code generation." : "entity code generation."));
        Logger.LogInformation("Atempting to create plugin default templates.");
        ThreadHelper.JoinableTaskFactory.Run(async () =>
        {
            await TemplateFileGenerator.GenerateTemplatesAsync();
            await SettingsProvider.ProjectSettings.EntityTemplateFilePathAsync();
        });
        Logger.LogInformation("Default template generation completed.");

        templateFilePath = isTemplatePlugin ? TemplateFinder.FindPluginTemplatePath(InputFilePath) : TemplateFinder.FindEntityTemplatePath(InputFilePath);
        if (templateFilePath != null) return templateFilePath;
        Logger.LogCritical("Still, no template found for " + (isTemplatePlugin ? "plugin generation." : "entity generation."));
        return null;
    }

    private EntityMetadata? GetEntityMetadata(string logicalName, IEnumerable<string> attributeNames, IEnumerable<string> prefixesToRemove)
    {
        var entityMetadataRepo = ThreadHelper.JoinableTaskFactory.Run(async () => await RepositoryFactory.CreateRepositoryAsync<IEntityMetadataRepository>());
        if (entityMetadataRepo is null) return null;
        using var cts = new CancellationTokenSource(120000);
        var entityDefinition = ThreadHelper.JoinableTaskFactory.Run(async () => await entityMetadataRepo.GetAsync(logicalName, cts.Token));
        if (entityDefinition == null) { return null; }
        
        //NOTE!
        // Logical attributes to avoid unnecessary processing.
        var filteredAttributes =
            attributeNames.Count() == 0 ?
            entityDefinition.Attributes :
            [.. entityDefinition.Attributes.Where(a => attributeNames.Contains(a.LogicalName))];
        //    entityDefinition.Attributes.Where(a => a.AttributeType != AttributeTypeCode.EntityName && a.IsLogical != true).ToArray() :
        //    entityDefinition.Attributes.Where(a => a.AttributeType != AttributeTypeCode.EntityName && attributes.Contains(a.LogicalName)).ToArray();

        FormatSchemNames(filteredAttributes ?? entityDefinition.Attributes, prefixesToRemove);
        FormatSchemaName(entityDefinition, prefixesToRemove);

        // The cloning is done because we don't want to modify the object in the cache.
        // In future when we load from local storage this might not be required.
        if (filteredAttributes?.Length != entityDefinition.Attributes.Length)
        {
            var clone = entityDefinition.Clone();
            var propertyInfo = typeof(EntityMetadata).GetProperty("Attributes");
            propertyInfo.SetValue(clone, filteredAttributes);
            return clone;
        }
        return entityDefinition;
    }

    private static void FormatSchemaName(EntityMetadata entityDefinition, IEnumerable<string> prefixesToRemove)
    {
        // Remove prefixes.
        foreach (var prefix in prefixesToRemove)
        {
            if (entityDefinition.SchemaName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                entityDefinition.SchemaName = entityDefinition.SchemaName[prefix.Length..];
            }
        }
        // Capitalize first letter.
        if (char.IsLower(entityDefinition.SchemaName[0]))
        {
            entityDefinition.SchemaName = char.ToUpper(entityDefinition.SchemaName[0]) + entityDefinition.SchemaName[1..];
        }
    }

    private static void FormatSchemNames(IEnumerable<AttributeMetadata> attributes, IEnumerable<string> prefixesToRemove)
    {
        foreach (var attribute in attributes)
        {
            // Remove prefixes.
            foreach (var prefix in prefixesToRemove)
            {
                if (attribute.SchemaName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    attribute.SchemaName = attribute.SchemaName[prefix.Length..];
                }
            }
            // Capitalize first letter.
            if (char.IsLower(attribute.SchemaName[0]))
            {
                attribute.SchemaName = char.ToUpper(attribute.SchemaName[0]) + attribute.SchemaName[1..];
            }
        }
    }

    private string GetDefaultNamespace()
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        if (GetService(typeof(IVsHierarchy)) is IVsHierarchy hierarchy)
        {
            // Get the current item ID
            hierarchy.ParseCanonicalName(InputFilePath, out var itemId);

            hierarchy.SetProperty(itemId, (int)__VSHPROPID.VSHPROPID_DefaultNamespace, "XrmGenTest");

            if (hierarchy.GetProperty(itemId, (int)__VSHPROPID.VSHPROPID_DefaultNamespace, out object defaultNamespace) == VSConstants.S_OK)
            {
                return defaultNamespace as string ?? string.Empty;
            }
        }
        return string.Empty;
    }

    private void AddEntityMetadataToPluginDefinition(PluginAssemblyConfig config)
    {
        // Let's first keep track of all the attributes that are used in the plugin definitions.
        var entityAttributes = new Dictionary<string, HashSet<string>>();
        // We also keep track of all the entities that are used in the plugin definitions.
        var entityDefinitions = new Dictionary<string, EntityMetadata>();
        foreach (var step in config.PluginTypes.SelectMany(plugin => plugin.Steps))
        {
            if (!string.IsNullOrWhiteSpace(step.PrimaryEntityName))
            {
                var filteringAttributes = step.FilteringAttributes?.Split([','], StringSplitOptions.RemoveEmptyEntries) ?? [];
                if (!entityAttributes.ContainsKey(step.PrimaryEntityName!))
                {
                    entityAttributes[step.PrimaryEntityName!] = [.. filteringAttributes];
                }
                else if (filteringAttributes.Length == 0)
                {
                    // Since we don't have any filtering attributes, we assume all attributes are used.
                    // so we don't need to keep track of them.
                    entityAttributes[step.PrimaryEntityName!] = [];
                }
                else if (entityAttributes[step.PrimaryEntityName!].Count > 0)
                {
                    // We have some attributes already, so we add the new ones too.
                    entityAttributes[step.PrimaryEntityName!].UnionWith(filteringAttributes);
                }
                var entityDefinition = GetEntityMetadata(step.PrimaryEntityName!, filteringAttributes, config.RemovePrefixes);
                step.PrimaryEntityDefinition = entityDefinition;
                //if (entityDefinition is not null && !entityDefinitions.ContainsKey(entityDefinition.LogicalName))
                //{
                //    entityDefinitions[entityDefinition.LogicalName] = GetEntityMetadata(step.PrimaryEntityName!, [], config.RemovePrefixesCollection)!;
                //}
            }
            foreach (var image in step.Images)
            {
                if (!string.IsNullOrWhiteSpace(image.EntityAlias))
                {
                    var attributes = image.ImageAttributes?.Split(',') ?? [];
                    if (!entityAttributes.ContainsKey(step.PrimaryEntityName!))
                    {
                        entityAttributes[step.PrimaryEntityName!] = [.. attributes];
                    }
                    else if (attributes.Length == 0)
                    {
                        // Since we don't have any filtering attributes, we assume all attributes are used.
                        // so we don't need to keep track of them.
                        entityAttributes[step.PrimaryEntityName!] = [];
                    }
                    else if (entityAttributes[step.PrimaryEntityName!].Count > 0)
                    {
                        entityAttributes[step.PrimaryEntityName!].UnionWith(attributes);
                    }
                    var entityDefinition = GetEntityMetadata(step.PrimaryEntityName!, attributes, config.RemovePrefixes);
                    image.MessagePropertyDefinition = entityDefinition;
                    //if (entityDefinition is not null && !entityDefinitions.ContainsKey(entityDefinition.LogicalName))
                    //{
                    //    entityDefinitions[entityDefinition.LogicalName] = GetEntityMetadata(step.PrimaryEntityName!, [], config.RemovePrefixesCollection)!;
                    //}
                }
            }
        }
        // Now we update entity definitions with the attributes used in the plugin definitions.
        foreach(var entityEntry in entityAttributes)//.Where(e => e.Value.Count > 0))
        {
            //var entityDefinition = entityDefinitions[entity.Key];
            //var entityDefinition = GetEntityMetadata(entity.Key, entity.Value, config.RemovePrefixesCollection);
            //var attributesUsedInPluginDefinitions = entityDefinition.Attributes.Where(a => entity.Value.Contains(a.LogicalName)).ToArray();
            //typeof(EntityMetadata).GetProperty("Attributes").SetValue(entityDefinition, attributesUsedInPluginDefinitions);
            entityDefinitions[entityEntry.Key] = GetEntityMetadata(entityEntry.Key, entityEntry.Value, config.RemovePrefixes)!;
        }
        foreach (var entityConfig in config.Entities)
        {
            if (!string.IsNullOrEmpty(entityConfig.LogicalName))
            {
                entityDefinitions[entityConfig.LogicalName!] = GetEntityMetadata(entityConfig.LogicalName!, entityConfig.Attributes?.SplitAndTrim(',') ?? [], config.RemovePrefixes)!;
            }
        }
        config.EntityDefinitions = entityDefinitions.Values;
    }

    #region IDisposable Support
    protected override void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                // Dispose managed resources
                if (RepositoryFactory is IDisposable disposableFactory)
                {
                    disposableFactory.Dispose();
                }
            }

            // Free unmanaged resources (if any)
            // Nope!
            disposed = true;
        }

        // Call the base class's Dispose method
        base.Dispose(disposing);
    }

    // Finalizer to ensure resources are released if Dispose is not called
    ~XrmCodeGenerator() => Dispose(false);
    #endregion
}
#nullable restore