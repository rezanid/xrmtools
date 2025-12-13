#nullable enable
namespace XrmTools;
using Microsoft.VisualStudio.Shell;
using System.ComponentModel.Composition;
using System.IO;
using XrmTools.Logging.Compatibility;
using Community.VisualStudio.Toolkit;
using System.Threading.Tasks;
using System.Reflection;
using XrmTools.CodeGen;
using System.Linq;

public interface ITemplateFileGenerator
{
    Task GenerateTemplatesAsync(bool overwrite = false);
    Task GenerateTemplatesAsync(Project project, bool overwrite = false);
    Task GenerateTemplatesAsync(Solution solution, bool overwrite = false);
    void GenerateTemplate(PhysicalFile file, bool overwrite = false);
}

[Export(typeof(ITemplateFileGenerator))]
[method: ImportingConstructor]
public class TemplateFileGenerator(ILogger<TemplateFileGenerator> logger) : ITemplateFileGenerator
{
    public async Task GenerateTemplatesAsync(bool overwrite = false)
    {
        var proj = await VS.Solutions.GetActiveProjectAsync();
        if (proj == null)
        {
            logger.LogWarning("Unable to find the active project. Consequently default templates won't be generated.");
            return;
        }
        await GenerateTemplatesAsync(proj, overwrite);
    }

    public async Task GenerateTemplatesAsync(Project project, bool overwrite = false)
    {
        var projDirectory = Path.GetDirectoryName(project.FullPath);
        var templatesDirectory = Path.Combine(projDirectory, Constants.ScribanTemplatesFolderName);
        var templateSourceDirectory = Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
            Constants.ScribanTemplatesFolderName);
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
    }

    public async Task GenerateTemplatesAsync(Solution solution, bool overwrite = false)
    {
        var solutionDirectory = Path.GetDirectoryName(solution.FullPath);
        var templatesDirectory = Path.Combine(solutionDirectory, Constants.ScribanTemplatesFolderName);
        var templateSourceDirectory = Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
            Constants.ScribanTemplatesFolderName);
        Directory.CreateDirectory(templatesDirectory);
        SolutionFolder? templateSolutionFolder = solution.Children.FirstOrDefault(i => i?.Name == Constants.ScribanTemplatesFolderName && i?.Type == SolutionItemType.SolutionFolder) as SolutionFolder;
        templateSolutionFolder ??= await solution.AddSolutionFolderAsync(Constants.ScribanTemplatesFolderName);
        foreach (var sourceFile in Directory.EnumerateFiles(templateSourceDirectory, $"*.{Constants.ScribanTemplateExtension}"))
        {
            var targetFile = Path.Combine(templatesDirectory, Path.GetFileName(sourceFile));
            var existedBefore = File.Exists(targetFile);
            if (!existedBefore || overwrite)
            {
                File.Copy(sourceFile, targetFile, overwrite);
            }
            templateSolutionFolder?.AddExistingFilesAsync(targetFile);
        }
    }

    public void GenerateTemplate(PhysicalFile file, bool overwrite = false)
    {
        var templateSourceDirectory = Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
            Constants.ScribanTemplatesFolderName);

        var searchResult = Directory.GetFiles(templateSourceDirectory, Path.GetFileName(file.FullPath), SearchOption.TopDirectoryOnly);

        if (searchResult.Length == 0) return;

        var sourceFile = searchResult[0];

        var targetFile = file.FullPath;
        var existedBefore = File.Exists(targetFile);
        if (!existedBefore || overwrite)
        {
            File.Copy(sourceFile, targetFile, overwrite);
        }
    }
}
#nullable restore