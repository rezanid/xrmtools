using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.TextTemplating.VSHost;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using XrmGen.Xrm.Generators;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using XrmGen.Xrm;
using XrmGen.Extensions;

namespace XrmGen;

public class EntityGenerator : BaseCodeGeneratorWithSite
{
    public const string Name = "XrmGen Entity Generator";
    public const string Description = "Generates entity classes from metadata";

    private IVsRunningDocumentTable _RunningDocumenTable;

    [ImportMany]
    IEnumerable<IEntityGenerator> Generators { get; set; }

    [Import]
    IXrmSchemaProviderFactory SchemaProviderFactory { get; set; }

    private IVsRunningDocumentTable RunningDocumenTable
    {
        get => _RunningDocumenTable ??= GetService(typeof(SVsRunningDocumentTable)) as IVsRunningDocumentTable;
    }

    public override string GetDefaultExtension() => ".cs";

    protected override byte[] GenerateCode(string inputFileName, string inputFileContent)
    {
        var parser = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        var config = parser.Deserialize<CodeGenConfig>(inputFileContent);
        if (config?.Entities is null) { return null; }


        // TODO: For now, we are using the first generator that is available. We should allow the user to select the generator.
        var generator = Generators.FirstOrDefault();
        if (generator is null) { return Encoding.UTF8.GetBytes("// No generator found."); }

        // Fetch metadata from SchemaProvider according to the configuration.
        string environmentUrl = GetProjectProperty(inputFileName, "EnvironmentUrl");
        string applicationId = GetProjectProperty(inputFileName, "ApplicationId");
        var schemaProvider = SchemaProviderFactory.Get(environmentUrl, applicationId);
        var entityMetadatas = config.Entities
            .Where(IsValidEntityConfig)
            .Select(e => schemaProvider.GetEntity(e.LogicalName)).ToList();

        var sb = new StringBuilder();
        foreach (var entityMetadata in entityMetadatas)
        {
            var (isValid, validationMessage) = generator.IsValid(entityMetadata);
            if (!isValid)
            {
                sb.Append("// ");
                sb.AppendLine(validationMessage);
                continue;
            }
            generator.GenerateCode(sb, entityMetadata, GetDefaultNamespace());
        }
        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private bool IsValidEntityConfig(EntityConfig entityConfig) => !string.IsNullOrWhiteSpace(entityConfig.LogicalName) && !string.IsNullOrWhiteSpace(entityConfig.AttributeNames);

    private string GetDefaultNamespace()
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        if (GetService(typeof(IVsHierarchy)) is IVsHierarchy hierarchy)
        {
            // Get the current item ID
            hierarchy.ParseCanonicalName(InputFilePath, out var itemId);

            if (hierarchy.GetProperty(itemId, (int)__VSHPROPID.VSHPROPID_DefaultNamespace, out object defaultNamespace) == VSConstants.S_OK)
            {
                return defaultNamespace as string;
            }
        }

        return null;
    }

    private string GetProjectProperty(string filePath, string propertyName) => RunningDocumenTable.TryGetHierarchyAndItemID(filePath, out var hierarchy, out _)
        ? hierarchy.GetPropertyForProject(propertyName)
        : null;
}


