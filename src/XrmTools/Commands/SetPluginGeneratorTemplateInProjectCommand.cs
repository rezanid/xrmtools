namespace XrmTools.Commands;
using Community.VisualStudio.Toolkit;
using EnvDTE;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using System.ComponentModel.Composition;
using XrmTools.Settings;
using Task = System.Threading.Tasks.Task;

/// <summary>
/// Command handler to set the selected file as template for plugin code generation.
/// </summary>
[Command(PackageGuids.XrmToolsCmdSetIdString, PackageIds.SetPluginGeneratorTemplateInProjectCmdId)]
internal sealed class SetPluginGeneratorTemplateInProjectCommand : BaseCommand<SetPluginGeneratorTemplateInProjectCommand>
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
        var i = await VS.Solutions.GetActiveItemAsync();
        if (i is null || i.Type != SolutionItemType.PhysicalFile) return;

        var solution = await VS.Solutions.GetCurrentSolutionAsync();
        var path = solution is not null && i.FullPath.StartsWith(solution.FullPath) ? i.FullPath[solution.FullPath.Length..] : i.FullPath;
        await SettingsProvider.ProjectSettings.PluginTemplateFilePathAsync(path);
    }
}