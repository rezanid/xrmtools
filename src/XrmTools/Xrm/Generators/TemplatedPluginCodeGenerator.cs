#nullable enable
namespace XrmGen.Xrm.Generators;

using Scriban;
using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using XrmGen.Xrm.Model;
using Scriban.Runtime;
using System.Collections.Generic;

[Export(typeof(IXrmPluginCodeGenerator))]
public class TemplatedPluginCodeGenerator : IXrmPluginCodeGenerator
{
    public XrmCodeGenConfig? Config { set; get; }

    private static readonly Type generatorType = typeof(TemplatedPluginCodeGenerator);
    private static readonly string generatorAttributeString = $"[GeneratedCode(\"{generatorType.Name}\", \"{generatorType.Assembly.GetName().Version}\")]";

    public (bool, string) IsValid(PluginAssemblyConfig pluginAssembly)
    {
        if (Config == null) { throw new InvalidOperationException("Config is not set."); }
        if (pluginAssembly is null) { throw new ArgumentNullException(nameof(pluginAssembly)); }
        if (pluginAssembly.PluginTypes is null || !pluginAssembly.PluginTypes.Any()) { return (false, Resources.Strings.PluginGenerator_NoPluginTypes); }
        return (true, string.Empty);
    }

    public string GenerateCode(PluginAssemblyConfig plugin)
    {
        if (Config == null) { throw new InvalidOperationException("Config is not set."); }
        if (string.IsNullOrWhiteSpace(Config.TemplateFilePath)) { throw new InvalidOperationException(Resources.Strings.PluginGenerator_TemplatePathNotSet); }
        return GenerateCodeUsingScribanTemplate(plugin);
    }

    private string GenerateCodeUsingScribanTemplate(PluginAssemblyConfig config)
    {
        var scriptObject = new ScriptObject();
        scriptObject.Import(typeof(ScribanExtensions));
        scriptObject.Add(
            ScribanExtensionCache.KnownAssemblies.Humanizr.ToString().ToLowerInvariant(), 
            ScribanExtensionCache.GetHumanizrMethods());
        scriptObject.Add("config", Config);
        scriptObject.Add("model", config);
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
            return string.Format(Resources.Strings.PluginGenerator_TemplateError, Config, errors.ToString());
        }
        try
        {
            return template.Render(context);
        }
        catch (Exception ex)
        {
            return string.Format(Resources.Strings.PluginGenerator_TemplateError, Config, ex);
        }
    }
}
#nullable restore