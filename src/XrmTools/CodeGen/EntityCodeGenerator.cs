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

public class EntityCodeGenerator : BaseCodeGeneratorWithSite
{
    public const string Name = "XrmTools Entity Generator";
    public const string Description = "Generates entity classes from metadata";

    private bool disposed = false;

    [Import]
    public IXrmEntityCodeGenerator? Generator { get; set; }

    [Import]
    internal IRepositoryFactory? RepositoryFactory { get; set; }

    [Import]
    public IEnvironmentProvider? EnvironmentProvider { get; set; }

    [Import]
    internal ILogger<EntityCodeGenerator> Logger {  get; set; }

    [Import]
    internal ITemplateFinder TemplateFinder { get; set; }

    [Import]
    internal ITemplateFileGenerator TemplateFileGenerator { get; set; }

    [Import]
    internal ISettingsProvider SettingsProvider { get; set; }

    private IEntityMetadataRepository EntityMetadataRepository;

    public override string GetDefaultExtension() => ".cs";

    public EntityCodeGenerator() => SatisfyImports();

    [MemberNotNull(
        nameof(Generator), nameof(RepositoryFactory), nameof(EnvironmentProvider), 
        nameof(Logger), nameof(EntityMetadataRepository), nameof(TemplateFinder), nameof(TemplateFileGenerator),
        nameof(SettingsProvider))]
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
        EntityMetadataRepository = ThreadHelper.JoinableTaskFactory.Run(RepositoryFactory.CreateRepositoryAsync<IEntityMetadataRepository>);
        if (EntityMetadataRepository == null) throw new InvalidOperationException(string.Format(Strings.MissingServiceDependency, nameof(EntityCodeGenerator), nameof(EntityMetadataRepository)));
    }

    protected override byte[]? GenerateCode(string inputFileName, string inputFileContent)
    {
        if (string.IsNullOrWhiteSpace(inputFileContent)) { return null; }
        if (Generator is null) { return Encoding.UTF8.GetBytes("// No generator found."); }
        if (TemplateFinder.FindEntityTemplatePath(InputFilePath) is not string templateFilePath) 
        {
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                await TemplateFileGenerator.GenerateTemplatesAsync();
                await SettingsProvider.ProjectSettings.EntityTemplateFilePathAsync();
            });
            templateFilePath = TemplateFinder.FindEntityTemplatePath(InputFilePath) ?? string.Empty;
            if (templateFilePath == string.Empty) return Encoding.UTF8.GetBytes("// Template not found.");
        }

        var entityDefinitions = ParseInputFile(inputFileName, inputFileContent);
        if (entityDefinitions?.Entities?.Any() != true) { return null; }

        Generator.Config = new XrmCodeGenConfig
        {
            DefaultNamespace = GetDefaultNamespace(),
            TemplateFilePath = templateFilePath
        };

        AddEntityMetadataToEntityConfig(entityDefinitions);

        var sb = new StringBuilder();
        foreach (var entityMetadata in entityDefinitions.EntityMetadatas)
        {
            var (isValid, validationMessage) = Generator.IsValid(entityMetadata);
            if (!isValid)
            {
                sb.Append("// ");
                sb.AppendLine(validationMessage);
                continue;
            }
            Generator.GenerateCode(entityMetadata);
        }
        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private XrmCodeGenConfig? ParseInputFile(string inputFileName, string inputFileContent)
    {
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        try
        {
            return deserializer.Deserialize<XrmCodeGenConfig>(inputFileContent);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, Strings.PluginGenerator_DeserializationError, inputFileName);
        }
        return null;
    }


    private bool IsValidEntityConfig(EntityConfig entityConfig) => !string.IsNullOrWhiteSpace(entityConfig.LogicalName) && !string.IsNullOrWhiteSpace(entityConfig.AttributeNames);

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

    private void AddEntityMetadataToEntityConfig(XrmCodeGenConfig entityCodeGenConfig) 
        => entityCodeGenConfig.EntityMetadatas = entityCodeGenConfig.Entities
            .Where(IsValidEntityConfig)
            .Select(e => GetEntityMetadata(e.LogicalName, e.AttributeNames.Split(',')))
            .ToList();

    private EntityMetadata? GetEntityMetadata(string logicalName, string[] attributes)
    {
        // Make a new cancellation token for 2 minutes.
        using var cts = new CancellationTokenSource(120000);
        var metadata = ThreadHelper.JoinableTaskFactory.Run(async () => await EntityMetadataRepository?.GetAsync(logicalName, cts.Token));
        if (metadata == null) { return null; }
        var filteredAttributes = metadata.Attributes.Where(a => attributes.Contains(a.LogicalName)).ToArray();
        var propertyInfo = typeof(EntityMetadata).GetProperty("Attributes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        propertyInfo.SetValue(metadata, filteredAttributes);
        return metadata;
    }

    #region IDisposable Support
    protected override void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                // Dispose managed resources
                if (EntityMetadataRepository is IDisposable disposableFactory)
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
    ~EntityCodeGenerator() => Dispose(false);
    #endregion
}

#nullable restore