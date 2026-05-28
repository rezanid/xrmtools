#nullable enable
namespace XrmTools;

using Community.VisualStudio.Toolkit;
using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using XrmTools.CodeGen;
using XrmTools.Logging.Compatibility;
using XrmTools.Settings;

public interface ITemplateFinder
{
    Task<string?> FindEntityTemplatePathAsync(string inputFile, string? projectDirectory = null, string? solutionDirectory = null);
    Task<string?> FindPluginTemplatePathAsync(string inputFile, string? projectDirectory = null, string? solutionDirectory = null);
    Task<string?> FindGlobalOptionSetsTemplatePathAsync(string? projectDirectory = null, string? solutionDirectory = null);
    Task<string?> FindFetchXmlTemplatePathAsync(string inputFile, string? projectDirectory = null, string? solutionDirectory = null);
}

[Export(typeof(ITemplateFinder))]
[method: ImportingConstructor]
public class TemplatePathFinder(ISettingsProvider settings, ILogger<TemplatePathFinder> logger) : ITemplateFinder
{
    private static bool FileExists(string? path) => !string.IsNullOrWhiteSpace(path) && File.Exists(path);

    private async Task<string?> ResolveTemplatePathAsync(
        string? explicitCandidatePath,
        Func<Task<string?>> settingsPathGetter,
        string templateFileName,
        string notFoundWarning,
        string? projectDirectory = null,
        string? solutionDirectory = null)
    {
        if (FileExists(explicitCandidatePath))
        {
            return explicitCandidatePath!;
        }

        var configuredPath = await settingsPathGetter();
        if (FileExists(configuredPath))
        {
            return configuredPath!;
        }

        if (string.IsNullOrWhiteSpace(projectDirectory))
        {
            var project = await VS.Solutions.GetActiveProjectAsync();
            if (project is not null)
            {
                projectDirectory = Path.GetDirectoryName(project.FullPath);
            }
        }

        if (!string.IsNullOrWhiteSpace(projectDirectory))
        {
            var path = Path.Combine(projectDirectory, Constants.ScribanTemplatesFolderName, templateFileName);
            if (File.Exists(path))
            {
                return path;
            }
        }

        if (string.IsNullOrWhiteSpace(solutionDirectory))
        {
            var solution = await VS.Solutions.GetCurrentSolutionAsync();
            if (solution is not null)
            {
                solutionDirectory = Path.GetDirectoryName(solution.FullPath);
            }
        }

        if (!string.IsNullOrWhiteSpace(solutionDirectory))
        {
            var path = Path.Combine(solutionDirectory, Constants.ScribanTemplatesFolderName, templateFileName);
            if (File.Exists(path))
            {
                return path;
            }
        }

        var templateSourceDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Constants.ScribanTemplatesFolderName);
        var fallback = Path.Combine(templateSourceDirectory, templateFileName);
        if (File.Exists(fallback))
        {
            // Remove settings from previous versions if any
            if (templateFileName.StartsWith(Constants.ScribanPluginTemplateFileName))
            {
                await settings.DeletePluginTemplateFilePathSettingAsync();
            }
            else if (templateFileName.StartsWith(Constants.ScribanEntityTemplateFileName))
            {
                await settings.DeleteEntityTemplateFilePathSettingAsync();
            }
            else
            {
                // Nothing to clean up for global option sets template.
            }

            return fallback;
        }

        logger.LogWarning(notFoundWarning);
        return null;
    }

    public async Task<string?> FindEntityTemplatePathAsync(string inputFile, string? projectDirectory = null, string? solutionDirectory = null)
    {
        var candidate = Path.Combine(Path.GetDirectoryName(inputFile), Path.GetFileNameWithoutExtension(inputFile)) + Constants.ScribanTemplateExtensionWithDot;

        return await ResolveTemplatePathAsync(
            explicitCandidatePath: candidate,
            settingsPathGetter: settings.EntityTemplateFilePathAsync,
            templateFileName: Constants.ScribanEntityTemplateFileName,
            notFoundWarning: "Failed to find any template for entity code generation.",
            projectDirectory: projectDirectory,
            solutionDirectory: solutionDirectory);
    }

    public async Task<string?> FindPluginTemplatePathAsync(string inputFile, string? projectDirectory = null, string? solutionDirectory = null)
    {
        var candidate = Path.Combine(Path.GetDirectoryName(inputFile), Path.GetFileNameWithoutExtension(inputFile)) + Constants.ScribanTemplateExtensionWithDot;

        return await ResolveTemplatePathAsync(
            explicitCandidatePath: candidate,
            settingsPathGetter: settings.PluginTemplateFilePathAsync,
            templateFileName: Constants.ScribanPluginTemplateFileName,
            notFoundWarning: "Failed to find any template for plugin code generation.",
            projectDirectory: projectDirectory,
            solutionDirectory: solutionDirectory);
    }

    public async Task<string?> FindGlobalOptionSetsTemplatePathAsync(string? projectDirectory = null, string? solutionDirectory = null)
    {
        return await ResolveTemplatePathAsync(
            explicitCandidatePath: null,
            settingsPathGetter: settings.GlobalOptionSetsTemplateFilePathAsync,
            templateFileName: Constants.ScribanGlobalOptionSetsFileName,
            notFoundWarning: "Failed to find any template for global option sets code generation.",
            projectDirectory: projectDirectory,
            solutionDirectory: solutionDirectory);
    }

    public async Task<string?> FindFetchXmlTemplatePathAsync(string inputFile, string? projectDirectory = null, string? solutionDirectory = null)
    {
        var candidate = Path.Combine(Path.GetDirectoryName(inputFile), Path.GetFileNameWithoutExtension(inputFile)) + Constants.ScribanTemplateExtensionWithDot;

        return await ResolveTemplatePathAsync(
            explicitCandidatePath: candidate,
            settingsPathGetter: settings.GlobalOptionSetsTemplateFilePathAsync,
            templateFileName: Constants.ScribanFetchXmlFileName,
            notFoundWarning: "Failed to find any template for FetchXML code generation.",
            projectDirectory: projectDirectory,
            solutionDirectory: solutionDirectory);
    }
}
#nullable restore