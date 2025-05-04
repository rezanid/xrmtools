#nullable enable
namespace XrmTools;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextTemplating.VSHost;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using XrmTools.Xrm.Generators;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using System.Diagnostics.CodeAnalysis;
using XrmTools.Resources;
using XrmTools.Logging.Compatibility;
using XrmTools.Environments;
using XrmTools.Xrm.Repositories;
using XrmTools.Core.Repositories;
using XrmTools.Settings;
using System.Collections.Generic;
using XrmTools.Helpers;
using System.IO;
using System.Threading.Tasks;
using XrmTools.Xrm.Model;
using XrmTools.Core.Helpers;
using XrmTools.Options;
using System.ComponentModel.DataAnnotations;
using Community.VisualStudio.Toolkit;
using XrmTools.Analyzers;

public class EntityCodeGenerator : BaseCodeGeneratorWithSite
{
    public const string Name = "XrmTools Entity Generator";
    public const string Description = "Generates entity classes from metadata";

    private bool disposed = false;

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
            if (TemplateFinder.FindEntityTemplatePath(InputFilePath) is not string templateFilePath)
            { 
                Logger.LogTrace("No template found for entity code generation.");
                Logger.LogInformation("Creating default templates.");
                await TemplateFileGenerator.GenerateTemplatesAsync();
                templateFilePath = TemplateFinder.FindEntityTemplatePath(InputFilePath) ?? string.Empty;
                Logger.LogCritical("Still no template found for entity code generation.");
                if (templateFilePath == string.Empty) return Encoding.UTF8.GetBytes("// No template found for entity code generation.");
            }

            var inputModel = await GetInputModelFromFileAsync(inputFileName, inputFileContent);

            if (inputModel?.Entities?.Any() != true) 
            { 
                return Encoding.UTF8.GetBytes("// No entity definition found for entity code generation.");
            }

            Generator.Config = new XrmCodeGenConfig
            {
                //TODO: The GetDefaultNamespace is not required. The FileNamespace is never empty even when not set.
                DefaultNamespace = string.IsNullOrWhiteSpace(FileNamespace) ? GetDefaultNamespace() : FileNamespace,
                TemplateFilePath = templateFilePath
            };

            var currentEnvironment = await EnvironmentProvider.GetActiveEnvironmentAsync();
            if (currentEnvironment == null || currentEnvironment == DataverseEnvironment.Empty)
            {
                return Encoding.UTF8.GetBytes(
                    "// Code generation failed because active environment has not been setup." +
                    " Please go to Tools > Options > XRM Tools to setup the environment and set the current environment.");
            }

            AddEntityMetadataToEntityConfig(inputModel);

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

            var inputFile = await PhysicalFile.FromFileAsync(inputFileName);
            if (inputFile is not null && inputFile.FindParent(SolutionItemType.Project) is Project project && project.IsSdkStyle())
            {
                var lastGenFileName = await inputFile.GetAttributeAsync("LastGenOutput");
                var lastGenFilePath = Path.Combine(Path.GetDirectoryName(inputFileName), lastGenFileName);
                File.WriteAllText(lastGenFilePath, "// SDK-Style Code Gen\r\n" + Generator.GenerateCode(inputModel));
            }

            return Encoding.UTF8.GetBytes(Generator.GenerateCode(inputModel));
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

    private void AddEntityMetadataToEntityConfig(IPluginAssemblyConfig config) 
        => config.EntityDefinitions = [.. config.Entities
            .Where(c => !string.IsNullOrWhiteSpace(c.LogicalName))
            .Select(e => GetEntityMetadata(e.LogicalName!, e.AttributeNames?.Split(',') ?? [], config.RemovePrefixes))];

    private EntityMetadata GetEntityMetadata(string logicalName, IEnumerable<string> attributeNames, IEnumerable<string> prefixesToRemove)
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
}
#nullable restore