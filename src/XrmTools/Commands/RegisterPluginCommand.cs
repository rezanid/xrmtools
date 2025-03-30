#nullable enable
namespace XrmTools.Commands;

using Community.VisualStudio.Toolkit;
using EnvDTE;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading.Tasks;
using XrmTools.WebApi.Batch;
using XrmTools.WebApi.Messages;
using XrmTools.Analyzers;
using XrmTools.Logging.Compatibility;
using XrmTools.Resources;
using XrmTools.Xrm.Model;
using Task = System.Threading.Tasks.Task;
using XrmTools.Environments;
using XrmTools.WebApi;
using XrmTools.WebApi.Methods;
using XrmTools.Helpers;
using System.Linq;
using XrmTools.Xrm.Repositories;
using XrmTools.Xrm;
using System.Threading;
using XrmTools.Meta.Model;
using XrmTools.WebApi.Entities;

/// <summary>
/// Command handler to set the custom tool of the selected item to the Xrm Plugin Code Generator.
/// </summary>
[Command(PackageGuids.XrmToolsCmdSetIdString, PackageIds.RegisterPluginCmdId)]
internal sealed class RegisterPluginCommand : BaseCommand<RegisterPluginCommand>
{
    [Import]
    internal IWebApiService WebApiService { get; set; } = null!;

    [Import]
    internal IEnvironmentProvider EnvironmentProvider { get; set; } = null!;

    [Import]
    internal IXrmMetaDataService MetaDataService { get; set; } = null!;

    [Import]
    internal IRepositoryFactory RepositoryFactory { get; set; } = null!;

    [Import]
    internal ILogger<RegisterPluginCommand> Logger { get; set; } = null!;

    private async Task<(bool pass, string message)> ValidatePluginAssemblyConfigAsync([NotNullWhen(true)]PluginAssemblyConfig? pluginAssembly)
    {
        if (pluginAssembly == null)
        {
            await VS.MessageBox.ShowErrorAsync(
                Vsix.Name,
                "Could not parse the file. Make sure the file is a valid C# file with at least a plugin in correct structure.");
            return (false, "Invalid plugin assembly");
        }
        if (string.IsNullOrWhiteSpace(pluginAssembly.Name))
        {
            await VS.MessageBox.ShowErrorAsync(
                Vsix.Name,
                "Could not parse assembly name or assembly name has not been set.");
            return (false, "Invalid plugin assembly");
        }

        pluginAssembly.PluginAssemblyId = pluginAssembly.PluginAssemblyId.NewIfEmpty(GuidFactory.Namespace.PluginAssembly, pluginAssembly.Name);
        if (!pluginAssembly.PluginAssemblyId.HasValue || pluginAssembly.PluginAssemblyId.Value == Guid.Empty)
        {
            pluginAssembly.PluginAssemblyId = GuidFactory.DeterministicGuid(GuidFactory.Namespace.PluginAssembly, pluginAssembly.Name);
            Logger.LogTrace($"A deterministic GUID has been asseigned to assembly {pluginAssembly.Name} based on its name.");
        }

        foreach (var plugin in pluginAssembly.PluginTypes)
        {
            if (plugin is null)
            {
                await VS.MessageBox.ShowErrorAsync(
                    Vsix.Name,
                    "Could not parse plugin type.");
                return (false, "Could not parse plugin type.");
            }
            if (string.IsNullOrWhiteSpace(plugin.TypeName))
            {
                await VS.MessageBox.ShowErrorAsync(
                    Vsix.Name,
                    "Could not parse plugin type name.");
                return (false, "Invalid plugin type.");
            }
            if (!plugin.PluginTypeId.HasValue || plugin.PluginTypeId.Value == Guid.Empty)
            {
                plugin.PluginTypeId = GuidFactory.DeterministicGuid(GuidFactory.Namespace.PluginType, plugin.TypeName);
                Logger.LogTrace($"A deterministic GUID has been asseigned to plugin type {plugin.TypeName} based on its name.");
            }
            foreach (var step in plugin.Steps)
            {
                if (step is null)
                {
                    await VS.MessageBox.ShowErrorAsync(
                        Vsix.Name,
                        "Could not parse plugin step.");
                    return (false, "Invalid plugin step.");
                }
                if (string.IsNullOrWhiteSpace(step.Name))
                {
                    await VS.MessageBox.ShowErrorAsync(
                        Vsix.Name,
                        "Could not parse plugin step name.");
                    return (false, "Invalid plugin step.");
                }
                if (!step.PluginStepId.HasValue || step.PluginStepId.Value == Guid.Empty)
                {
                    step.PluginStepId = GuidFactory.DeterministicGuid(GuidFactory.Namespace.Step, plugin.TypeName + step.Name);
                    Logger.LogTrace($"A deterministic GUID has been asseigned to sdkmessageprocessingstep based on its plugin type and its name.");
                }
                foreach (var image in step.Images)
                {
                    if (image is null)
                    {
                        await VS.MessageBox.ShowErrorAsync(
                            Vsix.Name,
                            "Could not parse plugin image.");
                        return (false, "Invalid plugin image.");
                    }
                    if (string.IsNullOrWhiteSpace(image.Name))
                    {
                        await VS.MessageBox.ShowErrorAsync(
                            Vsix.Name,
                            "Could not parse plugin image name.");
                        return (false, "Invalid plugin image.");
                    }
                    if (!image.PluginStepImageId.HasValue || image.PluginStepImageId.Value == Guid.Empty)
                    {
                        image.PluginStepImageId = GuidFactory.DeterministicGuid(GuidFactory.Namespace.Image, plugin.TypeName + step.Name + image.Name);
                        Logger.LogTrace($"A deterministic GUID has been asseigned to sdkmessageprocessingstepimage based on its plugin type, step and its name.");
                    }
                }
            }

        }

        return (true, "Valid plugin assembly");
    }

    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
    {
        var i = await VS.Solutions.GetActiveItemAsync();
        if (i is null || i.Type != SolutionItemType.PhysicalFile || i.FullPath is null) return;

        // Parse the file and generate the PluginAssemblyConfig model from it.
        var inputModel = await MetaDataService.ParseAsync(i.FullPath);

        var (validationPassed, validationMessage) = await ValidatePluginAssemblyConfigAsync(inputModel);
        if (!validationPassed)
        {
            Logger.LogWarning("Plugin registration will not continue due to validation error(s): " + validationMessage);
            return;
        }

        await VS.StatusBar.ShowMessageAsync("Registering plugin assembly...");
        await VS.StatusBar.StartAnimationAsync(StatusAnimation.General);

        var assemblyQuery = await WebApiService.RetrieveMultipleAsync<PluginAssembly>(
            $"{PluginAssemblyConfig.EntitySetName}?$select=name" +
            $"&$filter=name eq '{inputModel.Name}'" +
            $"&$expand=pluginassembly_plugintype($select=name" +
                $";$expand=plugintype_sdkmessageprocessingstep($select=sdkmessageprocessingstepid))");
        var requests = new List<HttpRequestMessage>();
        if (assemblyQuery?.Entities?.SingleOrDefault() is PluginAssembly existingAssembly)
        {
            inputModel.PluginAssemblyId = existingAssembly.Id;
            Logger.LogInformation($"Found existing assembly ({existingAssembly.Id}).");
            requests.AddRange(GenerateDeleteRequestsForCleanup(
                newAssembly: inputModel, existingAssembly: existingAssembly, skipPlugins: true));
            Logger.LogInformation($"Generated {requests.Count} delete requests for cleanup.");
        }

        BatchRequest? batch;
        try
        {
            var upserts = await GenerateUpsertRequestsAsync(inputModel);

            var environment = await EnvironmentProvider.GetActiveEnvironmentAsync();
            if (environment == null)
            {
                await VS.StatusBar.EndAnimationAsync(StatusAnimation.General);
                await VS.StatusBar.ShowMessageAsync("Plugin registration failed.");
                await VS.MessageBox.ShowErrorAsync(
                    Vsix.Name,
                    "No active environment found. Please connect to an environment and try again.");
                return;
            }
            requests.AddRange(upserts);
            batch = new BatchRequest(environment.BaseServiceUrl)
            {
                ChangeSets = [new(requests)]
            };
        }
        catch (Exception ex)
        {
            await VS.StatusBar.EndAnimationAsync(StatusAnimation.General);
            await VS.StatusBar.ShowMessageAsync("Plugin registration failed.");
            Logger.LogError(ex, "An error occurred while generating plugin registration requests.");
            await VS.MessageBox.ShowErrorAsync(Vsix.Name, "Plugin registration failed due to an error while generating registration requests. " + ex.Message);
            return;
        }

        var batchResponse = await WebApiService.SendAsync<BatchResponse>(batch);
        var responses = await batchResponse.ParseResponseAsync();
        foreach (var response in responses)
        {
            if (!response.IsSuccessStatusCode)
            {
                await VS.StatusBar.EndAnimationAsync(StatusAnimation.General);
                await VS.StatusBar.ShowMessageAsync("Plugin registration failed.");
                var error = await WebApi.WebApiService.ParseExceptionAsync(response);
                Logger.LogCritical(error.ToString());
                await VS.MessageBox.ShowErrorAsync(Vsix.Name, error.Message);
                return;
            }
            else
            {
                if (response.Headers.Contains("OData-EntityId"))
                {
                    var path = response.As<UpsertResponse>().EntityReference?.Path;
                    Logger.LogTrace($"Registered ({path}).");
                }
            }
        }
        await VS.StatusBar.ShowMessageAsync("Plugin registered successfully.");
        await VS.StatusBar.EndAnimationAsync(StatusAnimation.General);
    }

    private async Task<ICollection<HttpRequestMessage>> GenerateUpsertRequestsAsync(PluginAssemblyConfig config, CancellationToken cancellationToken = default)
    {
        var sdkMessages = await FetchSdkMessagesAsync(config, cancellationToken);
        var base64Assembly = Convert.ToBase64String(System.IO.File.ReadAllBytes(config.FilePath));

        var builder = new UpsertRequestBuilder(config, base64Assembly, sdkMessages);
        return builder
            .WithAssembly()
            .WithPluginTypes()
            .Build();
    }

    private async Task<Dictionary<string, SdkMessage>> FetchSdkMessagesAsync(PluginAssemblyConfig config, CancellationToken cancellationToken)
    {
        var stepEntities = config.PluginTypes
            .SelectMany(p => p.Steps.Select(s => s.PrimaryEntityName))
            .Distinct()
            .ToArray();

        if (stepEntities == null || stepEntities.Length == 0)
            return [];

        Logger.LogTrace($"Fetching SDK Messages for entities: {string.Join(", ", stepEntities)}");

        var messageRepo = await RepositoryFactory.CreateRepositoryAsync<ISdkMessageRepository>();
        var messages = await messageRepo.GetAsync(stepEntities!, cancellationToken);

        return messages.ToDictionary(m => m.Name, m => m);
    }

    protected override async Task InitializeCompletedAsync()
    {
        Command.Supported = false;
        var componentModel = await Package.GetServiceAsync<SComponentModel, IComponentModel>().ConfigureAwait(false);
        componentModel?.DefaultCompositionService.SatisfyImportsOnce(this);
        EnsureDependencies();
    }
    
    protected override void BeforeQueryStatus(EventArgs e)
    {
        ThreadHelper.JoinableTaskFactory.Run(async () =>
        {
            var item = await VS.Solutions.GetActiveItemAsync();
            Command.Visible = item?.Type == SolutionItemType.PhysicalFile &&
                await ((PhysicalFile)item).GetAttributeAsync("Generator") == XrmCodeGenerator.Name;
        });
    }

    [MemberNotNull(nameof(Logger), nameof(MetaDataService))]
    private void EnsureDependencies()
    {
        if (Logger == null) throw new InvalidOperationException(string.Format(Strings.MissingServiceDependency, nameof(RegisterPluginCommand), nameof(Logger)));
        if (MetaDataService == null) throw new InvalidOperationException(string.Format(Strings.MissingServiceDependency, nameof(RegisterPluginCommand), nameof(MetaDataService)));
    }

    private ICollection<HttpRequestMessage> GenerateDeleteRequestsForCleanup(
        PluginAssemblyConfig newAssembly, PluginAssembly existingAssembly, bool skipPlugins)
    {
        var deleteRequests = new List<HttpRequestMessage>();

        foreach (var existingPlugin in existingAssembly.PluginTypes)
        {
            if (string.IsNullOrEmpty(existingPlugin.Name)) continue;

            if (newAssembly.PluginTypes.FirstOrDefault(p => p.Name == existingPlugin.Name) is PluginTypeConfig newPlugin)
            {
                foreach (var step in existingPlugin.Steps)
                {
                    deleteRequests.Add(new DeleteRequest(step.ToReference()));
                }
            }
            else if (!skipPlugins)
            {
                deleteRequests.Add(new DeleteRequest(PluginType.CreateReference(existingPlugin.Id!.Value)));
            }
        }

        return deleteRequests;
    }
}
#nullable restore