#nullable enable
namespace XrmGen;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextTemplating.VSHost;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using XrmGen._Core;
using XrmGen.Extensions;
using XrmGen.Xrm;
using XrmGen.Xrm.Generators;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

public class EntityCodeGenerator : BaseCodeGeneratorWithSite
{
    public const string Name = "XrmGen Entity Generator";
    public const string Description = "Generates entity classes from metadata";

    private bool disposed = false;
    private IXrmEntityCodeGenerator? _generator;
    private IXrmSchemaProviderFactory? _schemaProviderFactory;

    [Import]
    private IXrmEntityCodeGenerator? Generator
    {
        // MEF does not work, so this is a workaround.
        get => _generator ??= GlobalServiceProvider.GetService(typeof(IXrmEntityCodeGenerator)) as IXrmEntityCodeGenerator;
        set => _generator = value;
    }

    [Import]
    private IXrmSchemaProviderFactory? SchemaProviderFactory
    {
        // MEF does not work, so this is a workaround.
        get => _schemaProviderFactory ??= GlobalServiceProvider.GetService(typeof(IXrmSchemaProviderFactory)) as IXrmSchemaProviderFactory;
        set => _schemaProviderFactory = value;
    }

    public override string GetDefaultExtension() => ".cs";

    protected override byte[]? GenerateCode(string inputFileName, string inputFileContent)
    {
        if (string.IsNullOrWhiteSpace(inputFileContent)) { return null; }
        if (Generator is null) { return Encoding.UTF8.GetBytes("// No generator found."); }
        if (GetTemplateFilePath() is not string templateFilePath) { return Encoding.UTF8.GetBytes("// Template not found."); ; }

        XrmCodeGenConfig? entityDefinitions = null;
        try
        {
            entityDefinitions = ParseConfig(inputFileContent);
        }
        catch (Exception ex)
        {
            Logger.Log(string.Format(Resources.Strings.PluginGenerator_DeserializationError, inputFileName));
            Logger.Log(ex.ToString());
        }
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

    private XrmCodeGenConfig ParseConfig(string inputFileContent)
    {
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        return deserializer.Deserialize<XrmCodeGenConfig>(inputFileContent);
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
        var environmentUrl = GetProjectProperty("EnvironmentUrl");
        if (string.IsNullOrWhiteSpace(environmentUrl)) { return null; }
        var schemaProvider = SchemaProviderFactory?.Get(environmentUrl!);
        var metadata = schemaProvider?.GetEntity(logicalName);
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
