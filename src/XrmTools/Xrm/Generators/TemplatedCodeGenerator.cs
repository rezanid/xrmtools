#nullable enable
namespace XrmTools.Xrm.Generators;

using Scriban;
using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Text;
using Scriban.Runtime;
using XrmTools.Validation;
using System.ComponentModel.DataAnnotations;
using XrmTools.Meta.Model.Configuration;
using XrmTools.FetchXml.Model;

[Export(typeof(IXrmCodeGenerator))]
[method:ImportingConstructor]
internal class TemplatedCodeGenerator(IValidationService validationService) : IXrmCodeGenerator
{
    public XrmCodeGenConfig? Config { set; get; }

    private static readonly Type generatorType = typeof(TemplatedCodeGenerator);
    private static readonly string generatorAttributeString = $"[GeneratedCode(\"{generatorType.Name}\", \"{generatorType.Assembly.GetName().Version}\")]";

    public ValidationResult IsValid(PluginAssemblyConfig inputModel)
    {
        if (Config == null) { throw new InvalidOperationException("Config is not set."); }
        return validationService.ValidateIfValidatorAvailable(inputModel, Categories.CodeGeneration);
    }

    public string GenerateCode(PluginAssemblyConfig inputModel)
    {
        if (Config == null) { throw new InvalidOperationException("Config is not set."); }
        if (string.IsNullOrWhiteSpace(Config.TemplateFilePath)) { throw new InvalidOperationException(Resources.Strings.CodeGen_TemplatePathNotSet); }
        return GenerateCodeUsingScribanTemplate(inputModel);
    }

    public string GenerateCode(FetchQuery inputModel)
    {
        if (Config == null) { throw new InvalidOperationException("Config is not set."); }
        if (string.IsNullOrWhiteSpace(Config.TemplateFilePath)) { throw new InvalidOperationException(Resources.Strings.CodeGen_TemplatePathNotSet); }
        return GenerateCodeUsingScribanTemplate(inputModel);
    }

    private string GenerateCodeUsingScribanTemplate(object inputModel)
    {
        var scriptObject = new ScriptObject();
        scriptObject.Import(typeof(ScribanExtensions));
        scriptObject.Add(
            ScribanExtensionCache.KnownAssemblies.Humanizr.ToString().ToLowerInvariant(), 
            ScribanExtensionCache.GetHumanizrMethods());
        scriptObject.Add("config", Config);
        scriptObject.Add("model", inputModel);
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