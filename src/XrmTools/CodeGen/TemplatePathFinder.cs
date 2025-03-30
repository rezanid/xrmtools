#nullable enable
namespace XrmTools;
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Composition;
using System.IO;
using XrmTools.CodeGen;
using XrmTools.Logging.Compatibility;
using XrmTools.Settings;

public interface ITemplateFinder
{
    string? FindEntityTemplatePath(string inputFile);
    string? FindPluginTemplatePath(string inputFile);
}

[Export(typeof(ITemplateFinder))]
[method: ImportingConstructor]
public class TemplatePathFinder(ISettingsProvider settings, ILogger<TemplatePathFinder> logger) : ITemplateFinder
{
    private readonly ISettingsProvider settings = settings ?? throw new ArgumentNullException(nameof(settings));

    public string? FindEntityTemplatePath(string inputFile)
    {
        var templateFilePath = inputFile + Constants.ScribanTemplateExtensionWithDot;
        if (File.Exists(templateFilePath))
        {
            return templateFilePath;
        }

        templateFilePath = ThreadHelper.JoinableTaskFactory.Run(settings.EntityTemplateFilePathAsync);
        if (!string.IsNullOrWhiteSpace(templateFilePath) && File.Exists(templateFilePath))
        {
            return templateFilePath!;
        }

        logger.LogWarning("Failed to find any template for entity code generation. Consequently default entity generation template will be crearted.");

        return null;
    }

    public string? FindPluginTemplatePath(string inputFile)
    {
        var templateFilePath = inputFile + Constants.ScribanTemplateExtensionWithDot;
        if (File.Exists(templateFilePath))
        {
            return templateFilePath;
        }

        templateFilePath = ThreadHelper.JoinableTaskFactory.Run(settings.PluginTemplateFilePathAsync);
        if (!string.IsNullOrWhiteSpace(templateFilePath) && File.Exists(templateFilePath))
        {
            return templateFilePath!;
        }

        logger.LogWarning("Failed to find any template for plugin code generation. Consequently default plugin generation template will be crearted.");

        return null;
    }
}

#nullable restore