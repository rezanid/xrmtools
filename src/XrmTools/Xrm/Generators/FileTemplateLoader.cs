#nullable enable
namespace XrmTools.Xrm.Generators;

using System;
using System.IO;
using System.Threading.Tasks;
using Scriban;
using Scriban.Parsing;
using Scriban.Runtime;

public class FileTemplateLoader(string baseDirectory) : ITemplateLoader
{
    public string GetPath(TemplateContext context, SourceSpan callerSpan, string templateName)
    {
        string templatePath = Path.Combine(baseDirectory, templateName);
        if (!templatePath.StartsWith(baseDirectory, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(string.Format(Resources.Strings.PluginGenerator_TemplateInvalidPath, templatePath));
        }
        return templatePath;
    }

    public string Load(TemplateContext context, SourceSpan callerSpan, string templatePath)
        => ValidateTemplatePath(templatePath) switch
        {
            (false, var message) => message,
            _ => File.ReadAllText(templatePath)
        };

    public async ValueTask<string> LoadAsync(TemplateContext context, SourceSpan callerSpan, string templatePath)
        => ValidateTemplatePath(templatePath) switch
        {

           (false, var message) => message,
            _ => await File.OpenText(templatePath).ReadToEndAsync().ConfigureAwait(false)
        };

    private (bool isValid, string message) ValidateTemplatePath(string templatePath)
    {
        if (!templatePath.StartsWith(baseDirectory, StringComparison.OrdinalIgnoreCase))
        {
            return (false, string.Format(Resources.Strings.PluginGenerator_TemplateInvalidPath, templatePath));
        }
        if (!File.Exists(templatePath))
        {
            return (false, string.Format(Resources.Strings.PluginGenerator_TemplatePathNotFound, templatePath));
        }
        return (true, string.Empty);
    }
}
#nullable restore