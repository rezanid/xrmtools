using Microsoft.Crm.Sdk.Messages;
using Microsoft.VisualStudio.TextTemplating.VSHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using XrmGen.Extensions;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace XrmGen;

public class PluginGenerator : BaseCodeGeneratorWithSite
{
    public const string Name = "XrmGen Plugin Generator";
    public const string Description = "Generates plugin classes from .dej.json file.";

    public override string GetDefaultExtension() => ".cs";
    protected override byte[] GenerateCode(string inputFileName, string inputFileContent)
    {
        var config = JsonSerializer.Deserialize<AssemblyRegistration>(inputFileContent);
        if (config?.PluginTypes is null) { return null; }
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("using AG.RM.Xrm.Utility;");
        stringBuilder.AppendLine("using AG.RM.Xrm.Utility.IAM;");
        stringBuilder.AppendLine("using AG.RM.Xrm.Utility.Json;");
        stringBuilder.AppendLine("using Microsoft.Xrm.Sdk;");
        stringBuilder.AppendLine();
        stringBuilder.AppendLine($"namespace {config.Name.FirstSegment()};");
        stringBuilder.AppendLine();
        foreach (var plugin in config.PluginTypes.Where(IsValidPluginRegistration))
        {
            foreach (var step in plugin.Steps)
            {
                stringBuilder.AppendLine($"public partial class {plugin.TypeName.LastSegment()} : PluginDefinition<{plugin.TypeName.LastSegment()}Manager>, IPlugin {{ }}");
                stringBuilder.AppendLine($"public partial class {plugin.TypeName.LastSegment()}Manager : {step.MessageName}PluginManager");
                stringBuilder.AppendLine("{");
                stringBuilder.AppendLine("}");
            }
        }
        return Encoding.UTF8.GetBytes(stringBuilder.ToString());
    }

    private bool IsValidPluginRegistration(PluginRegistration Registration) => !string.IsNullOrWhiteSpace(Registration.TypeName);
}

public class AssemblyRegistration { 

    public Guid Id { get; set; }
    public string Name { get; set; }
    public int IsolationMode { get; set; }
    public int SourceType { get; set; }
    public string Version { get; set; }
    public IEnumerable<PluginRegistration> PluginTypes { get; set; }
}

public class PluginRegistration {
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string UnsecureConfig { get; set; }
    public string SecureConfig { get; set; }
    public string TypeName { get; set; }
    public string FriendlyName { get; set; }
    public string GroupName { get; set; }

    public IEnumerable<PluginStepRegistration> Steps { get; set; }
}

public class PluginStepRegistration
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string UnsecureConfig { get; set; }
    public string SecureConfig { get; set; }
    public string MessageName { get; set; }
    public string PrimaryEntityName { get; set; }
    public bool AsyncAutoDelete { get; set; }
    public bool ImpersonatingUserFullName { get; set; }
    public string CustomConfiguration { get; set; }
    public string FilteringAttributes { get; set; }
    public int InvocationSource { get; set; }
    public int Mode { get; set; }
    public int Rank { get; set; }
    public int Stage { get; set; }
    public int SupportedDeployment { get; set; }
    public Guid SdkMessageId { get; set; }
    public int StateCode { get; set; }

    public IEnumerable<PluginStepImageRegistration> Images { get; set; }
}

public class PluginStepImageRegistration
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Attributes { get; set; }
    public string EntityAlias { get; set; }
    public string MessagePropertyName { get; set; }
    public int ImageType { get; set; }
}