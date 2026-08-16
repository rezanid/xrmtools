#nullable enable
namespace XrmTools.Services;

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.DataAnnotations;
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
using XrmTools.Meta.Model.Configuration;
using XrmTools.UI;
using XrmTools.WebApi;
using XrmTools.WebApi.Batch;
using XrmTools.WebApi.Entities;
using XrmTools.WebApi.Messages;
using XrmTools.WebApi.Methods;
using XrmTools.Xrm;
using XrmTools.Xrm.Repositories;

public interface IPluginRegistrationService
{
    public Task<PluginRegistrationResult> RegisterAsync(RegistrationInput input, IPluginRegistrationUI ui, CancellationToken cancellationToken = default);
    public Task<PluginRegistrationResult> UnregisterAsync(RegistrationInput input, IPluginRegistrationUI ui, CancellationToken cancellationToken = default);
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

    public async Task<PluginRegistrationResult> UnregisterAsync(RegistrationInput input, IPluginRegistrationUI ui, CancellationToken cancellationToken = default)
    {
        if (!input.IsProject)
        {
            return PluginRegistrationResult.Failure(
                "Unregistration is only supported for project (assembly). If you need to unregister a plugin, just remove the plugin from the project and register the plugin.");
        }
        PluginAssemblyConfig? model;
        try
        {
            model = await _meta.ParseProjectPluginsAsync(input.ItemFullPath, cancellationToken);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "An error occurred while parsing registration code.");
            return PluginRegistrationResult.Failure("Plugin registration failed due to an error while parsing registration code. " + ex.Message);
        }

        if (model is null)
        {
            return PluginRegistrationResult.Failure("No plugin definition found.");
        }

        var requests = new List<HttpRequestMessage>();
        PluginAssembly? existingAssembly;

        try
        {
            var assemblyQuery = await _webApi.RetrieveMultipleAsync<PluginAssembly>(
                $"{PluginAssembly.Metadata.EntitySetName}?$select=name" +
                $"&$filter=name eq '{model.Name}'" +
                $"&$expand=PackageId($select=name),pluginassembly_plugintype($select=name,typename" +
                $";$expand=plugintype_sdkmessageprocessingstep($select=name,stage),CustomAPIId($select=uniquename))");

            existingAssembly = assemblyQuery?.Value?.SingleOrDefault();

            if (existingAssembly is null)
            {
                return PluginRegistrationResult.Failure("No existing plugin assembly found to unregister.");
            }

            foreach (var existingPlugin in existingAssembly.PluginTypes)
            {
                AddDeleteRequestsForPlugin(requests, existingPlugin);
            }
            if (existingAssembly.Package?.Id is not null)
            {
                requests.Add(new DeleteRequest(existingAssembly.Package.ToReference()));
            }
            else
            {
                requests.Add(new DeleteRequest(existingAssembly.ToReference()));
            }
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "An error occurred while querying existing registrations.");
            return PluginRegistrationResult.Failure("Plugin unregistration failed due to an error while querying existing registrations. " + ex.Message);
        }

        DataverseEnvironment? environment;
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

            batch = new BatchRequest(environment!.BaseServiceUrl!)
            {
                ChangeSets = [new(requests)]
            };
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "An error occurred while generating plugin registration requests.");
            return PluginRegistrationResult.Failure("Plugin registration failed due to an error while generating registration requests. " + ex.Message);
        }

        try
        {
            var batchResponse = await _webApi.SendAsync(batch!, noThrow: true, cancellationToken: cancellationToken).ConfigureAwait(false);
            var responses = await batchResponse.ParseResponseAsync(cancellationToken).ConfigureAwait(false);

            foreach (var response in responses)
            {
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.AsServiceExceptionAsync().ConfigureAwait(false);
                    _log.LogCritical(error.ToString());
                    return PluginRegistrationResult.Failure(error.Message);
                }
                else if (response.GetEntityReference() is EntityReference entityReference)
                {
                    _log.LogTrace($"Registered ({entityReference.Path}).");
                }
            }
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "An error occurred while sending the batch request.");
            return PluginRegistrationResult.Failure("Plugin unregistration failed. " + ex.Message);
        }

        return PluginRegistrationResult.Success(existingAssembly.Package?.Id is not null
        ? "Plugin package unregistered successfully."
        : "Plugin assembly unregistered successfully.");
    }

    public async Task<PluginRegistrationResult> RegisterAsync(RegistrationInput input, IPluginRegistrationUI ui, CancellationToken cancellationToken = default)
    {
        _log.LogInformation(
            "Starting plugin registration for {RegistrationTarget} '{TargetName}'.",
            input.IsProject ? "project" : "file",
            Path.GetFileName(input.ItemFullPath));

        PluginAssemblyConfig? model;
        try
        {
            _log.LogInformation("Reading plugin definitions from the {RegistrationTarget}.", input.IsProject ? "project" : "selected file");
            model = input.IsProject
                ? await _meta.ParseProjectPluginsAsync(input.ItemFullPath, cancellationToken)
                : await _meta.ParsePluginsAsync(input.ItemFullPath, cancellationToken);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "An error occurred while parsing registrations.");
            return PluginRegistrationResult.Failure("Plugin registration failed due to an error while parsing registrations. " + ex.Message);
        }

        if (model is null)
        {
            _log.LogWarning("Plugin registration stopped because no plugin definition was found.");
            return PluginRegistrationResult.Failure("No plugin definition found.");
        }

        _log.LogInformation(
            "Found plugin assembly '{AssemblyName}' with {PluginTypeCount} plugin type(s), {StepCount} step(s), and {CustomApiCount} custom API definition(s).",
            model.Name,
            model.PluginTypes.Count,
            model.PluginTypes.Sum(pluginType => pluginType.Steps.Count),
            model.PluginTypes.Sum(pluginType => pluginType.CustomApi is not null ? 1 : 0));

        try
        {
            if (!string.IsNullOrWhiteSpace(input.NugetPackagePath))
            {
                _log.LogInformation("Loading the plugin package generated by the project build.");
                model.Package = NugetParser.LoadFromNugetFile(input.NugetPackagePath!);
                await TryApplySolutionPrefixToPackageAsync(model, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                _log.LogInformation("Loading the compiled plugin assembly.");
                model.Content = Convert.ToBase64String(File.ReadAllBytes(model.FilePath));
            }
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "An error occurred while parsing plugin package/assembly content.");
            return PluginRegistrationResult.Failure("Plugin registration failed due to an error while parsing plugin package. " + ex.Message);
        }

        _log.LogInformation("Validating the plugin registration definitions.");
        var validation = await _validator.ValidateIfValidatorAvailableAsync(model, Validation.Categories.WebApi, cancellationToken);
        if (validation != ValidationResult.Success)
        {
            _log.LogWarning("Plugin registration validation failed: {ValidationMessage}", validation?.ErrorMessage ?? "Validation failed.");
            return PluginRegistrationResult.Failure(validation?.ErrorMessage ?? "Validation failed.");
        }
        _log.LogInformation("Plugin registration validation succeeded.");

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
                _log.LogInformation("Found existing assembly '{AssemblyName}' ({AssemblyId}); registration will update it.", model.Name, existingAssembly.Id);

                // A plugin type is only genuinely "removed" when its class no longer exists in the
                // compiled assembly (i.e. it was renamed or deleted in code). Plugin types that are
                // still compiled into the assembly remain valid registrations even when they are not
                // (yet) annotated, so they must never be deleted. When the compiled plugin-type set is
                // unknown (null), we conservatively treat nothing as removed. Steps, images and custom
                // APIs are still reconciled against annotations by GenerateDeleteRequestsForCleanup.
                var removedPlugins = ComputeRemovedPluginTypes(existingAssembly.PluginTypes, model.AssemblyPluginTypeNames);

                if (removedPlugins.Count > 0)
                {
                    _log.LogInformation("Found {RemovedPluginCount} plugin type(s) that are no longer present in the compiled assembly.", removedPlugins.Count);
                    var summaries = removedPlugins.Select(ToRemovedPluginSummary).ToArray();
                    var decision = await ui.ConfirmRemovedPluginsAsync(summaries);
                    if (decision == RemovedPluginsDecision.Cancel)
                    {
                        _log.LogWarning("Plugin registration was cancelled when removal of obsolete registrations was declined.");
                        return PluginRegistrationResult.Success("Plugin registration was cancelled.");
                    }

                    _log.LogInformation("The removal of obsolete plugin registrations was confirmed.");
                }
                else
                {
                    _log.LogInformation("No obsolete plugin registrations were found.");
                }

                requests.AddRange(GenerateDeleteRequestsForCleanup(
                    newAssembly: model,
                    existingAssembly: existingAssembly,
                    removedPlugins: removedPlugins));
                _log.LogInformation($"Generated {requests.Count} delete requests for cleanup.");
            }
            else
            {
                _log.LogInformation("No existing assembly was found; a new registration will be created.");
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
                _log.LogWarning("Plugin registration stopped: {Reason}", errMessage);
                return PluginRegistrationResult.Failure(errMessage);
            }

            _log.LogInformation("Connected to the active Dataverse environment.");
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

            _log.LogInformation(
                "Prepared {RequestCount} Dataverse request(s) for the initial registration batch.",
                requests.Count);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "An error occurred while generating plugin registration requests.");
            return PluginRegistrationResult.Failure("Plugin registration failed due to an error while generating registration requests. " + ex.Message);
        }

        try
        {
            _log.LogInformation("Sending the initial plugin registration batch.");
            var batchResponse = await _webApi.SendAsync(batch!, noThrow: true, cancellationToken: cancellationToken).ConfigureAwait(false);
            var responses = await batchResponse.ParseResponseAsync(cancellationToken).ConfigureAwait(false);
            _log.LogInformation("Initial registration batch completed with {ResponseCount} response(s).", responses.Count);

            foreach (var response in responses)
            {
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.AsServiceExceptionAsync().ConfigureAwait(false);
                    _log.LogCritical(error.ToString());
                    return PluginRegistrationResult.Failure(error.Message);
                }
                else if (response.GetEntityReference() is EntityReference entityReference)
                {
                    _log.LogTrace($"Registered ({entityReference.Path}).");
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
                _log.LogInformation("Preparing the follow-up registration of plugin steps and custom APIs.");
                var assemblyQuery = await _webApi.RetrieveMultipleAsync<PluginAssembly>(
                    $"{PluginAssembly.Metadata.EntitySetName}?$select=name" +
                    $"&$filter=name eq '{model.Name}'" +
                    $"&$expand=PackageId($select=name),pluginassembly_plugintype($select=name,typename" +
                    $";$expand=plugintype_sdkmessageprocessingstep($select=name,stage),CustomAPIId($select=uniquename))");

                var existing = assemblyQuery?.Value?.SingleOrDefault();
                AssignIds(model, existing);

                var builder = new UpsertRequestBuilder(model, sdkMessages!);
                var upserts = builder.WithStepsAndCustomApis().Build();
                _log.LogInformation("Prepared {RequestCount} follow-up request(s) for plugin steps and custom APIs.", upserts.Count);

                var followupBatch = new BatchRequest(environment.BaseServiceUrl!)
                {
                    ChangeSets = [new(upserts)]
                };

                var followupResponse = await _webApi.SendAsync(followupBatch, noThrow: true, cancellationToken: cancellationToken).ConfigureAwait(false);
                var followupParts = await followupResponse.ParseResponseAsync(cancellationToken).ConfigureAwait(false);
                _log.LogInformation("Follow-up registration batch completed with {ResponseCount} response(s).", followupParts.Count);

                foreach (var response in followupParts)
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        var error = await response.AsServiceExceptionAsync().ConfigureAwait(false);
                        _log.LogCritical(error.ToString());
                        return PluginRegistrationResult.Failure(error.Message);
                    }
                    else if (response.GetEntityReference() is EntityReference entityReference)
                    {
                        _log.LogTrace($"Registered ({entityReference.Path}).");
                    }
                }
            }
            catch (Exception ex)    
            {
                _log.LogError(ex, "An error occurred while registering steps/custom APIs after package upload.");
                return PluginRegistrationResult.Failure("Plugin registration failed during follow-up registration of steps/custom APIs. Please check the Output window for more details.");
            }
        }

        _log.LogInformation("Plugin assembly '{AssemblyName}' registered successfully.", model.Name);
        return PluginRegistrationResult.Success();
    }

    private async Task<Dictionary<string, SdkMessage>> FetchSdkMessagesAsync(PluginAssemblyConfig config, CancellationToken cancellationToken)
    {
        var stepEntities = config.PluginTypes
            .SelectMany(p => p.Steps.Select(s => s.PrimaryEntityName)
            .Where(s => !string.IsNullOrEmpty(s)))
            .Distinct()
            .Union(["none"])
            .ToArray();

        if (stepEntities == null || stepEntities.Length == 0)
            return new Dictionary<string, SdkMessage>(StringComparer.OrdinalIgnoreCase);

        _log.LogTrace($"Fetching SDK Messages for entities: {string.Join(", ", stepEntities)}");

        using var messageRepo = _repositoryFactory.CreateRepository<ISdkMessageRepository>();
        var messages = await messageRepo.GetForEntitiesAsync(stepEntities!, cancellationToken).ConfigureAwait(false);

        return messages.ToDictionary(m => m.Name, m => m, StringComparer.OrdinalIgnoreCase);
    }

    private async Task TryApplySolutionPrefixToPackageAsync(PluginAssemblyConfig config, CancellationToken cancellationToken)
    {
        var package = config.Package;
        var solutionUniqueName = config.Solution?.UniqueName;

        if (package is null || string.IsNullOrWhiteSpace(package.Name) || string.IsNullOrWhiteSpace(solutionUniqueName))
        {
            return;
        }

        try
        {
            var packagePrefix = await GetSolutionPackagePrefixAsync(solutionUniqueName, cancellationToken).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(packagePrefix))
            {
                _log.LogTrace("No publisher customization prefix found for solution '{SolutionUniqueName}'.", solutionUniqueName);
                return;
            }

            var originalPackageName = package.Name;
            package.Name = NugetParser.EnsurePackagePrefix(package.Name, packagePrefix);

            if (!string.Equals(originalPackageName, package.Name, StringComparison.Ordinal))
            {
                _log.LogInformation(
                    "Auto-detected publisher prefix '{PackagePrefix}' for solution '{SolutionUniqueName}' and updated plugin package name from '{OriginalPackageName}' to '{UpdatedPackageName}'.",
                    packagePrefix,
                    solutionUniqueName,
                    originalPackageName,
                    package.Name);
            }
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "Could not auto-detect the plugin package prefix for solution '{SolutionUniqueName}'.", solutionUniqueName);
        }
    }

    private async Task<string?> GetSolutionPackagePrefixAsync(string solutionUniqueName, CancellationToken cancellationToken)
    {
        var solutionQuery = await _webApi.RetrieveMultipleAsync<Solution>(
            $"{Solution.Metadata.EntitySetName}?$select=solutionid,uniquename" +
            $"&$filter=uniquename eq '{EscapeODataString(solutionUniqueName)}'" +
            "&$expand=publisherid($select=customizationprefix)",
            cancellationToken: cancellationToken).ConfigureAwait(false);

        return solutionQuery?.Value?.SingleOrDefault()?.Publisher?.CustomizationPrefix;
    }

    private static string EscapeODataString(string value) => value.Replace("'", "''");

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
        PluginAssemblyConfig newAssembly, PluginAssembly existingAssembly, IReadOnlyList<PluginType> removedPlugins)
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
            else if (removedPlugins.Contains(existingPlugin))
            {
                AddDeleteRequestsForPlugin(deleteRequests, existingPlugin);
            }
        }

        return deleteRequests;
    }

    private static RemovedPluginSummary ToRemovedPluginSummary(PluginType plugin)
        => new(
            plugin.TypeName ?? plugin.Name ?? string.Empty,
            plugin.Steps?.Count ?? 0,
            plugin.CustomApi is { Count: > 0 });

    /// <summary>
    /// Determines which existing Dataverse plugin types have genuinely been removed from the compiled
    /// assembly (renamed or deleted in code) and are therefore eligible for deletion. A plugin type is
    /// considered removed only when its <c>TypeName</c> is absent from <paramref name="assemblyPluginTypeNames"/>.
    /// When <paramref name="assemblyPluginTypeNames"/> is <see langword="null"/> (the compiled plugin-type
    /// set could not be determined), nothing is treated as removed so that valid registrations are never
    /// deleted.
    /// </summary>
    internal static IReadOnlyList<PluginType> ComputeRemovedPluginTypes(
        IEnumerable<PluginType> existingPluginTypes, ISet<string>? assemblyPluginTypeNames)
    {
        if (assemblyPluginTypeNames is null)
        {
            return [];
        }

        return existingPluginTypes
            .Where(existing =>
                !string.IsNullOrEmpty(existing.Name) &&
                !string.IsNullOrEmpty(existing.TypeName) &&
                !assemblyPluginTypeNames.Contains(existing.TypeName!))
            .ToArray();
    }

    private static void AddDeleteRequestsForPlugin(List<HttpRequestMessage> requests, PluginType existingPlugin)
    {
        foreach (var step in existingPlugin.Steps)
        {
            if (step.Stage != Stages.MainOperation && step.Id.HasValue)
            {
                requests.Add(new DeleteRequest(SdkMessageProcessingStep.CreateReference(step.Id!.Value)));
            }
        }

        if (existingPlugin.CustomApi is not null)
        {
            foreach (var customApi in existingPlugin.CustomApi)
            {
                if (customApi.Id.HasValue)
                {
                    requests.Add(new DeleteRequest(CustomApi.CreateReference(customApi.Id!.Value)));
                }
            }
        }

        if (existingPlugin.Id.HasValue)
        {
            requests.Add(new DeleteRequest(PluginType.CreateReference(existingPlugin.Id!.Value)));
        }
    }
}
#nullable restore