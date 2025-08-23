#nullable enable
namespace XrmTools;
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading.Tasks;
using XrmTools.CodeGen;
using XrmTools.Logging.Compatibility;
using XrmTools.Settings;

public interface ITemplateFinder
{
    Task<string?> FindEntityTemplatePathAsync(string inputFile);
    Task<string?> FindPluginTemplatePathAsync(string inputFile);
    Task<string?> FindGlobalOptionSetsTemplatePathAsync();
}

[Export(typeof(ITemplateFinder))]
[method: ImportingConstructor]
public class TemplatePathFinder(ISettingsProvider settings, ILogger<TemplatePathFinder> logger) : ITemplateFinder
{
    private readonly ISettingsProvider settings = settings ?? throw new ArgumentNullException(nameof(settings));

    public async Task<string?> FindEntityTemplatePathAsync(string inputFile)
    {
        var templateFilePath = inputFile + Constants.ScribanTemplateExtensionWithDot;
        if (File.Exists(templateFilePath))
        {
            return templateFilePath;
        }

        templateFilePath = await settings.EntityTemplateFilePathAsync();
        if (!string.IsNullOrWhiteSpace(templateFilePath) && File.Exists(templateFilePath))
        {
            return templateFilePath!;
        }

        logger.LogWarning("Failed to find any template for entity code generation. Consequently default entity generation template will be crearted.");

        return null;
    }

    public async Task<string?> FindPluginTemplatePathAsync(string inputFile)
    {
        var templateFilePath = Path.Combine(Path.GetDirectoryName(inputFile), Path.GetFileNameWithoutExtension(inputFile)) + Constants.ScribanTemplateExtensionWithDot;
        if (File.Exists(templateFilePath))
        {
            return templateFilePath;
        }

        templateFilePath = await settings.PluginTemplateFilePathAsync();
        if (!string.IsNullOrWhiteSpace(templateFilePath) && File.Exists(templateFilePath))
        {
            return templateFilePath!;
        }

        logger.LogWarning("Failed to find any template for plugin code generation. Consequently default plugin generation template will be crearted.");

        return null;
    }

    public async Task<string?> FindGlobalOptionSetsTemplatePathAsync()
    {
        var templateFilePath = await settings.GlobalOptionSetsTemplateFilePathAsync();
        if (!string.IsNullOrWhiteSpace(templateFilePath) && File.Exists(templateFilePath))
        {
            return templateFilePath!;
        }

        logger.LogWarning("Failed to find any template for global option sets code generation. Consequently default global option sets generation template will be created.");

        return null;
    }

}

#nullable restore