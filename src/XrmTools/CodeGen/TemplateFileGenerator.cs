#nullable enable
namespace XrmTools;
using Microsoft.VisualStudio.Shell;
using System.ComponentModel.Composition;
using System.IO;
using XrmTools.Logging.Compatibility;
using XrmTools.Settings;
using Community.VisualStudio.Toolkit;
using System.Threading.Tasks;
using System.Reflection;
using XrmTools.CodeGen;

public interface ITemplateFileGenerator
{
    Task GenerateTemplatesAsync();
}

[Export(typeof(ITemplateFileGenerator))]
[method: ImportingConstructor]
public class TemplateFileGenerator(ISettingsProvider settings, ILogger<TemplateFileGenerator> logger) : ITemplateFileGenerator
{
    public async Task GenerateTemplatesAsync()
    {
        var proj = await VS.Solutions.GetActiveProjectAsync();
        if (proj == null)
        {
            logger.LogWarning("Unable to find the acttive project. Consequently default templates won't be generated.");
            return;
        }
        var projDirectory = Path.GetDirectoryName(proj.FullPath);
        var templatesDirectory = Path.Combine(projDirectory, "CodeGenTemplates");
        var templateSourceDirectory = Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
            "CodeGenTemplates");

        Directory.CreateDirectory(templatesDirectory);
        foreach (var sourceFile in Directory.EnumerateFiles(templateSourceDirectory, $"*.{Constants.ScribanTemplateExtension}"))
        {
            var targetFile = Path.Combine(templatesDirectory, Path.GetFileName(sourceFile));
            if (!File.Exists(targetFile))
            {
                File.Copy(sourceFile, targetFile);
                await proj.AddExistingFilesAsync(targetFile);
            }
        }
        await settings.ProjectSettings.EntityTemplateFilePathAsync(Path.Combine(templatesDirectory, Constants.ScribanEntityTemplateFileName));
        await settings.ProjectSettings.PluginTemplateFilePathAsync(Path.Combine(templatesDirectory, Constants.ScribanPluginTemplateFileName));
    }
}
#nullable restore