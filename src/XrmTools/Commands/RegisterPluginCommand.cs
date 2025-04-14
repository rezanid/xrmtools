#nullable enable
namespace XrmTools.Commands;

using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.Analyzers;
using XrmTools.Environments;
using XrmTools.Helpers;
using XrmTools.Logging.Compatibility;
using XrmTools.Meta.Model;
using XrmTools.Resources;
using XrmTools.WebApi;
using XrmTools.WebApi.Batch;
using XrmTools.WebApi.Entities;
using XrmTools.WebApi.Messages;
using XrmTools.WebApi.Methods;
using XrmTools.Xrm;
using XrmTools.Xrm.Model;
using XrmTools.Xrm.Repositories;
using Task = System.Threading.Tasks.Task;

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

    [Import]
    internal Validation.IValidationService Validator { get; set; } = null!;

    private async Task<bool> ValidatePluginAssemblyConfigAsync([NotNullWhen(true)] PluginAssemblyConfig? pluginAssembly)
    {
        var result = await Validator.ValidateIfValidatorAvailableAsync(pluginAssembly, Validation.Categories.WebApi);
        if (result != ValidationResult.Success)
        {
            await VS.MessageBox.ShowErrorAsync(Vsix.Name, result.ErrorMessage);
            return false;
        }
        return true;
    }

    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
    {
        var activeItem = await VS.Solutions.GetActiveItemAsync();
        if (activeItem is null || activeItem.FullPath is null || !(activeItem.Type is SolutionItemType.Project or SolutionItemType.PhysicalFile)) return;
        var project = activeItem.Type == SolutionItemType.Project ? (Project)activeItem : activeItem.FindParent(SolutionItemType.Project) as Project;
        if (project is null)
        {
            await VS.MessageBox.ShowErrorAsync(Vsix.Name, "The selected item is not a project or part of a project.");
            return;
        }
        var projectIsUpToDate = await VS.Build.ProjectIsUpToDateAsync(project);
        if (!projectIsUpToDate)
        {
            var buildSucceeded = await project.BuildAsync();
            if (!buildSucceeded) return;
        }

        // Parse the file and generate the PluginAssemblyConfig model from it.
        PluginAssemblyConfig? inputModel = null;
        try
        {
            inputModel = activeItem.Type == SolutionItemType.Project ?
                await MetaDataService.ParseProjectAsync(activeItem.FullPath) :
                await MetaDataService.ParseAsync(activeItem.FullPath);
        }
        catch (Exception ex)
        {
            await VS.StatusBar.EndAnimationAsync(StatusAnimation.General);
            await VS.StatusBar.ShowMessageAsync("Plugin registration failed.");
            Logger.LogError(ex, "An error occurred while parsing registration code.");
            await VS.MessageBox.ShowErrorAsync(Vsix.Name, "Plugin registration failed due to an error while while parsing registration code. " + ex.Message);
            return;
        }

        var validationPassed = await ValidatePluginAssemblyConfigAsync(inputModel);
        if (!validationPassed)
        {
            return;
        }

        var pluginCount = inputModel!.PluginTypes.Count;
        await VS.StatusBar.ShowMessageAsync($"Registering {pluginCount} plugin(s)...");
        await VS.StatusBar.StartAnimationAsync(StatusAnimation.General);

        var requests = new List<HttpRequestMessage>();
        
        try
        {
            var assemblyQuery = await WebApiService.RetrieveMultipleAsync<PluginAssembly>(
                $"{PluginAssemblyConfig.EntitySetName}?$select=name" +
                $"&$filter=name eq '{inputModel.Name}'" +
                $"&$expand=pluginassembly_plugintype($select=name" +
                    $";$expand=plugintype_sdkmessageprocessingstep($select=sdkmessageprocessingstepid),CustomAPIId($select=name))");
            if (assemblyQuery?.Entities?.SingleOrDefault() is PluginAssembly existingAssembly)
            {
                inputModel.PluginAssemblyId = existingAssembly.Id;
                Logger.LogInformation($"Found existing assembly ({existingAssembly.Id}).");
                requests.AddRange(GenerateDeleteRequestsForCleanup(
                    newAssembly: inputModel, existingAssembly: existingAssembly, skipPlugins: true));
                Logger.LogInformation($"Generated {requests.Count} delete requests for cleanup.");
            }
        }
        catch (Exception ex)
        {
            await VS.StatusBar.EndAnimationAsync(StatusAnimation.General);
            await VS.StatusBar.ShowMessageAsync("Plugin registration failed.");
            Logger.LogError(ex, "An error occurred while querying existin registrations.");
            await VS.MessageBox.ShowErrorAsync(Vsix.Name, "Plugin registration failed due to an error while querying existin registrations. " + ex.Message);
            return;
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
        await VS.StatusBar.ShowMessageAsync("Plugin(s) registered successfully.");
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
        //Command.Supported = false;
        var componentModel = await Package.GetServiceAsync<SComponentModel, IComponentModel>().ConfigureAwait(false);
        componentModel?.DefaultCompositionService.SatisfyImportsOnce(this);
        EnsureDependencies();
    }

    protected override void BeforeQueryStatus(EventArgs e)
    {
        ThreadHelper.JoinableTaskFactory.Run(async () =>
        {
            var item = await VS.Solutions.GetActiveItemAsync();
            Command.Visible = await IsVisibleAsync(item).ConfigureAwait(false);
        });

        async Task<bool> IsVisibleAsync(SolutionItem? item)
        {
            if (item is null)
                return false;

            if (item.Type == SolutionItemType.Project)
                return true;

            if (item.Type == SolutionItemType.PhysicalFile && item is PhysicalFile file)
                return await IsXrmPluginFileAsync(file).ConfigureAwait(false);

            return false;
        }

        async Task<bool> IsXrmPluginFileAsync(PhysicalFile file)
        {
            var generator = await file.GetAttributeAsync("Generator").ConfigureAwait(false);
            if (generator != XrmCodeGenerator.Name)
                return false;

            var pluginAttr = await file.GetAttributeAsync("IsXrmPlugin").ConfigureAwait(false);
            return bool.TryParse(pluginAttr, out var isXrmPlugin) && isXrmPlugin;
        }
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

            if (existingPlugin.CustomApi != null)
            {
                foreach (var customApi in existingPlugin.CustomApi)
                {
                    deleteRequests.Add(new DeleteRequest(customApi.ToReference()));
                }
            }
        }

        return deleteRequests;
    }

    [MemberNotNull(nameof(Logger), nameof(MetaDataService), nameof(WebApiService),
        nameof(EnvironmentProvider), nameof(RepositoryFactory))]
    private void EnsureDependencies()
    {
        if (Logger == null) throw new InvalidOperationException(string.Format(Strings.MissingServiceDependency, nameof(RegisterPluginCommand), nameof(Logger)));
        if (MetaDataService == null) throw new InvalidOperationException(string.Format(Strings.MissingServiceDependency, nameof(RegisterPluginCommand), nameof(MetaDataService)));
        if (WebApiService == null) throw new InvalidOperationException(string.Format(Strings.MissingServiceDependency, nameof(RegisterPluginCommand), nameof(WebApiService)));
        if (EnvironmentProvider == null) throw new InvalidOperationException(string.Format(Strings.MissingServiceDependency, nameof(RegisterPluginCommand), nameof(EnvironmentProvider)));
        if (RepositoryFactory == null) throw new InvalidOperationException(string.Format(Strings.MissingServiceDependency, nameof(RegisterPluginCommand), nameof(RepositoryFactory)));
    }
}
#nullable restore