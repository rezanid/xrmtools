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
    Task<string?> FindEntityTemplatePathAsync(string inputFile);
    Task<string?> FindPluginTemplatePathAsync(string inputFile);
    Task<string?> FindGlobalOptionSetsTemplatePathAsync();
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
        string notFoundWarning)
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

        var activeProject = await VS.Solutions.GetActiveProjectAsync();
        if (activeProject is not null)
        {
            var projectDir = Path.GetDirectoryName(activeProject.FullPath);
            var path = Path.Combine(projectDir, Constants.ScribanTemplatesFolderName, templateFileName);
            if (File.Exists(path))
            {
                return path;
            }
        }

        var solution = await VS.Solutions.GetCurrentSolutionAsync();
        if (solution is not null)
        {
            var solutionDir = Path.GetDirectoryName(solution.FullPath);
            var path = Path.Combine(solutionDir, Constants.ScribanTemplatesFolderName, templateFileName);
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

    public async Task<string?> FindEntityTemplatePathAsync(string inputFile)
    {
        var candidate = Path.Combine(Path.GetDirectoryName(inputFile), Path.GetFileNameWithoutExtension(inputFile)) + Constants.ScribanTemplateExtensionWithDot;

        return await ResolveTemplatePathAsync(
            explicitCandidatePath: candidate,
            settingsPathGetter: settings.EntityTemplateFilePathAsync,
            templateFileName: Constants.ScribanEntityTemplateFileName,
            notFoundWarning: "Failed to find any template for entity code generation.");
    }

    public async Task<string?> FindPluginTemplatePathAsync(string inputFile)
    {
        var candidate = Path.Combine(Path.GetDirectoryName(inputFile), Path.GetFileNameWithoutExtension(inputFile)) + Constants.ScribanTemplateExtensionWithDot;

        return await ResolveTemplatePathAsync(
            explicitCandidatePath: candidate,
            settingsPathGetter: settings.PluginTemplateFilePathAsync,
            templateFileName: Constants.ScribanPluginTemplateFileName,
            notFoundWarning: "Failed to find any template for plugin code generation.");
    }

    public async Task<string?> FindGlobalOptionSetsTemplatePathAsync()
    {
        return await ResolveTemplatePathAsync(
            explicitCandidatePath: null,
            settingsPathGetter: settings.GlobalOptionSetsTemplateFilePathAsync,
            templateFileName: Constants.ScribanGlobalOptionSetsFileName,
            notFoundWarning: "Failed to find any template for global option sets code generation.");
    }
}
#nullable restore