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
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.Analyzers;
using XrmTools.Environments;
using XrmTools.Helpers;
using XrmTools.Logging.Compatibility;
using XrmTools.Meta.Attributes;
using XrmTools.Resources;
using XrmTools.WebApi;
using XrmTools.WebApi.Batch;
using XrmTools.WebApi.Entities;
using XrmTools.WebApi.Messages;
using XrmTools.WebApi.Methods;
using XrmTools.Xrm;
using XrmTools.Xrm.Model;
using XrmTools.Xrm.Repositories;
using static XrmTools.Helpers.ProjectExtensions;
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

    public (bool suceeded, string message) RegisterPluginPackage()
    {

        return (true, string.Empty);
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
            if (!buildSucceeded)
            {
                return;
            }
        }

        // Parse the file and generate the PluginAssemblyConfig model from it.
        PluginAssemblyConfig? inputModel;
        try
        {
            inputModel = activeItem.Type == SolutionItemType.Project ?
                await MetaDataService.ParseProjectPluginsAsync(activeItem.FullPath) :
                await MetaDataService.ParsePluginsAsync(activeItem.FullPath);
        }
        catch (Exception ex)
        {
            await VS.StatusBar.EndAnimationAsync(StatusAnimation.General);
            await VS.StatusBar.ShowMessageAsync("Plugin registration failed.");
            Logger.LogError(ex, "An error occurred while parsing registration code.");
            await VS.MessageBox.ShowErrorAsync(Vsix.Name, "Plugin registration failed due to an error while while parsing registration code. " + ex.Message);
            return;
        }

        try
        {
            if (inputModel is not null && project.GetBuildProperty<bool>(BuildProperties.GeneratePackageOnBuild))
            {
                var nugetFilePath = Path.Combine(project.FullPath, project.GetOutputPackagePath());
                inputModel!.Package = NugetParser.LoadFromNugetFile(nugetFilePath);
            }
            else
            {
                inputModel!.Content = Convert.ToBase64String(File.ReadAllBytes(inputModel.FilePath));
            }
        }
        catch (Exception ex)
        {
            await VS.StatusBar.EndAnimationAsync(StatusAnimation.General);
            await VS.StatusBar.ShowMessageAsync("Plugin registration failed.");
            Logger.LogError(ex, "An error occurred while parsing plugin package code.");
            await VS.MessageBox.ShowErrorAsync(Vsix.Name, "Plugin registration failed due to an error while while parsing plugin package. " + ex.Message);
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
                $"{PluginAssembly.Metadata.EntitySetName}?$select=name" +
                $"&$filter=name eq '{inputModel.Name}'" +
                $"&$expand=PackageId($select=name),pluginassembly_plugintype($select=name,typename" +
                    $";$expand=plugintype_sdkmessageprocessingstep($select=name,stage),CustomAPIId($select=uniquename))");
            var existingAssembly = assemblyQuery?.Value?.SingleOrDefault();
            if (existingAssembly is not null)
            {
                inputModel.PluginAssemblyId = existingAssembly.Id;
                Logger.LogInformation($"Found existing assembly ({existingAssembly.Id}).");
                requests.AddRange(GenerateDeleteRequestsForCleanup(
                    newAssembly: inputModel, existingAssembly: existingAssembly, skipPlugins: true));
                Logger.LogInformation($"Generated {requests.Count} delete requests for cleanup.");
            }
            AssignIds(inputModel, existingAssembly);
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
        DataverseEnvironment? environment;
        Dictionary<string, SdkMessage>? sdkMessages;
        try
        {
            environment = await EnvironmentProvider.GetActiveEnvironmentAsync();
            if (environment == null)
            {
                await VS.StatusBar.EndAnimationAsync(StatusAnimation.General);
                await VS.StatusBar.ShowMessageAsync("Plugin registration failed.");
                await VS.MessageBox.ShowErrorAsync(
                    Vsix.Name,
                    "No active environment found. Please connect to an environment and try again.");
                return;
            }

            sdkMessages = await FetchSdkMessagesAsync(inputModel, default);
            var builder = new UpsertRequestBuilder(inputModel, sdkMessages);

            if (inputModel.Package is null)
            {
                var upserts = builder
                    .WithAssembly()
                    .WithPluginTypesAndStepsAndCustomApis()
                    .Build();

                requests.AddRange(upserts);
                batch = new BatchRequest(environment.BaseServiceUrl)
                {
                    ChangeSets = [new(requests)]
                };
            }
            else
            {
                var upserts = builder
                    .WithPackage()
                    .Build();

                requests.AddRange(upserts);
                batch = new BatchRequest(environment.BaseServiceUrl)
                {
                    ChangeSets = [new(requests)]
                };
            }
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

        if (inputModel.Package is not null)
        {
            var assemblyQuery = await WebApiService.RetrieveMultipleAsync<PluginAssembly>(
                $"{PluginAssembly.Metadata.EntitySetName}?$select=name" +
                $"&$filter=name eq '{inputModel.Name}'" +
                $"&$expand=PackageId($select=name),pluginassembly_plugintype($select=name,typename" +
                    $";$expand=plugintype_sdkmessageprocessingstep($select=name,stage),CustomAPIId($select=uniquename))");
            var existingAssembly = assemblyQuery?.Value?.SingleOrDefault();
            AssignIds(inputModel, existingAssembly);

            var builder = new UpsertRequestBuilder(inputModel, sdkMessages);
            var upserts = builder.WithStepsAndCustomApis().Build();
            batch = new BatchRequest(environment.BaseServiceUrl)
            {
                ChangeSets = [new(upserts)]
            };

            batchResponse = await WebApiService.SendAsync<BatchResponse>(batch);
            responses = await batchResponse.ParseResponseAsync();
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
        }

        await VS.StatusBar.ShowMessageAsync("Plugin(s) registered successfully.");
        await VS.StatusBar.EndAnimationAsync(StatusAnimation.General);
    }

    private void AssignIds(PluginAssemblyConfig pluginAssembly, PluginAssembly? existingPluginAssembly)
    {
        pluginAssembly.PluginAssemblyId = existingPluginAssembly?.Id ?? GuidFactory.DeterministicGuid(GuidFactory.Namespace.PluginAssembly, pluginAssembly.Name!);
        if (pluginAssembly.Package is not null)
        {
            pluginAssembly.Package.Id = existingPluginAssembly?.Package?.Id ?? GuidFactory.DeterministicGuid(GuidFactory.Namespace.PluginPackage, pluginAssembly.Package.Name!);
        }
        foreach (var pluginType in pluginAssembly.PluginTypes)
        {
            var existingPluginType = existingPluginAssembly?.PluginTypes.FirstOrDefault(p => p.TypeName!.Equals(pluginType.TypeName, StringComparison.OrdinalIgnoreCase));
            pluginType.Id = existingPluginType?.Id ?? GuidFactory.DeterministicGuid(GuidFactory.Namespace.PluginType, pluginType.TypeName!);
            foreach (var step in pluginType.Steps)
            {
                var existingStep = existingPluginType?.Steps.FirstOrDefault(s => s.Name == step.Name);
                step.Id = existingStep?.Id ?? GuidFactory.DeterministicGuid(GuidFactory.Namespace.Step, pluginType.TypeName + step.Name!);
                foreach (var image in step.Images)
                {
                    var existingImage = existingStep?.Images.FirstOrDefault(i => i.Name == image.Name);
                    image.Id = existingImage?.Id ?? GuidFactory.DeterministicGuid(GuidFactory.Namespace.Image, pluginType.TypeName + step.Name! + image.Name);
                }
            }
            if (pluginType.CustomApi is CustomApi customApi)
            {
                var existingCustomApi = existingPluginType?.CustomApi.FirstOrDefault(c => string.Equals(c.UniqueName, customApi.UniqueName, StringComparison.OrdinalIgnoreCase));
                customApi.Id = existingCustomApi?.Id ?? GuidFactory.DeterministicGuid(GuidFactory.Namespace.CustomApi, customApi.UniqueName!);
                foreach (var parameter in customApi.RequestParameters)
                {
                    var existingParameter = existingCustomApi?.RequestParameters.FirstOrDefault(p => p.UniqueName == parameter.UniqueName);
                    parameter.Id = existingParameter?.Id ?? GuidFactory.DeterministicGuid(GuidFactory.Namespace.CustomApiInput, customApi.UniqueName + parameter.UniqueName!);
                }
                foreach (var parameter in customApi.ResponseProperties)
                {
                    var existingParameter = existingCustomApi?.ResponseProperties.FirstOrDefault(p => p.UniqueName == parameter.UniqueName);
                    parameter.Id = existingParameter?.Id ?? GuidFactory.DeterministicGuid(GuidFactory.Namespace.CustomApiOutput, customApi.UniqueName + parameter.UniqueName!);
                }
            }
        }
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
        var messages = await messageRepo.GetForEntitiesAsync(stepEntities!, cancellationToken);

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

        static async Task<bool> IsVisibleAsync(SolutionItem? item)
        {
            if (item is null)
                return false;

            if (item.Type == SolutionItemType.Project)
                return true;

            if (item.Type == SolutionItemType.PhysicalFile && item is PhysicalFile file)
                return await file.IsXrmPluginFileAsync().ConfigureAwait(false);

            return false;
        }
    }

    private ICollection<HttpRequestMessage> GenerateDeleteRequestsForCleanup(
        PluginAssemblyConfig newAssembly, PluginAssembly existingAssembly, bool skipPlugins)
    {
        var deleteRequests = new List<HttpRequestMessage>();

        foreach (var existingPlugin in existingAssembly.PluginTypes)
        {
            if (string.IsNullOrEmpty(existingPlugin.Name)) continue;

            // If plugin type exists, delete all its steps, later plugin type will be upserted and its new steps created.
            if (newAssembly.PluginTypes.FirstOrDefault(p => string.Equals(p.TypeName, existingPlugin.TypeName, StringComparison.InvariantCulture)) is PluginTypeConfig newPlugin)
            {
                foreach (var step in existingPlugin.Steps)
                {
                    // We shouldn't (and cannot) delete / modify MainOperations (generated automatically by Dataverse for CustomApis).
                    if (step.Stage != Stages.MainOperation)
                        deleteRequests.Add(new DeleteRequest(step.ToReference()));
                }
            }
            // Delete all existing plguin types (if needed)
            else if (!skipPlugins)
            {
                deleteRequests.Add(new DeleteRequest(PluginType.CreateReference(existingPlugin.Id!.Value)));
            }
            // Delete all existing custom APIs
            if (existingPlugin.CustomApi != null)
            {
                foreach (var customApi in existingPlugin.CustomApi)
                {
                    deleteRequests.Add(new DeleteRequest(customApi.ToReference()));
                }
            }
        }

        //TODO: Let's not delete the package for now, we will see with more testing if we need to do this.
        //if (newAssembly.Package ==  null && existingAssembly.Package != null)
        //{
        //    deleteRequests.Add(new DeleteRequest(existingAssembly.Package.ToReference()));
        //}

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