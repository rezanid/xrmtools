#nullable enable
using Microsoft.Xrm.Sdk.Metadata;
using Scriban.Runtime;
using Scriban;
using System;
using System.ComponentModel.Composition;
using System.Text;
using XrmTools.Xrm.Model;
using System.IO;

namespace XrmTools.Xrm.Generators;

[Export(typeof(IXrmEntityCodeGenerator))]
public class TemplatedEntityCodeGenerator : IXrmEntityCodeGenerator
{
    public XrmCodeGenConfig? Config { set; get; }

    private static readonly Type generatorType = typeof(TemplatedEntityCodeGenerator);
    private static readonly string generatorAttributeString = $"[GeneratedCode(\"{generatorType.Name}\", \"{generatorType.Assembly.GetName().Version}\")]";

    public (bool, string) IsValid(XrmCodeGenConfig input)
    {
        if (Config == null) { throw new InvalidOperationException("Config is not set."); }
        if (input is null) { throw new ArgumentNullException(nameof(input)); }
        return (true, string.Empty);
    }

    public string GenerateCode(XrmCodeGenConfig input)
    {
        if (Config == null) { throw new InvalidOperationException("Config is not set."); }
        if (string.IsNullOrWhiteSpace(Config.TemplateFilePath)) { throw new InvalidOperationException(Resources.Strings.PluginGenerator_TemplatePathNotSet); }
        return GenerateCodeUsingScribanTemplate(input);
    }

    private string GenerateCodeUsingScribanTemplate(XrmCodeGenConfig input)
    {
        var scriptObject = new ScriptObject();
        scriptObject.Import(typeof(ScribanExtensions));
        scriptObject.Add(
            ScribanExtensionCache.KnownAssemblies.Humanizr.ToString().ToLowerInvariant(),
            ScribanExtensionCache.GetHumanizrMethods());
        scriptObject.Add("config", Config);
        scriptObject.Add("model", input);
        scriptObject.Add("codegen_attribute", generatorAttributeString);

        var context = new TemplateContext()
        {
            TemplateLoader = new FileTemplateLoader(Directory.GetParent(Config!.TemplateFilePath).FullName),
        };
        context.PushGlobal(scriptObject);

        var templateContent = File.ReadAllText(Config!.TemplateFilePath);
        var template = Template.Parse(templateContent, Config.TemplateFilePath)
            ?? throw new InvalidOperationException(
                string.Format(Resources.Strings.PluginGenerator_NullTemplate, Config.TemplateFilePath));
        if (template.HasErrors)
        {
            var errors = new StringBuilder();
            foreach (var error in template.Messages)
            {
                errors.AppendLine(error.ToString());
            }
            return string.Format(Resources.Strings.PluginGenerator_TemplateError, Config.TemplateFilePath, errors.ToString());
        }
        try
        {
            return template.Render(context);
        }
        catch (Exception ex)
        {
            return string.Format(Resources.Strings.PluginGenerator_TemplateError, Config.TemplateFilePath, ex);
        }
    }
}
#nullable restore