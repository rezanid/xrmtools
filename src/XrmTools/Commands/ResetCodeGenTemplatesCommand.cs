#nullable enable
namespace XrmTools.Commands;

using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using XrmTools.CodeGen;

[Command(PackageGuids.XrmToolsCmdSetIdString, PackageIds.ResetCodeGenTemplatesCmdId)]
internal sealed class ResetCodeGenTemplatesCommand : BaseCommand<ResetCodeGenTemplatesCommand>
{
    private enum ResetCodeGenModes
    {
        Disabled,
        CustomizeTemplates,
        ResetTemplates
    }

    private ResetCodeGenModes _CommandMode = ResetCodeGenModes.Disabled;

    [Import]
    internal ITemplateFileGenerator TemplateGenerator { get; set; } = null!;

    protected override async Task InitializeCompletedAsync()
    {
        Command.Supported = false;
        var componentModel = await Package.GetServiceAsync<SComponentModel, IComponentModel>().ConfigureAwait(false);
        componentModel?.DefaultCompositionService.SatisfyImportsOnce(this);
        // This is to allow the command to accept arguments:
        Command.ParametersDescription = "$";
    }

    protected override void BeforeQueryStatus(EventArgs e)
    {
        ThreadHelper.JoinableTaskFactory.Run(async () =>
        {
            Command.Supported = true;
            _CommandMode = await DetectCommandModeAsync(await VS.Solutions.GetActiveItemAsync()).ConfigureAwait(false);
            Command.Visible = _CommandMode != ResetCodeGenModes.Disabled;
            Command.Text = _CommandMode == ResetCodeGenModes.ResetTemplates ? "Reset Code Generation Templates" : "Customize Code Generation Template";
        });
    }

    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
    {
        var argPath = e?.InValue as string;

        var targetItems = await ResolveTargetItemsAsync(argPath);
        if (targetItems is null || targetItems.Length == 0)
        {
            await VS.MessageBox.ShowErrorAsync(
                Vsix.Name,
                "No target project or solution or template found. Select a project or solution or a template in Solution Explorer.");
            return;
        }

        if (targetItems[0] is Solution solution)
        {
            if (_CommandMode == ResetCodeGenModes.ResetTemplates)
            {
                var confirmed = await VS.MessageBox.ShowConfirmAsync(
                    "Reset Code Generation Templates",
                    "This will overwrite existing code generation templates in the solution folder of " +
                    solution.Name + Path.DirectorySeparatorChar + Constants.ScribanTemplatesFolderName +
                    ". Do you want to continue?");
                if (!confirmed) return;
            }
            await TemplateGenerator.GenerateTemplatesAsync(solution, overwrite: true);
            return;
        }

        if (targetItems[0] is Project project)
        {
            if (_CommandMode == ResetCodeGenModes.ResetTemplates)
            {
                var confirmed = await VS.MessageBox.ShowConfirmAsync(
                "Reset Code Generation Templates",
                "This will overwrite existing code generation templates in the project folder of " +
                project.Name + Path.DirectorySeparatorChar + Constants.ScribanTemplatesFolderName +
                ". Do you want to continue?");
                if (!confirmed) return;
            }
            await TemplateGenerator.GenerateTemplatesAsync(project, overwrite: true);
            return;
        }

        if (targetItems[0].Type == SolutionItemType.PhysicalFile)
        {
            var templateSourceDirectory = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                Constants.ScribanTemplatesFolderName);

            // This is not a default template!
            var filesToReset = new List<PhysicalFile>(targetItems.Length);
            var fileNames = new StringBuilder();
            foreach (var item in targetItems)
            {
                if (item is PhysicalFile file)
                {
                    var searchResult = Directory.GetFiles(templateSourceDirectory, Path.GetFileName(file.FullPath), SearchOption.TopDirectoryOnly);
                    if (searchResult.Length > 0)
                    {
                        filesToReset.Add(file);
                        fileNames.Append(file.Text).Append(", ");
                    }
                }

            }
            if (filesToReset.Count == 0) return;

            var confirmed = await VS.MessageBox.ShowConfirmAsync(
                "Reset Code Generation Template",
                "This will overwrite " + filesToReset.Count + " existing code generation template: " +
                fileNames.ToString(0, fileNames.Length - 2) +
                ". Do you want to continue?");
            if (!confirmed) return;
            foreach (var file in filesToReset)
            {
                TemplateGenerator.GenerateTemplate(file, overwrite: true);
            }
        }
    }

    private async Task<SolutionItem[]> ResolveTargetItemsAsync(string? nameOrPath)
    {
        if (string.IsNullOrWhiteSpace(nameOrPath))
        {
            var items = await VS.Solutions.GetActiveItemsAsync();
            if (items is null || !items.Any()) return [];
            var firstItem = items.First();
            if (firstItem.Type is SolutionItemType.SolutionFolder && firstItem.Name == Constants.ScribanTemplatesFolderName)
            {
                var parent = firstItem.FindParent(SolutionItemType.Solution);
                if (parent is not null) return [parent];
            }
            if (firstItem.Type is SolutionItemType.PhysicalFolder && firstItem.Name == Constants.ScribanTemplatesFolderName)
            {
                var parent = firstItem.FindParent(SolutionItemType.Project);
                if (parent is not null) return [parent];
            }
            if (firstItem.Type is SolutionItemType.Solution or SolutionItemType.Project)
            {
                return [firstItem];
            }
            if (firstItem.Type is SolutionItemType.PhysicalFile)
            {
                return [.. items.Where(i => i.Type is SolutionItemType.PhysicalFile && string.Equals(".sbncs", Path.GetExtension(i.FullPath), StringComparison.OrdinalIgnoreCase))];
            }
            return [];
        }
        var sanitized = nameOrPath!.Trim().Trim('"');
        var extension = Path.GetExtension(sanitized);
        SolutionItem? itemFound = null;
        if (string.Equals(".csproj", extension, StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(extension))
        {
            itemFound = await FindProjectByFullPathOrNameAsync(sanitized);
        }
        if (string.Equals(".sln", extension, StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(extension))
        {
            itemFound = await FindSolutionByFullPathOrNameAsync(sanitized);
        }
        if (nameOrPath.EndsWith(Constants.ScribanTemplatesFolderName))
        {
            var items = await PhysicalFile.FromFilesAsync(sanitized);
            itemFound = items?.FirstOrDefault();
        }
        if (itemFound is not null)
        {
            _CommandMode = await DetectCommandModeAsync(itemFound);
            return [itemFound];
        }
        return [];
    }

    private static async Task<Project?> FindProjectByFullPathOrNameAsync(string nameOrPath)
    {
        var projects = await VS.Solutions.GetAllProjectsAsync();
        return projects.FirstOrDefault(
            p => PathUtil.GetCommonPathPrefix(p.FullPath, nameOrPath) == p.FullPath
            || string.Equals(p.Name, nameOrPath, StringComparison.OrdinalIgnoreCase));
    }

    private static async Task<Solution?> FindSolutionByFullPathOrNameAsync(string nameOrPath)
    {
        var solution = await VS.Solutions.GetCurrentSolutionAsync();
        if (solution is not null
            && (PathUtil.GetCommonPathPrefix(solution.FullPath, nameOrPath) == solution.FullPath
                || string.Equals(solution.Name, nameOrPath, StringComparison.OrdinalIgnoreCase)))
        {
            return solution;
        }
        return null;

    }

    private static async Task<ResetCodeGenModes> DetectCommandModeAsync(SolutionItem? item)
    {
        if (item is null)
            return ResetCodeGenModes.Disabled;

        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        if (!KnownUIContexts.SolutionHasSingleProjectContext.IsActive && !KnownUIContexts.SolutionHasMultipleProjectsContext.IsActive)
            return ResetCodeGenModes.Disabled;

        var proj = await VS.Solutions.GetActiveProjectAsync();
        if (!proj?.IsCapabilityMatch("CSharp") ?? false)
        return ResetCodeGenModes.Disabled;

        if (item.Type == SolutionItemType.Solution || item.Type == SolutionItemType.Project)
        {
            // Check if the target already has the templates folder
            var targetDirectory = item.Type == SolutionItemType.Solution
                ? Path.GetDirectoryName(((Solution)item).FullPath)
                : Path.GetDirectoryName(((Project)item).FullPath);
            var templatesDirectory = Path.Combine(targetDirectory!, Constants.ScribanTemplatesFolderName);
            if (Directory.Exists(templatesDirectory) && Directory.EnumerateFiles(templatesDirectory, $"*.{Constants.ScribanTemplateExtension}").Any())
            {
                return ResetCodeGenModes.ResetTemplates;
            }
            else
            {
                return ResetCodeGenModes.CustomizeTemplates;
            }
        }

        if (item.Type == SolutionItemType.PhysicalFile && item is PhysicalFile file && string.Equals(".sbncs", Path.GetExtension(file.FullPath), StringComparison.OrdinalIgnoreCase))
        {
            var templateSourceDirectory = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                Constants.ScribanTemplatesFolderName);

            var searchResult = Directory.GetFiles(templateSourceDirectory, Path.GetFileName(file.FullPath), SearchOption.TopDirectoryOnly);
            // This is not a default template!
            return (searchResult.Length != 0) ? ResetCodeGenModes.ResetTemplates : ResetCodeGenModes.Disabled;
        }
        return ResetCodeGenModes.Disabled;
    }
}
#nullable restore