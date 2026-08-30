#nullable enable
namespace XrmTools.Services;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.Environments;
using XrmTools.Helpers;
using XrmTools.Logging.Compatibility;
using XrmTools.UI;
using XrmTools.WebApi;
using XrmTools.WebApi.Batch;
using XrmTools.WebApi.Entities;
using XrmTools.WebApi.Messages;
using XrmTools.WebApi.Methods;
using XrmTools.WebApi.Types;

internal sealed class WebResourceRegistrationResult(bool succeeded, string message)
{
    public bool Succeeded { get; } = succeeded;
    public string Message { get; } = message;

    public static WebResourceRegistrationResult Success(string message) => new(true, message);
    public static WebResourceRegistrationResult Failure(string message) => new(false, message);
}

internal interface IWebResourceRegistrationService
{
    Task<WebResourceRegistrationResult> RegisterAsync(
        string projectFilePath,
        string configurationName,
        IWebResourceRegistrationUI ui,
        CancellationToken cancellationToken = default);
}

[Export(typeof(IWebResourceRegistrationService))]
[method: ImportingConstructor]
internal sealed class WebResourceRegistrationService(
    IWebApiService webApi,
    IEnvironmentProvider environmentProvider,
    IWebResourceProjectManifestReader manifestReader,
    ILogger<WebResourceRegistrationService> log) : IWebResourceRegistrationService
{
    private const int MaxBatchOperations = 1000;

    public async Task<WebResourceRegistrationResult> RegisterAsync(
        string projectFilePath,
        string configurationName,
        IWebResourceRegistrationUI ui,
        CancellationToken cancellationToken = default)
    {
        WebResourceProjectManifest manifest;
        try
        {
            manifest = await manifestReader.ReadAsync(
                projectFilePath,
                configurationName,
                cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Web-resource registration stopped while reading the project manifest.");
            return WebResourceRegistrationResult.Failure(
                "Web-resource registration could not read the project output. " + ex.Message);
        }

        DataverseEnvironment? environment;
        try
        {
            environment = await environmentProvider.GetActiveEnvironmentAsync(true).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Web-resource registration could not resolve the active environment.");
            return WebResourceRegistrationResult.Failure("Could not resolve the active Dataverse environment. " + ex.Message);
        }

        if (environment?.BaseServiceUrl is null)
            return WebResourceRegistrationResult.Failure(
                "No active Dataverse environment is selected, or the selected environment has no valid URL.");

        IReadOnlyList<WebResource> existing;
        try
        {
            var escapedSolutionName = EscapeODataString(manifest.SolutionUniqueName);
            var solutions = await webApi.QueryAsync<Solution>(
                $"{Solution.Metadata.EntitySetName}?$select=solutionid,uniquename" +
                $"&$filter=uniquename eq '{escapedSolutionName}' and ismanaged eq false",
                cancellationToken).ConfigureAwait(false);
            if (solutions.Value?.SingleOrDefault() is null)
            {
                return WebResourceRegistrationResult.Failure(
                    $"The unmanaged Dataverse solution '{manifest.SolutionUniqueName}' was not found.");
            }

            var escapedPrefix = EscapeODataString(manifest.NamePrefix);
            var response = await webApi.QueryAsync<WebResource>(
                $"{WebResource.Metadata.EntitySetName}?$select=webresourceid,name,displayname,description,webresourcetype,content,ismanaged" +
                $"&$filter=startswith(name,'{escapedPrefix}')",
                cancellationToken).ConfigureAwait(false);
            existing = response.Value?.ToArray() ?? [];
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Web-resource registration could not query the current Dataverse state.");
            return WebResourceRegistrationResult.Failure("Could not retrieve existing web resources. " + ex.Message);
        }

        var managed = existing.Where(resource => resource.IsManaged is true).Select(resource => resource.Name).ToArray();
        if (managed.Length > 0)
        {
            return WebResourceRegistrationResult.Failure(
                "The configured ownership prefix contains managed web resources and cannot be reconciled: " +
                string.Join(", ", managed));
        }

        WebResourceRegistrationDelta delta;
        try
        {
            var desired = manifest.Resources.Select(ToDesiredResource).ToArray();
            delta = CalculateDelta(desired, existing);
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Web-resource registration could not calculate the deployment delta.");
            return WebResourceRegistrationResult.Failure("Could not calculate the web-resource deployment delta. " + ex.Message);
        }

        if (delta.Deletes.Count > 0
            && !await ui.ConfirmDeleteRemovedWebResourcesAsync(
                delta.Deletes.Select(resource => resource.Name ?? resource.Id?.ToString() ?? "Unknown").ToArray()))
        {
            return WebResourceRegistrationResult.Success("Web-resource registration was cancelled.");
        }

        var requests = BuildRequests(delta, manifest.SolutionUniqueName);
        if (requests.Count == 0)
        {
            return WebResourceRegistrationResult.Success(
                $"All {manifest.Resources.Count} web resource(s) are already up to date.");
        }

        try
        {
            foreach (var chunk in requests.ChunkBy(MaxBatchOperations))
            {
                var batch = new BatchRequest(environment.BaseServiceUrl)
                {
                    ChangeSets = [new ChangeSet(chunk)]
                };
                var batchResponse = await webApi.SendAsync(
                    batch,
                    noThrow: true,
                    cancellationToken: cancellationToken).ConfigureAwait(false);
                var responses = await batchResponse.ParseResponseAsync(cancellationToken).ConfigureAwait(false);
                foreach (var response in responses)
                {
                    if (response.IsSuccessStatusCode) continue;
                    var error = await response.AsServiceExceptionAsync().ConfigureAwait(false);
                    log.LogCritical(error.ToString());
                    return WebResourceRegistrationResult.Failure(error.Message);
                }
            }

            var publishIds = delta.Creates.Select(resource => resource.Id)
                .Concat(delta.Updates.Select(update => update.Desired.Id))
                .Concat(delta.Deletes.Where(resource => resource.Id.HasValue).Select(resource => resource.Id!.Value))
                .Distinct()
                .ToArray();
            if (publishIds.Length > 0)
                await PublishAsync(publishIds, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            log.LogError(ex, "An error occurred while applying the web-resource deployment delta.");
            return WebResourceRegistrationResult.Failure("Web-resource registration failed. " + ex.Message);
        }

        return WebResourceRegistrationResult.Success(
            $"Web resources registered successfully: {delta.Creates.Count} created, " +
            $"{delta.Updates.Count} updated, {delta.Deletes.Count} deleted, {delta.UnchangedCount} unchanged.");
    }

    internal static WebResourceRegistrationDelta CalculateDelta(
        IReadOnlyList<DesiredWebResource> desired,
        IReadOnlyList<WebResource> existing)
    {
        var existingByName = existing.Where(resource => !string.IsNullOrWhiteSpace(resource.Name))
            .GroupBy(resource => resource.Name!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group.Single(),
                StringComparer.OrdinalIgnoreCase);
        var desiredNames = new HashSet<string>(desired.Select(resource => resource.Name), StringComparer.OrdinalIgnoreCase);
        var creates = new List<DesiredWebResource>();
        var updates = new List<WebResourceUpdate>();
        var unchanged = 0;

        foreach (var resource in desired)
        {
            if (!existingByName.TryGetValue(resource.Name, out var current))
            {
                creates.Add(resource);
                continue;
            }
            if (!current.Id.HasValue)
                throw new InvalidOperationException($"Existing web resource '{resource.Name}' has no ID.");

            resource.Id = current.Id.Value;
            if (IsEquivalent(resource, current)) unchanged++;
            else updates.Add(new WebResourceUpdate(resource, current));
        }

        var deletes = existing.Where(resource =>
            !string.IsNullOrWhiteSpace(resource.Name)
            && !desiredNames.Contains(resource.Name!)).ToArray();
        return new WebResourceRegistrationDelta(creates, updates, deletes, unchanged);
    }

    private static DesiredWebResource ToDesiredResource(WebResourceDefinition definition)
        => new(
            GuidFactory.DeterministicGuid(GuidFactory.Namespace.WebResource, definition.Name.ToLowerInvariant()),
            definition.Name,
            definition.DisplayName,
            definition.Description,
            definition.Type,
            Convert.ToBase64String(File.ReadAllBytes(definition.FilePath)));

    private static bool IsEquivalent(DesiredWebResource desired, WebResource existing)
        => string.Equals(desired.Name, existing.Name, StringComparison.Ordinal)
        && string.Equals(desired.DisplayName, existing.DisplayName, StringComparison.Ordinal)
        && string.Equals(desired.Description ?? string.Empty, existing.Description ?? string.Empty, StringComparison.Ordinal)
        && desired.Type == existing.WebResourceType
        && string.Equals(desired.Content, existing.Content, StringComparison.Ordinal);

    private static List<HttpRequestMessage> BuildRequests(
        WebResourceRegistrationDelta delta,
        string solutionUniqueName)
    {
        var requests = new List<HttpRequestMessage>();
        requests.AddRange(delta.Deletes.Where(resource => resource.Id.HasValue)
            .Select(resource => (HttpRequestMessage)new DeleteRequest(resource.ToReference(), eTag: resource.ODataETag)));
        requests.AddRange(delta.Creates.Select(resource => CreateUpsertRequest(
            resource,
            UpsertBehavior.PreventUpdate,
            solutionUniqueName)));
        requests.AddRange(delta.Updates.Select(update => CreateUpsertRequest(
            update.Desired,
            UpsertBehavior.PreventCreate,
            solutionUniqueName)));
        return requests;
    }

    private static UpsertRequest CreateUpsertRequest(
        DesiredWebResource resource,
        UpsertBehavior behavior,
        string solutionUniqueName)
        => new(
            WebResource.CreateReference(resource.Id),
            new JObject
            {
                ["name"] = resource.Name,
                ["displayname"] = resource.DisplayName,
                ["description"] = resource.Description,
                ["webresourcetype"] = (int)resource.Type,
                ["languagecode"] = 0,
                ["content"] = resource.Content
            },
            behavior,
            solutionUniqueName);

    private async Task PublishAsync(IReadOnlyCollection<Guid> webResourceIds, CancellationToken cancellationToken)
    {
        var parameterXml = "<importexportxml><webresources>" +
            string.Concat(webResourceIds.Select(id => $"<webresource>{{{id.ToString("D")}}}</webresource>")) +
            "</webresources></importexportxml>";
        using var request = new HttpRequestMessage(HttpMethod.Post, new Uri("PublishXml", UriKind.Relative))
        {
            Content = new StringContent(
                new JObject { ["ParameterXml"] = parameterXml }.ToString(Formatting.None),
                Encoding.UTF8,
                "application/json")
        };
        using var response = await webApi.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    private static string EscapeODataString(string value) => value.Replace("'", "''");
}

internal sealed class DesiredWebResource(
    Guid id,
    string name,
    string displayName,
    string? description,
    WebResourceType type,
    string content)
{
    public Guid Id { get; set; } = id;
    public string Name { get; } = name;
    public string DisplayName { get; } = displayName;
    public string? Description { get; } = description;
    public WebResourceType Type { get; } = type;
    public string Content { get; } = content;
}

internal sealed class WebResourceUpdate(DesiredWebResource desired, WebResource existing)
{
    public DesiredWebResource Desired { get; } = desired;
    public WebResource Existing { get; } = existing;
}

internal sealed class WebResourceRegistrationDelta(
    IReadOnlyList<DesiredWebResource> creates,
    IReadOnlyList<WebResourceUpdate> updates,
    IReadOnlyList<WebResource> deletes,
    int unchangedCount)
{
    public IReadOnlyList<DesiredWebResource> Creates { get; } = creates;
    public IReadOnlyList<WebResourceUpdate> Updates { get; } = updates;
    public IReadOnlyList<WebResource> Deletes { get; } = deletes;
    public int UnchangedCount { get; } = unchangedCount;
}

internal static class WebResourceRegistrationEnumerableExtensions
{
    public static IEnumerable<List<T>> ChunkBy<T>(this IReadOnlyList<T> source, int size)
    {
        for (var index = 0; index < source.Count; index += size)
            yield return source.Skip(index).Take(Math.Min(size, source.Count - index)).ToList();
    }
}
#nullable restore
