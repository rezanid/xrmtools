#nullable enable
namespace XrmTools.Commands;

using Community.VisualStudio.Toolkit;
using Community.VisualStudio.Toolkit.DependencyInjection.Core;
using Community.VisualStudio.Toolkit.DependencyInjection;
using Microsoft.VisualStudio.Shell;
using XrmTools.Helpers;
using Task = System.Threading.Tasks.Task;
using Microsoft.Extensions.Logging;
using XrmTools.UI;
using System.Threading.Tasks;
using System;
using XrmTools.Xrm.Model;

[Command(PackageGuids.XrmToolsCmdSetIdString, PackageIds.NewPluginDefinitionCmdId)]
internal sealed class NewPluginDefinitionFileCommand : BaseDICommand
{
    private readonly ILogger<NewPluginDefinitionFileCommand> logger;
    private readonly IAssemblySelector assemblySelector;

    public NewPluginDefinitionFileCommand(
        DIToolkitPackage parentPackage, 
        ILogger<NewPluginDefinitionFileCommand> logger, 
        IAssemblySelector assemblySelector) : base(parentPackage)
    {
        Command.Supported = false;
        this.logger = logger;
        this.assemblySelector = assemblySelector;
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
            logger.LogInformation("Assembly selected: " + selectedAssembly.Name);
        }
        else
        {
            logger.LogInformation("No assembly selected.");
            return;
        }
        await WriteAssemblyConfigAsync(selectedAssembly, activeItem, filename);
    }

    private async Task<(PluginAssemblyConfig? selectedAssembly, string? filename)> ChooseAssemblyAsync()
    {
        try
        {
            return await assemblySelector.ChooseAssemblyAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while choosing assembly: {0}" + ex.InnerException?.Message, ex);
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
                logger.LogCritical("Failed to serialize plugin assembly configuration.", []);
                await VS.MessageBox.ShowWarningAsync(Vsix.Name, "Failed to serialize plugin assembly configuration.");
                return;
            }
            //await FileHelper.AddItemAsync(filename, target, content ?? string.Empty, dte, logger);
            await FileHelper.AddItemAsync(filename, content, activeItem);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while generating plugin assembly config. {filename}", filename);
            await VS.MessageBox.ShowErrorAsync("Error while generating plugin assembly config", ex.Message);
        }
    }
}
#nullable restore