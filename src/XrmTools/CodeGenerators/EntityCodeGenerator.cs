#nullable enable
namespace XrmTools;

using Community.VisualStudio.Toolkit;
using Community.VisualStudio.Toolkit.DependencyInjection.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextTemplating.VSHost;
using Microsoft.Xrm.Sdk.Metadata;
using Nito.AsyncEx.Synchronous;
using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using XrmTools.Helpers;
using XrmTools.Xrm;
using XrmTools.Xrm.Generators;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

public class EntityCodeGenerator : BaseCodeGeneratorWithSite
{
    public const string Name = "XrmTools Entity Generator";
    public const string Description = "Generates entity classes from metadata";

    private bool disposed = false;
    private IXrmEntityCodeGenerator? _generator;
    private IXrmSchemaProviderFactory? _schemaProviderFactory;
    private IEnvironmentProvider? _environmentProvider;
    private ILogger<EntityCodeGenerator>? _logger;

    [Import]
    private IXrmEntityCodeGenerator? Generator
    {
        // MEF does not work, so this is a workaround.
        get => _generator ??= GlobalServiceProvider.GetService(typeof(IXrmEntityCodeGenerator)) as IXrmEntityCodeGenerator;
        set => _generator = value;
    }

    private IXrmSchemaProviderFactory? SchemaProviderFactory
    {
        // MEF does not work, so this is a workaround.
        get => _schemaProviderFactory ??= GlobalServiceProvider.GetService(typeof(IXrmSchemaProviderFactory)) as IXrmSchemaProviderFactory;
        set => _schemaProviderFactory = value;
    }

    private IEnvironmentProvider? EnvironmentProvider
    {
        get => _environmentProvider ??= GlobalServiceProvider.GetService(typeof(IEnvironmentProvider)) as IEnvironmentProvider;
        set => _environmentProvider = value;
    }

    ILogger<EntityCodeGenerator> Logger
    {
        get
        {
            if (_logger is not null) return _logger;
            var serviceProvider = GlobalServiceProvider.GetService<SToolkitServiceProvider<XrmToolsPackage>, IToolkitServiceProvider<XrmToolsPackage>>();
            return _logger = serviceProvider.GetRequiredService<ILogger<EntityCodeGenerator>>();
        }
    }

    public override string GetDefaultExtension() => ".cs";

    public EntityCodeGenerator()
    {
        var serviceProvider = VS.GetRequiredService<SToolkitServiceProvider<XrmToolsPackage>, IToolkitServiceProvider<XrmToolsPackage>>();
        EnvironmentProvider = serviceProvider.GetRequiredService<IEnvironmentProvider>();
        SchemaProviderFactory = serviceProvider.GetRequiredService<IXrmSchemaProviderFactory>();
    }

    protected override byte[]? GenerateCode(string inputFileName, string inputFileContent)
    {
        if (string.IsNullOrWhiteSpace(inputFileContent)) { return null; }
        if (Generator is null) { return Encoding.UTF8.GetBytes("// No generator found."); }
        if (GetTemplateFilePath() is not string templateFilePath) { return Encoding.UTF8.GetBytes("// Template not found."); ; }

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
            Logger.LogError(ex, Resources.Strings.PluginGenerator_DeserializationError, inputFileName);
        }
        return null;
    }

    private string? GetTemplateFilePath()
    {
        var templateFilePath = InputFilePath + ".sbn";
        if (File.Exists(templateFilePath))
        {
            return templateFilePath;
        }
        templateFilePath = GetProjectProperty("EntityCodeGenTemplatePath");
        return !string.IsNullOrWhiteSpace(templateFilePath) && File.Exists(templateFilePath) ? templateFilePath : null;
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
                return defaultNamespace as string;
            }
        }
        return string.Empty;
    }

    private string? GetProjectProperty(string propertyName)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        return GetService(typeof(IVsHierarchy)) is IVsHierarchy hierarchy ? hierarchy.GetProjectProperty(propertyName) : null;
    }

    private void AddEntityMetadataToEntityConfig(XrmCodeGenConfig entityCodeGenConfig) 
        => entityCodeGenConfig.EntityMetadatas = entityCodeGenConfig.Entities
            .Where(IsValidEntityConfig)
            .Select(e => GetEntityMetadata(e.LogicalName, e.AttributeNames.Split(',')))
            .ToList();

    private EntityMetadata? GetEntityMetadata(string logicalName, string[] attributes)
    {
        var environment = EnvironmentProvider?.GetActiveEnvironmentAsync().WaitAndUnwrapException();
        if (environment is null || !environment.IsValid) return null;
        var schemaProvider = SchemaProviderFactory?.Get(environment);
        if (schemaProvider is null) return null;
        // Make a new cancellation token for 2 minutes.
        using var cts = new CancellationTokenSource(120000);
        var metadata = schemaProvider?.GetEntityAsync(logicalName, cts.Token).WaitAndUnwrapException();
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
    ~EntityCodeGenerator() => Dispose(false);
    #endregion
}
#nullable restore