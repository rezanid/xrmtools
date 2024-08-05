#nullable enable
namespace XrmGen;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextTemplating.VSHost;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Windows.Documents;
using XrmGen._Core;
using XrmGen.Extensions;
using XrmGen.Xrm;
using XrmGen.Xrm.Generators;
using XrmGen.Xrm.Model;

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

        PluginAssemblyInfo? pluginDefinition = null;
        try
        {
            pluginDefinition = JsonSerializer.Deserialize<PluginAssemblyInfo>(inputFileContent);
        }
        catch (Exception ex)
        {
            Logger.Log(string.Format(Resources.Strings.PluginGenerator_DeserializationError, inputFileName));
            Logger.Log(ex.ToString());
        }
        if (pluginDefinition?.PluginTypes?.Any() != true) { return null; }

        Generator.Config = new XrmCodeGenConfig
        {
            //TODO: The GetDefaultNamespace is not required. The FileNamespace is never empty even when not set.
            DefaultNamespace = string.IsNullOrWhiteSpace(FileNamespace) ? GetDefaultNamespace() : FileNamespace,
            TemplateFilePath = templateFilePath
        };

        AddEntityMetadataToPluginDefinition(pluginDefinition!);

        var (isValid, validationMessage) = Generator.IsValid(pluginDefinition);
        if (!isValid)
        {
            return Encoding.UTF8.GetBytes(validationMessage);
        }
        return Encoding.UTF8.GetBytes(Generator.GenerateCode(pluginDefinition));
    }

    private string? GetTemplateFilePath()
    {
        var templateFilePath = InputFilePath + ".sbn";
        if (File.Exists(templateFilePath))
        {
            return templateFilePath;
        }
        templateFilePath = Path.GetFileNameWithoutExtension(InputFilePath) + ".sbn";
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

    private EntityMetadata? GetEntityMetadata(string logicalName, string[] attributes)
    {
        var environmentUrl = GetProjectProperty("EnvironmentUrl");
        if (string.IsNullOrWhiteSpace(environmentUrl)) { return null; }
        var schemaProvider = SchemaProviderFactory?.Get(environmentUrl!);
        var metadata = schemaProvider?.GetEntity(logicalName);
        if (metadata == null) { return null; }

        //NOTE!
        // Logical attributes generally don't have DisplayName.
        // We also filter them out to avoid unnecessary processing.
        var filteredAttributes = 
            attributes.Length == 0 ?
            metadata.Attributes.Where(a => a.IsLogical != true).ToArray() :
            metadata.Attributes.Where(a => !attributes.Any() || attributes.Contains(a.LogicalName)).ToArray();
        var propertyInfo = typeof(EntityMetadata).GetProperty("Attributes");

        //TODO: The cloning is done because we don't want to modify the object in the cache.
        //      In future when we load from local storage this might not be required.
        var clone = metadata.Clone();
        propertyInfo.SetValue(clone, filteredAttributes);
        return clone;
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

    private void AddEntityMetadataToPluginDefinition(PluginAssemblyInfo pluginDefinition)
    {
        var optionsetMetadataList = new List<OptionSetMetadata>();
        foreach (var step in pluginDefinition.PluginTypes.SelectMany(plugin => plugin.Steps))
        {
            if (!string.IsNullOrWhiteSpace(step.PrimaryEntityName))
            {
                var metadata = GetEntityMetadata(step.PrimaryEntityName!, step.FilteringAttributes?.Split(',') ?? []);
                step.PrimaryEntityMetadata = metadata;
                if (metadata is not null)
                {
                    optionsetMetadataList.AddRange(metadata.Attributes
                        .Where(a => (a.AttributeType == AttributeTypeCode.Picklist || a.AttributeType == AttributeTypeCode.Virtual) && a.IsLogical == false)
                        .Cast<EnumAttributeMetadata>()
                        .Select(a => a.OptionSet));
                }
            }

            foreach (var image in step.Images)
            {
                if (!string.IsNullOrWhiteSpace(image.EntityAlias))
                {
                    var metadata = GetEntityMetadata(step.PrimaryEntityName!, image.Attributes?.Split(',') ?? []);
                    image.MessagePropertyMetadata = metadata;
                    if (metadata is not null)
                    {
                        optionsetMetadataList.AddRange(metadata.Attributes
                            .Where(a => (a.AttributeType == AttributeTypeCode.Picklist || a.AttributeType == AttributeTypeCode.Virtual) && a.IsLogical == false)
                            .Cast<EnumAttributeMetadata>()
                            .Select(a => a.OptionSet));
                    }

                }
            }
        }
        pluginDefinition.OptionMetadatas = optionsetMetadataList.Distinct(OptionSetMetadataComparer).ToList();
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