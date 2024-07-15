using Microsoft.VisualStudio.TextTemplating.VSHost;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace XrmGen;

public class EntityGenerator : BaseCodeGeneratorWithSite
{
    public const string Name = "XrmGen Entity Generator";
    public const string Description = "Generates entity classes from metadata";

    public override string GetDefaultExtension() => ".cs";
    protected override byte[] GenerateCode(string inputFileName, string inputFileContent)
    {
        var parser = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        var config = parser.Deserialize<CodeGenConfig>(inputFileContent);
        if (config?.Entities is null) { return null; }
        var stringBuilder = new StringBuilder();
        foreach (var entity in config.Entities.Where(IsValidEntityConfig))
        {
            var attributeNames = entity.AttributeNames.Split(',');
            stringBuilder.AppendLine($"public class {entity.LogicalName}");
            stringBuilder.AppendLine("{");
            foreach (var attributeName in attributeNames)
            {
                stringBuilder.AppendLine($"    public string {attributeName} {{ get; set; }}");
            }
            stringBuilder.AppendLine("}");
        }
        return Encoding.UTF8.GetBytes(stringBuilder.ToString());
    }

    private bool IsValidEntityConfig(EntityConfig entityConfig) => !string.IsNullOrWhiteSpace(entityConfig.LogicalName) && !string.IsNullOrWhiteSpace(entityConfig.AttributeNames);
}

public class CodeGenConfig
{
    public string Environment { get; set; }
    public List<EntityConfig> Entities { get; set; }
}

public class EntityConfig
{
    [YamlMember(Alias = "entity")]
    public string LogicalName { get; set; }
    public string AttributeNames { get; set; }
}


