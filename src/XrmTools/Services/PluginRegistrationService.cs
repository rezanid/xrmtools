#nullable enable
namespace XrmTools.Services;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using XrmTools.WebApi;
using XrmTools.Environments;
using XrmTools.Analyzers;
using XrmTools.Xrm.Repositories;
using XrmTools.Logging.Compatibility;
using XrmTools.Meta.Model.Configuration;
using XrmTools.WebApi.Entities;
using XrmTools.Helpers;
using XrmTools.WebApi.Methods;
using XrmTools.WebApi.Messages;
using XrmTools.WebApi.Batch;
using XrmTools.Xrm;
using XrmTools.Meta.Attributes;
using System.ComponentModel.Composition;

public interface IPluginRegistrationService
{
    Task<PluginRegistrationResult> RegisterAsync(RegistrationInput input, IPluginRegistrationUI ui, CancellationToken cancellationToken = default);
}

[Export(typeof(IPluginRegistrationService))]
[method: ImportingConstructor]
internal sealed class PluginRegistrationService(
    IWebApiService webApi,
    IEnvironmentProvider environmentProvider,
    IXrmMetaDataService meta,
    IRepositoryFactory repositoryFactory,
    ILogger<PluginRegistrationService> log,
    Validation.IValidationService validator) : IPluginRegistrationService
{
    private readonly IWebApiService _webApi = webApi;
    private readonly IEnvironmentProvider _environmentProvider = environmentProvider;
    private readonly IXrmMetaDataService _meta = meta;
    private readonly IRepositoryFactory _repositoryFactory = repositoryFactory;
    private readonly ILogger<PluginRegistrationService> _log = log;
    private readonly Validation.IValidationService _validator = validator;

    public async Task<PluginRegistrationResult> RegisterAsync(RegistrationInput input, IPluginRegistrationUI ui, CancellationToken cancellationToken = default)
    {
        PluginAssemblyConfig? model;
        try
        {
            model = input.IsProject
                ? await _meta.ParseProjectPluginsAsync(input.ItemFullPath, cancellationToken)
                : await _meta.ParsePluginsAsync(input.ItemFullPath, cancellationToken);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "An error occurred while parsing registration code.");
            return PluginRegistrationResult.Failure("Plugin registration failed due to an error while parsing registration code. " + ex.Message);
        }

        if (model is null)
        {
            return PluginRegistrationResult.Failure("No plugin configuration found.");
        }

        try
        {
            if (!string.IsNullOrWhiteSpace(input.NugetPackagePath))
            {
                model.Package = NugetParser.LoadFromNugetFile(input.NugetPackagePath!);
            }
            else
            {
                model.Content = Convert.ToBase64String(File.ReadAllBytes(model.FilePath));
            }
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "An error occurred while parsing plugin package/assembly content.");
            return PluginRegistrationResult.Failure("Plugin registration failed due to an error while parsing plugin package. " + ex.Message);
        }

        var validation = await _validator.ValidateIfValidatorAvailableAsync(model, Validation.Categories.WebApi, cancellationToken);
        if (validation != ValidationResult.Success)
        {
            return PluginRegistrationResult.Failure(validation?.ErrorMessage ?? "Validation failed.");
        }

        var requests = new List<HttpRequestMessage>();
        PluginAssembly? existingAssembly = null;

        try
        {
            var assemblyQuery = await _webApi.RetrieveMultipleAsync<PluginAssembly>(
                $"{PluginAssembly.Metadata.EntitySetName}?$select=name" +
                $"&$filter=name eq '{model.Name}'" +
                $"&$expand=PackageId($select=name),pluginassembly_plugintype($select=name,typename" +
                $";$expand=plugintype_sdkmessageprocessingstep($select=name,stage),CustomAPIId($select=uniquename))");

            existingAssembly = assemblyQuery?.Value?.SingleOrDefault();
            if (existingAssembly is not null)
            {
                model.Id = existingAssembly.Id;
                _log.LogInformation($"Found existing assembly ({existingAssembly.Id}).");

                if (!input.IsProject)
                {
                    var removedPlugins = existingAssembly.PluginTypes
                        .Where(existing =>
                            !model.PluginTypes.Any(p => p.TypeName == existing.TypeName) &&
                            !model.OtherPluginTypes.Any(p => p.TypeName == existing.TypeName))
                        .ToArray();

                    if (removedPlugins.Length > 0)
                    {
                        var removedNames = removedPlugins.Select(p => p.TypeName ?? p.Name ?? string.Empty)
                                                         .Where(n => !string.IsNullOrEmpty(n))
                                                         .ToArray();
                        var confirmed = await ui.ConfirmRemovePluginsAsync(removedNames, cancellationToken);
                        if (confirmed)
                        {
                            foreach (var removedPlugin in removedPlugins)
                            {
                                requests.AddRange(removedPlugin.Steps
                                    .Where(s => s.Id.HasValue)
                                    .Select(s => new DeleteRequest(SdkMessageProcessingStep.CreateReference(s.Id!.Value))));
                                if (removedPlugin.CustomApi.Count > 0 && removedPlugin.CustomApi[0].Id.HasValue)
                                {
                                    requests.Add(new DeleteRequest(CustomApi.CreateReference(removedPlugin.CustomApi[0].Id!.Value)));
                                }
                                if (removedPlugin.Id.HasValue)
                                {
                                    requests.Add(new DeleteRequest(PluginType.CreateReference(removedPlugin.Id!.Value)));
                                }
                            }
                        }
                    }
                }

                requests.AddRange(GenerateDeleteRequestsForCleanup(
                    newAssembly: model, existingAssembly: existingAssembly, deleteRemovedPlugins: input.IsProject));
                _log.LogInformation($"Generated {requests.Count} delete requests for cleanup.");
            }

            AssignIds(model, existingAssembly);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "An error occurred while querying existing registrations.");
            return PluginRegistrationResult.Failure("Plugin registration failed due to an error while querying existing registrations. " + ex.Message);
        }

        DataverseEnvironment? environment;
        Dictionary<string, SdkMessage>? sdkMessages;
        BatchRequest? batch;

        try
        {
            environment = await _environmentProvider.GetActiveEnvironmentAsync(true);
            var errMessage = environment is null
                ? "No active environment found. Please connect to an environment and try again."
                : environment.BaseServiceUrl is null
                    ? "Active environment has no valid URL. Please check the environment and try again."
                    : null;

            if (errMessage is not null)
            {
                return PluginRegistrationResult.Failure(errMessage);
            }

            sdkMessages = await FetchSdkMessagesAsync(model, cancellationToken);
            var builder = new UpsertRequestBuilder(model, sdkMessages);

            if (model.Package is null)
            {
                var upserts = builder
                    .WithAssembly()
                    .WithPluginTypesAndStepsAndCustomApis()
                    .Build();

                requests.AddRange(upserts);

                batch = new BatchRequest(environment!.BaseServiceUrl!)
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

                batch = new BatchRequest(environment!.BaseServiceUrl!)
                {
                    ChangeSets = [new(requests)]
                };
            }
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "An error occurred while generating plugin registration requests.");
            return PluginRegistrationResult.Failure("Plugin registration failed due to an error while generating registration requests. " + ex.Message);
        }

        try
        {
            var batchResponse = await _webApi.SendAsync<BatchResponse>(batch!);
            var responses = await batchResponse.ParseResponseAsync(cancellationToken);

            foreach (var response in responses)
            {
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.AsServiceExceptionAsync();
                    _log.LogCritical(error.ToString());
                    return PluginRegistrationResult.Failure(error.Message);
                }
                else if (response.Headers.Contains("OData-EntityId"))
                {
                    var path = response.As<UpsertResponse>().EntityReference?.Path;
                    _log.LogTrace($"Registered ({path}).");
                }
            }
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "An error occurred while sending the batch request.");
            return PluginRegistrationResult.Failure("Plugin registration failed. " + ex.Message);
        }

        if (model.Package is not null)
        {
            try
            {
                var assemblyQuery = await _webApi.RetrieveMultipleAsync<PluginAssembly>(
                    $"{PluginAssembly.Metadata.EntitySetName}?$select=name" +
                    $"&$filter=name eq '{model.Name}'" +
                    $"&$expand=PackageId($select=name),pluginassembly_plugintype($select=name,typename" +
                    $";$expand=plugintype_sdkmessageprocessingstep($select=name,stage),CustomAPIId($select=uniquename))");

                var existing = assemblyQuery?.Value?.SingleOrDefault();
                AssignIds(model, existing);

                var builder = new UpsertRequestBuilder(model, sdkMessages!);
                var upserts = builder.WithStepsAndCustomApis().Build();

                var followupBatch = new BatchRequest(environment.BaseServiceUrl!)
                {
                    ChangeSets = [new(upserts)]
                };

                var followupResponse = await _webApi.SendAsync<BatchResponse>(followupBatch);
                var followupParts = await followupResponse.ParseResponseAsync(cancellationToken);

                foreach (var response in followupParts)
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        var error = await response.AsServiceExceptionAsync();
                        _log.LogCritical(error.ToString());
                        return PluginRegistrationResult.Failure(error.Message);
                    }
                    else if (response.Headers.Contains("OData-EntityId"))
                    {
                        var path = response.As<UpsertResponse>().EntityReference?.Path;
                        _log.LogTrace($"Registered ({path}).");
                    }
                }
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An error occurred while registering steps/custom APIs after package upload.");
                return PluginRegistrationResult.Failure("Plugin registration failed during follow-up registration of steps/custom APIs.");
            }
        }

        return PluginRegistrationResult.Success();
    }

    private async Task<Dictionary<string, SdkMessage>> FetchSdkMessagesAsync(PluginAssemblyConfig config, CancellationToken cancellationToken)
    {
        var stepEntities = config.PluginTypes
            .SelectMany(p => p.Steps.Select(s => s.PrimaryEntityName))
            .Distinct()
            .ToArray();

        if (stepEntities == null || stepEntities.Length == 0)
            return new Dictionary<string, SdkMessage>(StringComparer.OrdinalIgnoreCase);

        _log.LogTrace($"Fetching SDK Messages for entities: {string.Join(", ", stepEntities)}");

        using var messageRepo = _repositoryFactory.CreateRepository<ISdkMessageRepository>();
        var messages = await messageRepo.GetForEntitiesAsync(stepEntities!, cancellationToken).ConfigureAwait(false);

        return messages.ToDictionary(m => m.Name, m => m, StringComparer.OrdinalIgnoreCase);
    }

    private void AssignIds(PluginAssemblyConfig pluginAssembly, PluginAssembly? existingPluginAssembly)
    {
        pluginAssembly.Id = existingPluginAssembly?.Id ?? GuidFactory.DeterministicGuid(GuidFactory.Namespace.PluginAssembly, pluginAssembly.Name!);
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
                    parameter.Id = existingParameter?.Id ?? GuidFactory.DeterministicGuid(GuidFactory.Namespace.CustomApiRequestParameter, customApi.UniqueName + parameter.UniqueName!);
                }
                foreach (var parameter in customApi.ResponseProperties)
                {
                    var existingParameter = existingCustomApi?.ResponseProperties.FirstOrDefault(p => p.UniqueName == parameter.UniqueName);
                    parameter.Id = existingParameter?.Id ?? GuidFactory.DeterministicGuid(GuidFactory.Namespace.CustomApiResponseProperty, customApi.UniqueName + parameter.UniqueName!);
                }
            }
        }
    }

    private ICollection<HttpRequestMessage> GenerateDeleteRequestsForCleanup(
        PluginAssemblyConfig newAssembly, PluginAssembly existingAssembly, bool deleteRemovedPlugins)
    {
        var deleteRequests = new List<HttpRequestMessage>();

        foreach (var existingPlugin in existingAssembly.PluginTypes)
        {
            if (string.IsNullOrEmpty(existingPlugin.Name)) continue;

            if (newAssembly.PluginTypes.FirstOrDefault(p => string.Equals(p.TypeName, existingPlugin.TypeName, StringComparison.InvariantCulture)) is not null)
            {
                foreach (var step in existingPlugin.Steps)
                {
                    if (step.Stage != Stages.MainOperation)
                        deleteRequests.Add(new DeleteRequest(step.ToReference()));
                }

                if (existingPlugin.CustomApi != null)
                {
                    foreach (var customApi in existingPlugin.CustomApi)
                    {
                        deleteRequests.Add(new DeleteRequest(customApi.ToReference()));
                    }
                }
            }
            else if (deleteRemovedPlugins)
            {
                if (existingPlugin.CustomApi is not null && existingPlugin.CustomApi.Count > 0 && existingPlugin.CustomApi[0] is CustomApi api && api.Id.HasValue)
                {
                    deleteRequests.Add(new DeleteRequest(CustomApi.CreateReference(api.Id!.Value)));
                }
                if (existingPlugin.Id.HasValue)
                {
                    deleteRequests.Add(new DeleteRequest(PluginType.CreateReference(existingPlugin.Id!.Value)));
                }
            }
        }

        return deleteRequests;
    }
}
#nullable restore