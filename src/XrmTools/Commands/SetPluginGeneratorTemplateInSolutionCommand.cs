namespace XrmTools.Commands;
using Community.VisualStudio.Toolkit;
using Community.VisualStudio.Toolkit.DependencyInjection;
using Community.VisualStudio.Toolkit.DependencyInjection.Core;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using XrmTools.Settings;
using Task = System.Threading.Tasks.Task;

/// <summary>
/// Command handler to set the selected file as template for plugin code generation.
/// </summary>
[Command(PackageGuids.XrmToolsCmdSetIdString, PackageIds.SetPluginGeneratorTemplateInSolutionCmdId)]
internal sealed class SetPluginGeneratorTemplateInSolutionCommand : BaseDICommand
{
    private readonly ISettingsProvider settingsProvider;

    public SetPluginGeneratorTemplateInSolutionCommand(DIToolkitPackage package, ISettingsProvider settingsProvider) : base(package)
    {
        Command.Supported = false;
        this.settingsProvider = settingsProvider;
    }

    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
    {
        var i = await VS.Solutions.GetActiveItemAsync();
        if (i is null || i.Type != SolutionItemType.PhysicalFile) return;

        var solution = await VS.Solutions.GetCurrentSolutionAsync();
        var path = solution is not null && i.FullPath.StartsWith(solution.FullPath) ? i.FullPath[solution.FullPath.Length..] : i.FullPath;
        settingsProvider.SolutionSettings.Set(XrmSettingKeys.DataversePluginTemplateFilePath, path);
        settingsProvider.SolutionSettings.PluginTemplateFilePath(path);
    }
}
