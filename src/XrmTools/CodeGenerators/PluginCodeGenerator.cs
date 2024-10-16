﻿#nullable enable
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextTemplating.VSHost;
using Microsoft.Xrm.Sdk.Metadata;
using Newtonsoft.Json;
using Nito.AsyncEx.Synchronous;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using XrmGen._Core;
using XrmGen.Extensions;
using XrmGen.Xrm;
using XrmGen.Xrm.Generators;
using XrmGen.Xrm.Model;

namespace XrmGen;

public class PluginCodeGenerator : BaseCodeGeneratorWithSite
{
    public const string Name = "XrmGen Plugin Generator";
    public const string Description = "Generates plugin classes from .dej.json file.";

    private readonly static OptionSetMetadataNameComparer OptionSetMetadataComparer = new ();

    private bool disposed = false;
    private IXrmPluginCodeGenerator? _generator;
    private IXrmSchemaProviderFactory? _schemaProviderFactory;

    [Import]
    IXrmPluginCodeGenerator? Generator 
    { 
        // MEF does not work here, so this is our only option.
        get => _generator ??= GlobalServiceProvider.GetService(typeof(IXrmPluginCodeGenerator)) as IXrmPluginCodeGenerator;
        set => _generator = value;
    }

    [Import]
    IXrmSchemaProviderFactory? SchemaProviderFactory 
    {
        // MEF does not work here, so this is our only option.
        get => _schemaProviderFactory ??= GlobalServiceProvider.GetService(typeof(IXrmSchemaProviderFactory)) as IXrmSchemaProviderFactory;
        set => _schemaProviderFactory = value; 
    }

    public override string GetDefaultExtension() => ".cs";

    protected override byte[]? GenerateCode(string inputFileName, string inputFileContent)
    {
        if (string.IsNullOrWhiteSpace(inputFileContent)) { return null; }
        if (Generator is null) { return Encoding.UTF8.GetBytes("// No generator found."); }
        if (GetTemplateFilePath() is not string templateFilePath) { return Encoding.UTF8.GetBytes("// Template not found."); ; }

        PluginAssemblyConfig? config = null;
        try
        {
            config = inputFileContent.DeserializeJson<PluginAssemblyConfig>();
        }
        catch (Exception ex)
        {
            Logger.Log(string.Format(Resources.Strings.PluginGenerator_DeserializationError, inputFileName));
            Logger.Log(ex.ToString());
        }
        if (config?.PluginTypes?.Any() != true) { return null; }

        Generator.Config = new XrmCodeGenConfig
        {
            //TODO: The GetDefaultNamespace is not required. The FileNamespace is never empty even when not set.
            DefaultNamespace = string.IsNullOrWhiteSpace(FileNamespace) ? GetDefaultNamespace() : FileNamespace,
            TemplateFilePath = templateFilePath
        };

        AddEntityMetadataToPluginDefinition(config!);

        //TODO: The following temporary code is used for troubleshooting and can be removed.
        //var serializedConfig = JsonConvert.SerializeObject(config);
        // Polymorphic serialization is not supported by System.Text.Json.
        //var serializedConfig  = System.Text.Json.JsonSerializer.Serialize(config, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        var serializedConfig = config.SerializeJson(useNewtonsoft: false);
        File.WriteAllText(Path.ChangeExtension(inputFileName, ".config.json"), serializedConfig);

        //End of TODO.

        var (isValid, validationMessage) = Generator.IsValid(config);
        if (!isValid)
        {
            return Encoding.UTF8.GetBytes(validationMessage);
        }
        return Encoding.UTF8.GetBytes(Generator.GenerateCode(config));
    }

    private string? GetTemplateFilePath()
    {
        var templateFilePath = InputFilePath + ".sbn";
        if (File.Exists(templateFilePath))
        {
            return templateFilePath;
        }
        templateFilePath = Path.ChangeExtension(InputFilePath, "sbn");
        if (File.Exists(templateFilePath))
        {
            return templateFilePath;
        }
        templateFilePath = GetProjectProperty("EntityCodeGenTemplatePath");
        if (string.IsNullOrWhiteSpace(templateFilePath))
        {
            return null;
        }
        if (!Path.IsPathRooted(templateFilePath))
        {
            templateFilePath = Path.GetFullPath(templateFilePath);
        }
        return File.Exists(templateFilePath) ? templateFilePath : null;
    }

    private EntityMetadata? GetEntityMetadata(string logicalName, string[] attributes, IEnumerable<string> prefixesToRemove)
    {
        var environmentUrl = GetProjectProperty("EnvironmentUrl");
        if (string.IsNullOrWhiteSpace(environmentUrl)) { return null; }
        var schemaProvider = SchemaProviderFactory?.Get(environmentUrl!);
        //make a new cancellation token for 2 minutes.
        using var cts = new CancellationTokenSource(120000);
        var entityDefinition = schemaProvider?.GetEntityAsync(logicalName, cts.Token).WaitAndUnwrapException();
        if (entityDefinition == null) { return null; }

        //NOTE!
        // Logical attributes to avoid unnecessary processing.
        var filteredAttributes =
            attributes.Length == 0 ?
            entityDefinition.Attributes :
            entityDefinition.Attributes.Where(a => attributes.Contains(a.LogicalName)).ToArray();
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

    private string? GetProjectProperty(string propertyName)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        if (GetService(typeof(IVsHierarchy)) is IVsHierarchy hierarchy)
        {
            return hierarchy.GetProjectProperty(propertyName);
        }
        return null;
    }

    private void AddEntityMetadataToPluginDefinitionOLD(PluginAssemblyConfig config)
    {
        static bool IsEnumAttribute(AttributeMetadata a) => (a.AttributeType is AttributeTypeCode.Picklist or AttributeTypeCode.Virtual or AttributeTypeCode.State or AttributeTypeCode.Status) && a.IsLogical == false;

        var optionsetMetadataList = new List<OptionSetMetadata>();
        foreach (var step in config.PluginTypes.SelectMany(plugin => plugin.Steps))
        {
            if (!string.IsNullOrWhiteSpace(step.PrimaryEntityName))
            {
                var metadata = GetEntityMetadata(step.PrimaryEntityName!, step.FilteringAttributes?.Split(',') ?? [], config.RemovePrefixesCollection);
                step.PrimaryEntityDefinition = metadata;
                if (metadata is not null)
                {
                    optionsetMetadataList.AddRange(metadata.Attributes
                        .Where(IsEnumAttribute)
                        .Cast<EnumAttributeMetadata>()
                        .Select(a => a.OptionSet));
                }
            }

            foreach (var image in step.Images)
            {
                if (!string.IsNullOrWhiteSpace(image.EntityAlias))
                {
                    var metadata = GetEntityMetadata(step.PrimaryEntityName!, image.ImageAttributes?.Split(',') ?? [], config.RemovePrefixesCollection);
                    image.MessagePropertyDefinition = metadata;
                    if (metadata is not null)
                    {
                        optionsetMetadataList.AddRange(metadata.Attributes
                            .Where(IsEnumAttribute)
                            .Cast<EnumAttributeMetadata>()
                            .Select(a => a.OptionSet));
                    }

                }
            }
        }
        //pluginDefinition.OptionSetMetadatas = optionsetMetadataList.Distinct(OptionSetMetadataComparer).ToList();
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
                var attributes = step.FilteringAttributes?.Split([','], StringSplitOptions.RemoveEmptyEntries) ?? [];
                if (!entityAttributes.ContainsKey(step.PrimaryEntityName!))
                {
                    entityAttributes[step.PrimaryEntityName!] = new HashSet<string>(attributes);
                }
                else if (attributes.Length == 0)
                {
                    entityAttributes[step.PrimaryEntityName!] = [];
                }
                else if (entityAttributes[step.PrimaryEntityName!].Count > 0)
                {
                    entityAttributes[step.PrimaryEntityName!].UnionWith(attributes);
                }
                var entityDefinition = GetEntityMetadata(step.PrimaryEntityName!, attributes, config.RemovePrefixesCollection);
                step.PrimaryEntityDefinition = entityDefinition;
                if (entityDefinition is not null && !entityDefinitions.ContainsKey(entityDefinition.LogicalName))
                {
                    entityDefinitions[entityDefinition.LogicalName] = GetEntityMetadata(step.PrimaryEntityName!, [], config.RemovePrefixesCollection)!;
                }
            }
            foreach (var image in step.Images)
            {
                if (!string.IsNullOrWhiteSpace(image.EntityAlias))
                {
                    var attributes = image.ImageAttributes?.Split(',') ?? [];
                    if (!entityAttributes.ContainsKey(step.PrimaryEntityName!))
                    {
                        entityAttributes[step.PrimaryEntityName!] = new HashSet<string>(attributes);
                    }
                    else if (entityAttributes[step.PrimaryEntityName!].Count > 0)
                    {
                        entityAttributes[step.PrimaryEntityName!].UnionWith(attributes);
                    }
                    var entityDefinition = GetEntityMetadata(step.PrimaryEntityName!, attributes, config.RemovePrefixesCollection);
                    image.MessagePropertyDefinition = entityDefinition;
                    if (entityDefinition is not null && !entityDefinitions.ContainsKey(entityDefinition.LogicalName))
                    {
                        entityDefinitions[entityDefinition.LogicalName] = GetEntityMetadata(step.PrimaryEntityName!, [], config.RemovePrefixesCollection)!;
                    }
                }
            }
        }
        // Now we update entity definitions with the attributes used in the plugin definitions.
        foreach(var entity in entityAttributes.Where(e => e.Value.Count > 0))
        {
            var entityDefinition = entityDefinitions[entity.Key];
            var attributesUsedInPluginDefinitions = entityDefinition.Attributes.Where(a => entity.Value.Contains(a.LogicalName)).ToArray();
            typeof(EntityMetadata).GetProperty("Attributes").SetValue(entityDefinition, attributesUsedInPluginDefinitions);
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
                if (SchemaProviderFactory is IDisposable disposableFactory)
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
    ~PluginCodeGenerator() => Dispose(false);
    #endregion

}

public class OptionSetMetadataNameComparer : IEqualityComparer<OptionSetMetadata>
{
    public bool Equals(OptionSetMetadata x, OptionSetMetadata y) => x.Name == y.Name;
    public int GetHashCode(OptionSetMetadata obj) => obj.Name.GetHashCode();
}
#nullable restore