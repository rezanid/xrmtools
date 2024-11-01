#nullable enable
namespace XrmTools;
using Community.VisualStudio.Toolkit.DependencyInjection.Core;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextTemplating.VSHost;
using Microsoft.Xrm.Sdk.Metadata;
using Nito.AsyncEx.Synchronous;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using XrmTools.Helpers;
using XrmTools.Xrm;
using XrmTools.Xrm.Generators;
using XrmTools.Xrm.Model;
using Microsoft.Extensions.DependencyInjection;
using XrmTools.Options;
using Community.VisualStudio.Toolkit;
using System.Threading.Tasks;
using Microsoft.VisualStudio.LanguageServices;
using XrmTools.Analyzers;
using Microsoft.VisualStudio.ComponentModelHost;
using XrmTools.Settings;
using XrmTools.Resources;
using Microsoft.CodeAnalysis.Elfie.Model.Strings;

public class PluginCodeGenerator : BaseCodeGeneratorWithSite
{
    public const string Name = Vsix.Name + " Plugin Code Generator";
    public const string Description = "Generates plugin code from .dej.json file.";

    private bool disposed = false;
    private IXrmPluginCodeGenerator? _generator;
    private IXrmSchemaProviderFactory? _schemaProviderFactory;
    private IEnvironmentProvider? _environmentProvider;

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

    [Import]
    ISettingsProvider SettingsProvider { get; set; }

    private IEnvironmentProvider? EnvironmentProvider
    {
        get => _environmentProvider ??= GlobalServiceProvider.GetService(typeof(IEnvironmentProvider)) as IEnvironmentProvider;
        set => _environmentProvider = value;
    }

    ILogger<PluginCodeGenerator> Logger { get; }

    public override string GetDefaultExtension() => ".cs";

    public PluginCodeGenerator()
    {
        var serviceProvider = VS.GetRequiredService<SToolkitServiceProvider<XrmToolsPackage>, IToolkitServiceProvider<XrmToolsPackage>>();
        Logger = serviceProvider.GetRequiredService<ILogger<PluginCodeGenerator>>();
        EnvironmentProvider = serviceProvider.GetRequiredService<IEnvironmentProvider>();
        SchemaProviderFactory = serviceProvider.GetRequiredService<IXrmSchemaProviderFactory>();
        SettingsProvider = serviceProvider.GetRequiredService<ISettingsProvider>();
    }

    protected override byte[]? GenerateCode(string inputFileName, string inputFileContent)
    {
        if (string.IsNullOrWhiteSpace(inputFileContent)) { return null; }
        if (Generator is null) { return Encoding.UTF8.GetBytes("// No generator found."); }
        if (GetTemplateFilePath() is not string templateFilePath) 
        { 
            return Encoding.UTF8.GetBytes("// " + Strings.PluginGenerator_TemplateNotSet); 
        }
        if (!File.Exists(templateFilePath))
        { 
            return Encoding.UTF8.GetBytes("// " + string.Format(Strings.PluginGenerator_TemplateFileNotFound, templateFilePath));
        }
        PluginAssemblyConfig? inputModel;
        if (".cs".Equals(Path.GetExtension(inputFileName), StringComparison.OrdinalIgnoreCase))
        {
            inputModel = ThreadHelper.JoinableTaskFactory.Run(async () =>
                await ParseCSharpInputFileAsync(inputFileName, inputFileContent));
        }
        else
        {
            inputModel = ParseJsonInputFile(inputFileName, inputFileContent);
        }

        if (inputModel?.PluginTypes?.Any() != true) { return null; }

        Generator.Config = new XrmCodeGenConfig
        {
            //TODO: The GetDefaultNamespace is not required. The FileNamespace is never empty even when not set.
            DefaultNamespace = string.IsNullOrWhiteSpace(FileNamespace) ? GetDefaultNamespace() : FileNamespace,
            TemplateFilePath = templateFilePath
        };

        AddEntityMetadataToPluginDefinition(inputModel!);

        if (GeneralOptions.Instance.LogLevel == LogLevel.Trace)
        {
            var serializedConfig = inputModel.SerializeJson();
            File.WriteAllText(Path.ChangeExtension(inputFileName, ".config.json"), serializedConfig);
        }

        var (isValid, validationMessage) = Generator.IsValid(inputModel);
        if (!isValid)
        {
            return Encoding.UTF8.GetBytes(validationMessage);
        }
        return Encoding.UTF8.GetBytes(Generator.GenerateCode(inputModel));
    }

    private PluginAssemblyConfig? ParseJsonInputFile(string inputFileName, string inputFileContent)
    {
        try
        {
            return inputFileContent.DeserializeJson<PluginAssemblyConfig>();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, Resources.Strings.PluginGenerator_DeserializationError, inputFileName);
        }
        return null;
    }

    private async Task<PluginAssemblyConfig?> ParseCSharpInputFileAsync(string inputFileName, string inputFileContent)
    {
        var proj = await VS.Solutions.GetActiveProjectAsync();
        if (proj is null)
        {
            Logger.LogWarning("No active project found.");
            return null;
        }
        var assemblyPath = proj.GetOutputAssemblyPath();
        if (assemblyPath == null || !File.Exists(assemblyPath))
        {
            Logger.LogWarning("Assembly not found: {0}", assemblyPath);
            return null;
        }

        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var componentModel = (IComponentModel)Package.GetGlobalService(typeof(SComponentModel));
        var workspace = componentModel.GetService<VisualStudioWorkspace>();
        var documentId = workspace.CurrentSolution.GetDocumentIdsWithFilePath(inputFileName).FirstOrDefault();
        if (documentId == null) return null;
        var document = workspace.CurrentSolution.GetDocument(documentId);
        if (document == null)
        {
            return null;
        }
        var attributeExtractor = new AttributeExtractor();
        var pluginAssemblyMetaService = new PluginAssemblyMetadataService(workspace, attributeExtractor);
        var pluginAssemblyInfo = await pluginAssemblyMetaService.GetAssemblyConfigAsync(document);
        return pluginAssemblyInfo;
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
        return ThreadHelper.JoinableTaskFactory.Run(SettingsProvider.PluginTemplateFilePathAsync);
    }

    private EntityMetadata? GetEntityMetadata(string logicalName, string[] attributes, IEnumerable<string> prefixesToRemove)
    {
        var environment = EnvironmentProvider?.GetActiveEnvironmentAsync().WaitAndUnwrapException();
        if (environment is null || !environment.IsValid) return null;
        var schemaProvider = SchemaProviderFactory?.Get(environment);
        if (schemaProvider is null) return null;
        // Make a new cancellation token for 2 minutes.
        using var cts = new CancellationTokenSource(120000);
        var entityDefinition = ThreadHelper.JoinableTaskFactory.Run(async () => await schemaProvider.GetEntityAsync(logicalName, cts.Token));
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
#nullable restore