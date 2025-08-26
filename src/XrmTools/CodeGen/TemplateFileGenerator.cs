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
    Task GenerateTemplatesAsync(bool overwrite = false);
    Task GenerateTemplatesAsync(Project project, bool overwrite = false);
}

[Export(typeof(ITemplateFileGenerator))]
[method: ImportingConstructor]
public class TemplateFileGenerator(ISettingsProvider settings, ILogger<TemplateFileGenerator> logger) : ITemplateFileGenerator
{
    public async Task GenerateTemplatesAsync(bool overwrite = false)
    {
        var proj = await VS.Solutions.GetActiveProjectAsync();
        if (proj == null)
        {
            logger.LogWarning("Unable to find the acttive project. Consequently default templates won't be generated.");
            return;
        }
        await GenerateTemplatesAsync(proj, overwrite);
    }

    public async Task GenerateTemplatesAsync(Project project, bool overwrite = false)
    {
        var projDirectory = Path.GetDirectoryName(project.FullPath);
        var templatesDirectory = Path.Combine(projDirectory, "CodeGenTemplates");
        var templateSourceDirectory = Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
            "CodeGenTemplates");
        Directory.CreateDirectory(templatesDirectory);
        foreach (var sourceFile in Directory.EnumerateFiles(templateSourceDirectory, $"*.{Constants.ScribanTemplateExtension}"))
        {
            var targetFile = Path.Combine(templatesDirectory, Path.GetFileName(sourceFile));
            var existedBefore = File.Exists(targetFile);
            if (!existedBefore || overwrite)
            {
                File.Copy(sourceFile, targetFile, overwrite);
            }
            if (!existedBefore)
            {
                await project.AddExistingFilesAsync(targetFile);
            }
        }
        await settings.ProjectSettings.EntityTemplateFilePathAsync(Path.Combine(Path.DirectorySeparatorChar + "CodeGenTemplates", Constants.ScribanEntityTemplateFileName));
        await settings.ProjectSettings.PluginTemplateFilePathAsync(Path.Combine(Path.DirectorySeparatorChar + "CodeGenTemplates", Constants.ScribanPluginTemplateFileName));
    }
}
#nullable restore