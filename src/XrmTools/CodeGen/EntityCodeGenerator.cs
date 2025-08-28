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
using XrmTools.Meta.Model.Configuration;
using XrmTools.Options;
using XrmTools.Resources;
using XrmTools.Serialization;
using XrmTools.Settings;
using XrmTools.WebApi.Entities;
using XrmTools.Xrm.Generators;
using XrmTools.Xrm.Repositories;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

internal class EntityCodeGenerator : BaseCodeGeneratorWithSite
{
    public const string Name = "XrmTools Entity Generator";
    public const string Description = "Generates entity classes from metadata";

    [Import]
    public IXrmCodeGenerator? Generator { get; set; }

    [Import]
    internal IRepositoryFactory? RepositoryFactory { get; set; }

    [Import]
    public IEnvironmentProvider EnvironmentProvider { get; set; }

    [Import]
    internal ILogger<EntityCodeGenerator> Logger {  get; set; }

    [Import]
    internal ITemplateFinder TemplateFinder { get; set; }

    [Import]
    internal ITemplateFileGenerator TemplateFileGenerator { get; set; }

    [Import]
    internal IXrmMetaDataService XrmMetaDataService { get; set; }

    [Import]
    internal ISettingsProvider SettingsProvider { get; set; }

    public override string GetDefaultExtension() => ".Generated.cs";

    public EntityCodeGenerator() => SatisfyImports();

    [MemberNotNull(
        nameof(Generator), nameof(RepositoryFactory), nameof(EnvironmentProvider), 
        nameof(Logger), nameof(TemplateFinder), nameof(TemplateFileGenerator),
        nameof(SettingsProvider), nameof(XrmMetaDataService))]
    private void SatisfyImports()
    {
        var componentModel = (IComponentModel)Package.GetGlobalService(typeof(SComponentModel));
        componentModel?.DefaultCompositionService.SatisfyImportsOnce(this);
        if (Generator == null) throw new InvalidOperationException(string.Format(Strings.MissingServiceDependency, nameof(EntityCodeGenerator), nameof(Generator)));
        if (RepositoryFactory == null) throw new InvalidOperationException(string.Format(Strings.MissingServiceDependency, nameof(EntityCodeGenerator), nameof(RepositoryFactory)));
        if (EnvironmentProvider == null) throw new InvalidOperationException(string.Format(Strings.MissingServiceDependency, nameof(EntityCodeGenerator), nameof(EnvironmentProvider)));
        if (Logger == null) throw new InvalidOperationException(string.Format(Strings.MissingServiceDependency, nameof(EntityCodeGenerator), nameof(Logger)));
        if (TemplateFinder == null) throw new InvalidOperationException(string.Format(Strings.MissingServiceDependency, nameof(EntityCodeGenerator), nameof(TemplateFinder)));
        if (TemplateFileGenerator == null) throw new InvalidOperationException(string.Format(Strings.MissingServiceDependency, nameof(EntityCodeGenerator), nameof(TemplateFileGenerator)));
        if (SettingsProvider == null) throw new InvalidOperationException(string.Format(Strings.MissingServiceDependency, nameof(EntityCodeGenerator), nameof(SettingsProvider)));
        if (XrmMetaDataService == null) throw new InvalidOperationException(string.Format(Strings.MissingServiceDependency, nameof(PluginCodeGenerator), nameof(XrmMetaDataService)));
    }

    protected override byte[]? GenerateCode(string inputFileName, string inputFileContent)
    {
        if (string.IsNullOrWhiteSpace(inputFileContent)) { return null; }
        if (Generator is null) { return Encoding.UTF8.GetBytes("// No generator found."); }

        return ThreadHelper.JoinableTaskFactory.Run(async () =>
        {
            var templateFilePath = await TemplateFinder.FindEntityTemplatePathAsync(InputFilePath);
            if (templateFilePath is null)
            { 
                Logger.LogTrace("No template found for entity code generation.");
                Logger.LogInformation("Creating default templates.");
                await TemplateFileGenerator.GenerateTemplatesAsync();
                templateFilePath = await TemplateFinder.FindEntityTemplatePathAsync(InputFilePath) ?? string.Empty;
                Logger.LogCritical("Still no template found for entity code generation.");
                if (templateFilePath == string.Empty) return Encoding.UTF8.GetBytes("// No template found for entity code generation.");
            }

            var inputModel = await GetInputModelFromFileAsync(inputFileName, inputFileContent);

            if (inputModel?.Entities?.Any() != true) 
            { 
                return Encoding.UTF8.GetBytes("// No entity definition found for entity code generation.");
            }

            var currentEnvironment = await EnvironmentProvider.GetActiveEnvironmentAsync();
            if (currentEnvironment == null || currentEnvironment == DataverseEnvironment.Empty)
            {
                return Encoding.UTF8.GetBytes(
                    "// Code generation failed because active environment has not been setup." +
                    " Please go to Tools > Options > XRM Tools to setup the environment and set the current environment.");
            }

            AddEntityMetadataToEntityConfig(inputModel);

            if (inputModel.GlobalOptionSetCodeGen.Mode == Meta.Attributes.GlobalOptionSetGenerationMode.GlobalOptionSetFile)
            {
                inputModel.GlobalOptionSetDefinitions = inputModel.EntityDefinitions.Union(inputModel.OtherEntityDefinitions ?? [])
                    .SelectMany(e => e.Attributes.FilterGlobalEnumAttributes())
                    .Select(a => a.OptionSet)
                    .Where(o => o?.Name != null)
                    .GroupBy(o => o!.Name!)
                    .Select(g => g.First())
                    .ToList()!;

                // Generate GlobalOptionSets.cs file if there are any global option sets.
                if (inputModel.GlobalOptionSetDefinitions.Any())
                {
                    Generator.Config = new XrmCodeGenConfig
                    {
                        DefaultNamespace = GetDefaultNamespace() + ".OptionSets",
                        TemplateFilePath = await TemplateFinder.FindGlobalOptionSetsTemplatePathAsync()
                    };

                    var globalOptionSetFileName = await SettingsProvider.GlobalOptionSetsFilePathAsync();
                    var globalOptionSetCode = Generator.GenerateCode(inputModel);
                    File.WriteAllText(globalOptionSetFileName, globalOptionSetCode);
                }
            }

            Generator.Config = new XrmCodeGenConfig
            {
                //TODO: The GetDefaultNamespace is not required. The FileNamespace is never empty even when not set.
                DefaultNamespace = string.IsNullOrWhiteSpace(FileNamespace) ? GetDefaultNamespace() : FileNamespace,
                TemplateFilePath = templateFilePath
            };

            if (GeneralOptions.Instance.LogLevel == LogLevel.Trace)
            {
                // We use Newtonsoft for serialization because it supports polymorphic types
                // Probably through old serialization attributes set on Xrm.Sdk types.
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

            var inputFile = await PhysicalFile.FromFileAsync(inputFileName);
            string? generatedCode = null;
            if (inputFile is not null && inputFile.FindParent(SolutionItemType.Project) is Project project && project.IsSdkStyle())
            {
                var lastGenFileName = await inputFile.GetAttributeAsync("LastGenOutput");
                if (string.IsNullOrWhiteSpace(lastGenFileName))
                {
                    lastGenFileName = Path.ChangeExtension(Path.GetFileName(inputFileName), ".Generated.cs");
                    await inputFile.TrySetAttributeAsync(PhysicalFileAttribute.LastGenOutput, lastGenFileName);
                }
                var lastGenFilePath = Path.Combine(Path.GetDirectoryName(inputFileName), lastGenFileName);
                generatedCode = Generator.GenerateCode(inputModel);
                File.WriteAllText(lastGenFilePath, "// SDK-Style Code Gen\r\n" + generatedCode);
            }
            generatedCode ??= Generator.GenerateCode(inputModel);
            return Encoding.UTF8.GetBytes(generatedCode);
        });
    }

    private async Task<PluginAssemblyConfig?> GetInputModelFromFileAsync(string inputFileName, string inputFileContent)
    {
        if (".cs".Equals(Path.GetExtension(inputFileName), StringComparison.OrdinalIgnoreCase))
        {
            return await XrmMetaDataService.ParseEntitiesAsync(inputFileName);
        }
        else if (".yml".Equals(Path.GetExtension(inputFileName), StringComparison.OrdinalIgnoreCase))
        {
            return ParseYamlInputFile(inputFileName, inputFileContent);
        }
        return null;
    }

    private PluginAssemblyConfig? ParseYamlInputFile(string inputFileName, string inputFileContent)
    {
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        try
        {
            return deserializer.Deserialize<PluginAssemblyConfig>(inputFileContent);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, Strings.PluginGenerator_DeserializationError, inputFileName);
        }
        return null;
    }

    private string GetDefaultNamespace()
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        if (GetService(typeof(IVsHierarchy)) is IVsHierarchy hierarchy)
        {
            // Get the current item ID
            hierarchy.ParseCanonicalName(InputFilePath, out var itemId);

            if (hierarchy.GetProperty(itemId, (int)__VSHPROPID.VSHPROPID_DefaultNamespace, out var defaultNamespace) == VSConstants.S_OK)
            {
                return defaultNamespace as string ?? string.Empty;
            }
        }
        return string.Empty;
    }

    private void AddEntityMetadataToEntityConfig(PluginAssemblyConfig config) 
        => config.EntityDefinitions = [.. config.Entities
            .Where(c => !string.IsNullOrWhiteSpace(c.LogicalName))
            .Select(e => GetEntityMetadata(e.LogicalName!, e.AttributeNames?.Split(',') ?? [], config.ReplacePrefixes))];

    private EntityMetadata GetEntityMetadata(string logicalName, IEnumerable<string> attributeNames, Meta.Model.CodeGenReplacePrefixConfig prefixReplacement)
    {
        var entityMetadataRepo = ThreadHelper.JoinableTaskFactory.Run(RepositoryFactory.CreateRepositoryAsync<IEntityMetadataRepository>);
        if (entityMetadataRepo is null) return null;
        using var cts = new CancellationTokenSource(120000);
        var entityDefinition = ThreadHelper.JoinableTaskFactory.Run(async () => await entityMetadataRepo.GetAsync(logicalName, cts.Token));
        if (entityDefinition == null) { return null; }

        //NOTE!
        // Logical attributes to avoid unnecessary processing.
        var filteredAttributes =
            attributeNames.Count() == 0 ?
            entityDefinition.Attributes.Where(a => a.IsValidForRead && a.AttributeOf is null).ToArray() :
            [.. entityDefinition.Attributes.Where(a => attributeNames.Contains(a.LogicalName))];
        //    entityDefinition.Attributes.Where(a => a.AttributeType != AttributeTypeCode.EntityName && a.IsLogical != true).ToArray() :
        //    entityDefinition.Attributes.Where(a => a.AttributeType != AttributeTypeCode.EntityName && attributes.Contains(a.LogicalName)).ToArray();

        FormatAttributeSchemNames(filteredAttributes ?? entityDefinition.Attributes, prefixReplacement);
        FormatEntitySchemaName(entityDefinition, prefixReplacement);

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

    private static void FormatEntitySchemaName(EntityMetadata entityDefinition, Meta.Model.CodeGenReplacePrefixConfig prefixReplacement)
    {
        // Remove prefixes.
        foreach (var prefix in prefixReplacement.PrefixList)
        {
            if (entityDefinition.SchemaName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                entityDefinition.SchemaName = entityDefinition.SchemaName[prefix.Length..];
                if (!string.IsNullOrWhiteSpace(prefixReplacement.ReplaceWith))
                {
                    entityDefinition.SchemaName = prefixReplacement.ReplaceWith + entityDefinition.SchemaName;
                }
                break;
            }
        }
        // Capitalize first letter.
        if (char.IsLower(entityDefinition.SchemaName[0]))
        {
            entityDefinition.SchemaName = char.ToUpper(entityDefinition.SchemaName[0]) + entityDefinition.SchemaName[1..];
        }
    }

    private static void FormatAttributeSchemNames(IEnumerable<AttributeMetadata> attributes, Meta.Model.CodeGenReplacePrefixConfig prefixReplacement)
    {
        var prefixList = prefixReplacement.Prefixes.SplitAndTrim(',') ?? [];
        foreach (var attribute in attributes)
        {
            // Remove prefixes.
            foreach (var prefix in prefixList)
            {
                if (attribute.SchemaName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    attribute.SchemaName = attribute.SchemaName[prefix.Length..];
                    if (!string.IsNullOrWhiteSpace(prefixReplacement.ReplaceWith))
                    {
                        attribute.SchemaName = prefixReplacement.ReplaceWith + attribute.SchemaName;
                    }
                    break;
                }
            }
            // Capitalize first letter.
            if (char.IsLower(attribute.SchemaName[0]))
            {
                attribute.SchemaName = char.ToUpper(attribute.SchemaName[0]) + attribute.SchemaName[1..];
            }
        }
    }
}
#nullable restore