#nullable enable
namespace XrmTools.Commands;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using XrmTools.Helpers;
using Task = System.Threading.Tasks.Task;
using XrmTools.UI;
using System.Threading.Tasks;
using System;
using XrmTools.Xrm.Model;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.ComponentModelHost;
using System.Diagnostics.CodeAnalysis;
using XrmTools.Resources;
using XrmTools.Logging.Compatibility;
using XrmTools.Core.Helpers;

[Command(PackageGuids.XrmToolsCmdSetIdString, PackageIds.NewPluginDefinitionCmdId)]
internal sealed class NewPluginDefinitionFileCommand : BaseCommand<NewPluginDefinitionFileCommand>
{
    [Import]
    public ILogger<NewPluginDefinitionFileCommand>? Logger { get; set; }
    [Import]
    public IAssemblySelector? AssemblySelector { get; set; }

    protected override async Task InitializeCompletedAsync()
    {
        Command.Supported = false;
        var componentModel = await Package.GetServiceAsync<SComponentModel, IComponentModel>().ConfigureAwait(false);
        componentModel?.DefaultCompositionService.SatisfyImportsOnce(this);
        EnsureDependencies();
    }

    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
    {
        var activeItem = await FileHelper.FindActiveItemAsync();

        if (activeItem == null)
        {
            await VS.MessageBox.ShowErrorAsync(
                Vsix.Name,
                "Could not determine where to create the new file. Select a file or folder in Solution Explorer and try again.");
            return;
        }

        var (selectedAssembly, filename) = await ChooseAssemblyAsync();
        if (selectedAssembly is not null && filename is not null)
        {
            Logger.LogInformation("Assembly selected: " + selectedAssembly.Name);
        }
        else
        {
            Logger.LogInformation("No assembly selected.");
            return;
        }
        await WriteAssemblyConfigAsync(selectedAssembly, activeItem, filename);
    }

    [MemberNotNull(nameof(Logger), nameof(AssemblySelector))]
    private void EnsureDependencies()
    {
        if (Logger == null) throw new InvalidOperationException(string.Format(Strings.MissingServiceDependency, nameof(NewPluginDefinitionFileCommand), nameof(Logger)));
        if (AssemblySelector == null) throw new InvalidOperationException(string.Format(Strings.MissingServiceDependency, nameof(NewPluginDefinitionFileCommand), nameof(AssemblySelector)));
    }

    private async Task<(PluginAssemblyConfig? selectedAssembly, string? filename)> ChooseAssemblyAsync()
    {
        try
        {
            return await AssemblySelector!.ChooseAssemblyAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error while choosing assembly: {0}" + ex.InnerException?.Message, ex);
            await VS.MessageBox.ShowErrorAsync("Error while choosing assembly", ex.Message);
        }
        return (null, null);
    }

    private async Task WriteAssemblyConfigAsync(PluginAssemblyConfig selectedAssembly, SolutionItem activeItem, string filename)
    {
        try
        {
            var content = StringHelpers.SerializeJson(selectedAssembly);
            if (string.IsNullOrWhiteSpace(content))
            {
                Logger.LogCritical("Failed to serialize plugin assembly configuration.", []);
                await VS.MessageBox.ShowWarningAsync(Vsix.Name, "Failed to serialize plugin assembly configuration.");
                return;
            }
            //await FileHelper.AddItemAsync(filename, target, content ?? string.Empty, dte, logger);
            await FileHelper.AddItemAsync(filename, content, activeItem);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error while generating plugin assembly config. {filename}", filename);
            await VS.MessageBox.ShowErrorAsync("Error while generating plugin assembly config", ex.Message);
        }
    }
}
#nullable restore