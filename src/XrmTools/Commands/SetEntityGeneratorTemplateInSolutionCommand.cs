namespace XrmTools.Commands;
using Community.VisualStudio.Toolkit;
using EnvDTE;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using System.ComponentModel.Composition;
using System.IO;
using XrmTools.Settings;
using Task = System.Threading.Tasks.Task;

/// <summary>
/// Command handler to set the selected file as template for entity code generation.
/// </summary>
[Command(PackageGuids.XrmToolsCmdSetIdString, PackageIds.SetEntityGeneratorTemplateInSolutionCmdId)]
internal sealed class SetEntityGeneratorTemplateInSolutionCommand : BaseCommand<SetEntityGeneratorTemplateInSolutionCommand>
{
    [Import]
    public ISettingsProvider SettingsProvider { get; set; }

    protected override async Task InitializeCompletedAsync()
    {
        Command.Supported = false;
        var componentModel = await Package.GetServiceAsync<SComponentModel, IComponentModel>();
        componentModel?.DefaultCompositionService.SatisfyImportsOnce(this);
    }

    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
    {
        var item = await VS.Solutions.GetActiveItemAsync();
        if (item is null || item.Type != SolutionItemType.PhysicalFile) return;

        var solution = await VS.Solutions.GetCurrentSolutionAsync();
        if (solution is null) return;

        var solutionDir = Path.GetDirectoryName(solution.FullPath);
        var path = item.FullPath.StartsWith(solutionDir) ? item.FullPath[solutionDir.Length..] : item.FullPath;
        SettingsProvider.SolutionSettings.EntityTemplateFilePath(path);
    }
}