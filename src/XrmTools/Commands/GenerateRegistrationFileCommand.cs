#nullable enable
namespace XrmGen.Commands;

using Community.VisualStudio.Toolkit;
using Community.VisualStudio.Toolkit.DependencyInjection.Core;
using Community.VisualStudio.Toolkit.DependencyInjection;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using System.ComponentModel.Composition;
using XrmGen.Helpers;
using XrmGen.Xrm;
using Task = System.Threading.Tasks.Task;
using Microsoft.Extensions.Logging;
using XrmGen.UI;
using System.Threading.Tasks;
using System;
using XrmGen.Xrm.Model;

[Command(PackageGuids.guidGenerateXrmPluginConfigCmdSetString, PackageIds.idGeneratePluginConfigFileCommand)]
internal sealed class GenerateRegistrationFileCommand(
    DIToolkitPackage parentPackage, ILogger<GenerateRegistrationFileCommand> logger) : BaseDICommand(parentPackage)
{
    private readonly DTE2 dte = (parentPackage as XrmGenPackage)!.Dte!;
    private IXrmSchemaProviderFactory? _schemaProviderFactory;

    [Import]
    IXrmSchemaProviderFactory? SchemaProviderFactory
    {
        get => _schemaProviderFactory ??= VS.GetMefService<IXrmSchemaProviderFactory>();
        set => _schemaProviderFactory = value;
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
            return await AssemblySelector.ChooseAssemblyAsync(SchemaProviderFactory, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while choosing assembly: {0}", ex);
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