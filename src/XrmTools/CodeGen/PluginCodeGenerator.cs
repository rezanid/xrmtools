#nullable enable
namespace XrmTools;

using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextTemplating.VSHost;
using Newtonsoft.Json;
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
using XrmTools.Environments;
using XrmTools.Helpers;
using XrmTools.Logging.Compatibility;
using XrmTools.Meta.Model;
using XrmTools.Meta.Model.Configuration;
using XrmTools.Options;
using XrmTools.Resources;
using XrmTools.Serialization;
using XrmTools.Settings;
using XrmTools.WebApi.Entities;
using XrmTools.Xrm.Generators;
using XrmTools.Xrm.Repositories;
using static XrmTools.Helpers.ProjectExtensions;

public class PluginCodeGenerator : BaseCodeGeneratorWithSite
{
    public const string Name = Vsix.Name + " Plugin Code Generator";
    public const string Description = "Generates plugin code from .dej.json file.";

    private bool disposed = false;

    [Import]
    IXrmCodeGenerator Generator { get; set; }

    [Import]
    internal IRepositoryFactory RepositoryFactory { get; set; }

    [Import]
    public IEnvironmentProvider EnvironmentProvider { get; set; }

    [Import]
    internal ISettingsProvider SettingsProvider { get; set; }

    [Import]
    internal ILogger<PluginCodeGenerator> Logger { get; set; }

    [Import]
    internal ITemplateFinder TemplateFinder { get; set; }

    [Import]
    internal ITemplateFileGenerator TemplateFileGenerator { get; set; }

    [Import]
    internal IXrmMetaDataService XrmMetaDataService { get; set; }

    public override string GetDefaultExtension() => ".g.cs";

    public PluginCodeGenerator() => SatisfyImports();

    [MemberNotNull(nameof(Generator), nameof(RepositoryFactory), nameof(EnvironmentProvider),
        nameof(Logger), nameof(TemplateFinder), nameof(TemplateFileGenerator),
        nameof(SettingsProvider), nameof(XrmMetaDataService))]
    private void SatisfyImports()
    {
        var componentModel = (IComponentModel)Package.GetGlobalService(typeof(SComponentModel));
        componentModel?.DefaultCompositionService.SatisfyImportsOnce(this);
        if (Generator == null) throw new InvalidOperationException(string.Format(Strings.MissingServiceDependency, nameof(PluginCodeGenerator), nameof(Generator)));
        if (RepositoryFactory == null) throw new InvalidOperationException(string.Format(Strings.MissingServiceDependency, nameof(PluginCodeGenerator), nameof(RepositoryFactory)));
        if (EnvironmentProvider == null) throw new InvalidOperationException(string.Format(Strings.MissingServiceDependency, nameof(PluginCodeGenerator), nameof(EnvironmentProvider)));
        if (SettingsProvider == null) throw new InvalidOperationException(string.Format(Strings.MissingServiceDependency, nameof(PluginCodeGenerator), nameof(SettingsProvider)));
        if (Logger == null) throw new InvalidOperationException(string.Format(Strings.MissingServiceDependency, nameof(PluginCodeGenerator), nameof(Logger)));
        if (TemplateFinder == null) throw new InvalidOperationException(string.Format(Strings.MissingServiceDependency, nameof(PluginCodeGenerator), nameof(TemplateFinder)));
        if (TemplateFileGenerator == null) throw new InvalidOperationException(string.Format(Strings.MissingServiceDependency, nameof(PluginCodeGenerator), nameof(TemplateFileGenerator)));
        if (XrmMetaDataService == null) throw new InvalidOperationException(string.Format(Strings.MissingServiceDependency, nameof(PluginCodeGenerator), nameof(XrmMetaDataService)));
    }

    [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "The following supression is necessary for ThreadHelper.JoinableTaskFactory.Run")]
    [SuppressMessage("Usage", "VSTHRD104:Offer async methods", Justification = "This method is considered the entry point for code generation.")]
    protected override byte[]? GenerateCode(string inputFileName, string inputFileContent)
    {
        if (string.IsNullOrWhiteSpace(inputFileContent)) { return null; }
        if (Generator is null) { return Encoding.UTF8.GetBytes("// No generator found."); }
        
        return ThreadHelper.JoinableTaskFactory.Run(async () =>
        {
            PluginAssemblyConfig? inputModel = null;
            try
            {
                inputModel = await GetInputModelFromFileAsync(inputFileName, inputFileContent);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Parsing input file filed due to an error.");
            }
            if (inputModel == null)
            {
                Logger.LogWarning("Failed to parse input file for code generation. Please review the input file and try again.");
                return null;
            }

            var inputFile = await PhysicalFile.FromFileAsync(inputFileName);

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            if (inputFile is null || inputFile.FindParent(SolutionItemType.Project) is not Project project)
            {
                return Encoding.UTF8.GetBytes("// Unable to find the input file or its project.");
            }

            var templateFilePath = await GetTemplateFilePathAsync(inputModel, project);

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
                DefaultNamespace = string.IsNullOrWhiteSpace(FileNamespace) ? GetDefaultNamespace() : FileNamespace,
                TemplateFilePath = templateFilePath,
                InputFileName = inputFileName
            };

            var currentEnvironment = await EnvironmentProvider.GetActiveEnvironmentAsync();
            if (currentEnvironment == null || currentEnvironment == DataverseEnvironment.Empty)
            {
                return Encoding.UTF8.GetBytes(
                    "// Code generation failed because active environment has not been setup." +
                    " Please go to Tools > Options > XRM Tools to setup the environment and set the current environment.");
            }

            try
            {
                using var cts = new CancellationTokenSource(120000);
                await AddEntityMetadataToPluginDefinitionAsync(inputModel!, cts.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                Logger.LogWarning("Metadata retrieval was canceled.");
                throw;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to retrieve metadata for code generation");
                throw;
            }

            if (GeneralOptions.Instance.LogLevel == LogLevel.Trace)
            {
                var serializedConfig = JsonConvert.SerializeObject(inputModel, new JsonSerializerSettings
                {
                    ContractResolver = new PolymorphicContractResolver()
                });
                File.WriteAllText(Path.ChangeExtension(inputFileName, ".model.json"), serializedConfig);
            }

            var validation = Generator.IsValid(inputModel);
            if (validation != ValidationResult.Success)
            {
                return Encoding.UTF8.GetBytes("// " + validation.ErrorMessage);
            }

            string? generatedCode = null;

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            if (project.IsSdkStyle())
            {
                var lastGenFileName = await inputFile.GetAttributeAsync("LastGenOutput");
                if (string.IsNullOrWhiteSpace(lastGenFileName))
                {
                    lastGenFileName = Path.ChangeExtension(Path.GetFileName(inputFileName), ".g.cs");
                    await inputFile.TrySetAttributeAsync(PhysicalFileAttribute.LastGenOutput, lastGenFileName);
                }
                var lastGenFilePath = Path.Combine(Path.GetDirectoryName(inputFileName), lastGenFileName);
                generatedCode = Generator.GenerateCode(inputModel);
                File.WriteAllText(lastGenFilePath, "// SDK-Style Code Gen\r\n" + generatedCode);
            }
            generatedCode ??= Generator.GenerateCode(inputModel);
            return Encoding.UTF8.GetBytes(Generator.GenerateCode(inputModel));
        });
    }

    private async Task<PluginAssemblyConfig?> GetInputModelFromFileAsync(string inputFileName, string inputFileContent)
    {
        if (".cs".Equals(Path.GetExtension(inputFileName), StringComparison.OrdinalIgnoreCase))
        {
            return await XrmMetaDataService.ParsePluginsAsync(inputFileName);
        }
        //else if (".json".Equals(Path.GetExtension(inputFileName), StringComparison.OrdinalIgnoreCase))
        //{
        //    return ParseJsonInputFile(inputFileName, inputFileContent);
        //}
        return null;
    }

    private async Task<string?> GetTemplateFilePathAsync(PluginAssemblyConfig config, Project project)
    {
        if (config == null) return null;
        bool isTemplatePlugin = config.PluginTypes?.Any() ?? false;
        bool isTemplateEntity = config.Entities?.Any() ?? false;
        if (!isTemplatePlugin && !isTemplateEntity) 
        {
            Logger.LogWarning("Input model for code generation neither contains any plugin nor entity definition.");
            return null;
        }

        string? templateFilePath;
        if (isTemplatePlugin)
        {
            templateFilePath = await TemplateFinder.FindPluginTemplatePathAsync(InputFilePath);
        }
        else
        {
            templateFilePath = await TemplateFinder.FindEntityTemplatePathAsync(InputFilePath);
        }
        if (templateFilePath != null) return templateFilePath;

        Logger.LogTrace("No template found for " + (isTemplatePlugin ? "plugin code generation." : "entity code generation."));
        Logger.LogInformation("Atempting to create plugin default templates.");

        await TemplateFileGenerator.GenerateTemplatesAsync(project, false);

        Logger.LogInformation("Default template generation completed.");

        if (isTemplatePlugin)
        {
            templateFilePath = await TemplateFinder.FindPluginTemplatePathAsync(InputFilePath);
        }
        else
        {
            templateFilePath = await TemplateFinder.FindEntityTemplatePathAsync(InputFilePath);
        }
        if (templateFilePath != null) return templateFilePath;
        Logger.LogCritical("Still, no template found for " + (isTemplatePlugin ? "plugin generation." : "entity generation."));
        return null;
    }

    private async Task<EntityMetadata?> GetEntityMetadataAsync(string logicalName, IEnumerable<string> attributeNames, CodeGenReplacePrefixConfig[] prefixReplacements, CancellationToken ct)
    {
        using var entityMetadataRepo = RepositoryFactory.CreateRepository<IEntityMetadataRepository>();
        if (entityMetadataRepo is null) return null;

        var entityDefinition = await entityMetadataRepo.GetAsync(logicalName, ct).ConfigureAwait(false);
        if (entityDefinition == null) { return null; }
        
        //NOTE!
        // Logical attributes to avoid unnecessary processing.
        var filteredAttributes =
            attributeNames.Count() == 0 ?
            entityDefinition.Attributes :
            [.. entityDefinition.Attributes.Where(a => attributeNames.Contains(a.LogicalName))];

        FormatAttributeSchemaNames(filteredAttributes ?? entityDefinition.Attributes ?? [], prefixReplacements);
        FormatEntitySchemaName(entityDefinition, prefixReplacements);

        // The cloning is done because we don't want to modify the object in the cache.
        // In future when we load from local storage this might not be required.
        if (filteredAttributes?.Length != entityDefinition.Attributes?.Length)
        {
            var clone = entityDefinition.Clone();
            var propertyInfo = typeof(EntityMetadata).GetProperty("Attributes");
            propertyInfo.SetValue(clone, filteredAttributes);
            return clone;
        }
        return entityDefinition;
    }

    private static void FormatEntitySchemaName(EntityMetadata entityDefinition, CodeGenReplacePrefixConfig[] prefixReplacements)
    {
        // Remove prefixes.
        foreach (var prefixReplacement in prefixReplacements)
            foreach (var prefix in prefixReplacement.PrefixList)
                if (entityDefinition.SchemaName!.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    entityDefinition.SchemaName = entityDefinition.SchemaName[prefix.Length..];
                    if (!string.IsNullOrWhiteSpace(prefixReplacement.ReplaceWith))
                    {
                        entityDefinition.SchemaName = prefixReplacement.ReplaceWith + entityDefinition.SchemaName;
                    }
                    break;
                }
        // Capitalize first letter.
        if (char.IsLower(entityDefinition.SchemaName![0]))
        {
            entityDefinition.SchemaName = char.ToUpper(entityDefinition.SchemaName[0]) + entityDefinition.SchemaName[1..];
        }
    }

    private static void FormatAttributeSchemaNames(IEnumerable<AttributeMetadata> attributes, CodeGenReplacePrefixConfig[] prefixReplacements)
    {
        foreach (var attribute in attributes)
        {
            // Remove prefixes.
            foreach (var prefixReplacement in prefixReplacements)
                foreach (var prefix in prefixReplacement.PrefixList)
                    if (attribute.SchemaName!.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    {
                        attribute.SchemaName = attribute.SchemaName[prefix.Length..];
                        if (!string.IsNullOrWhiteSpace(prefixReplacement.ReplaceWith))
                        {
                            attribute.SchemaName = prefixReplacement.ReplaceWith + attribute.SchemaName;
                        }
                        break;
                    }
            // Capitalize first letter.
            if (char.IsLower(attribute.SchemaName![0]))
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

            if (hierarchy.GetProperty(itemId, (int)__VSHPROPID.VSHPROPID_DefaultNamespace, out object defaultNamespace) == VSConstants.S_OK)
            {
                return defaultNamespace as string ?? string.Empty;
            }
        }
        return string.Empty;
    }

    private async Task AddEntityMetadataToPluginDefinitionAsync(PluginAssemblyConfig config, CancellationToken ct)
    {
        // Let's first keep track of all the attributes that are used in the plugin definitions.
        var entitiesAndAttributes = new Dictionary<string, HashSet<string>>();
        // We also keep track of all the entities that are used in the plugin definitions.
        var entityDefinitions = new Dictionary<string, EntityMetadata?>();
        foreach (var step in config.PluginTypes.SelectMany(plugin => plugin.Steps).Where(s => !string.IsNullOrWhiteSpace(s.PrimaryEntityName)))
        {
            ct.ThrowIfCancellationRequested();

            var filteringAttributes = step.FilteringAttributes?.Split([','], StringSplitOptions.RemoveEmptyEntries) ?? [];
            if (!entitiesAndAttributes.ContainsKey(step.PrimaryEntityName!))
            {
                entitiesAndAttributes[step.PrimaryEntityName!] = [.. filteringAttributes];
            }
            else if (filteringAttributes.Length == 0)
            {
                // Since we don't have any filtering attributes, we assume all attributes are used.
                // so we don't need to keep track of them.
                entitiesAndAttributes[step.PrimaryEntityName!] = [];
            }
            else if (entitiesAndAttributes[step.PrimaryEntityName!].Count > 0)
            {
                // We have some attributes already, so we add the new ones too.
                entitiesAndAttributes[step.PrimaryEntityName!].UnionWith(filteringAttributes);
            }
            var entityDefinition = await GetEntityMetadataAsync(step.PrimaryEntityName!, filteringAttributes, config.ReplacePrefixes, ct).ConfigureAwait(false);
            step.PrimaryEntityDefinition = entityDefinition;

            foreach (var image in step.Images)
            {
                ct.ThrowIfCancellationRequested();

                var imageAttributes = image.Attributes?.Split(',') ?? [];
                if (!entitiesAndAttributes.ContainsKey(step.PrimaryEntityName!))
                {
                    entitiesAndAttributes[step.PrimaryEntityName!] = [.. imageAttributes];
                }
                else if (imageAttributes.Length == 0)
                {
                    // Since we don't have any filtering attributes, we assume all attributes are used.
                    // so we don't need to keep track of them.
                    entitiesAndAttributes[step.PrimaryEntityName!] = [];
                }
                else if (entitiesAndAttributes[step.PrimaryEntityName!].Count > 0)
                {
                    entitiesAndAttributes[step.PrimaryEntityName!].UnionWith(imageAttributes);
                }
                entityDefinition = await GetEntityMetadataAsync(step.PrimaryEntityName!, imageAttributes, config.ReplacePrefixes, ct).ConfigureAwait(false);
                image.MessagePropertyDefinition = entityDefinition;
            }
        }
        // Now we update entity definitions with the attributes used in the plugin definitions.
        foreach (var entityEntry in entitiesAndAttributes)//.Where(e => e.Value.Count > 0))
        {
            ct.ThrowIfCancellationRequested();
            entityDefinitions[entityEntry.Key] = await GetEntityMetadataAsync(entityEntry.Key, entityEntry.Value, config.ReplacePrefixes, ct).ConfigureAwait(false);
        }
        foreach (var entityConfig in config.Entities)
        {
            ct.ThrowIfCancellationRequested();
            if (!string.IsNullOrEmpty(entityConfig.LogicalName))
            {
                entityDefinitions[entityConfig.LogicalName!] = await GetEntityMetadataAsync(entityConfig.LogicalName!, entityConfig.AttributeNames?.SplitAndTrim(',') ?? [], config.ReplacePrefixes, ct).ConfigureAwait(false)!;
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
    ~PluginCodeGenerator() => Dispose(false);
    #endregion
}
#nullable restore