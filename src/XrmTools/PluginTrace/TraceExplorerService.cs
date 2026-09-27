#nullable enable
namespace XrmTools.PluginTrace;

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.Environments;
using XrmTools.Http;
using XrmTools.OData;

[Export]
internal sealed class TraceExplorerService
{
    private readonly IEnvironmentSelection environments;
    private readonly IXrmHttpClientFactory authentication;
    private readonly IODataTransport transport;
    internal const int MaximumRecords = 2000;

    [ImportingConstructor]
    public TraceExplorerService(IEnvironmentSelection environments, IXrmHttpClientFactory authentication, IODataTransport transport)
    {
        this.environments = environments;
        this.authentication = authentication;
        this.transport = transport;
    }

    public Task<DataverseEnvironment?> GetEnvironmentAsync() => environments.GetSelectedEnvironmentAsync();

    private async Task<ODataResponse> SendAsync(DataverseEnvironment environment, ODataRequest request, bool interactive, CancellationToken cancellation)
    {
        cancellation.ThrowIfCancellationRequested();
        await VerifyEnvironmentAsync(environment, cancellation);
        using var validation = ODataRequestBuilder.Build(request, environment.BaseServiceUrl!, "validation");
        var token = await authentication.PreAuthenticateAsync(environment, interactive, cancellation);
        if (token == null || string.IsNullOrEmpty(token.AccessToken) || token.ExpiresOn <= DateTimeOffset.UtcNow)
            throw new InvalidOperationException("Sign in using Apply or Refresh to continue.");
        cancellation.ThrowIfCancellationRequested();
        await VerifyEnvironmentAsync(environment, cancellation);
        using var message = ODataRequestBuilder.Build(request, environment.BaseServiceUrl!, token.AccessToken);
        var response = await transport.SendAsync(message, cancellation).ConfigureAwait(false);
        await VerifyEnvironmentAsync(environment, cancellation).ConfigureAwait(false);
        if (response.StatusCode < 200 || response.StatusCode >= 300)
        {
            var error = response.Status;
            try
            {
                using var json = JsonDocument.Parse(response.Body);
                error += ": " + json.RootElement.GetProperty("error").GetProperty("message").GetString();
            }
            catch (JsonException) { }
            catch (KeyNotFoundException) { }
            catch (InvalidOperationException) { }
            throw new InvalidOperationException(error);
        }
        return response;
    }

    private async Task<string> GetAsync(DataverseEnvironment environment, string target, bool interactive, CancellationToken cancellation)
    {
        var request = new ODataRequest { Target = target };
        request.Headers.Add(new KeyValuePair<string, string>("Prefer", "odata.maxpagesize=250"));
        return (await SendAsync(environment, request, interactive, cancellation).ConfigureAwait(false)).Body;
    }

    private async Task VerifyEnvironmentAsync(DataverseEnvironment environment, CancellationToken cancellation)
    {
        cancellation.ThrowIfCancellationRequested();
        if ((await environments.GetSelectedEnvironmentAsync())?.ConnectionString != environment.ConnectionString)
            throw new InvalidOperationException("Environment changed. Apply the filter again.");
        cancellation.ThrowIfCancellationRequested();
    }

    public async Task<TraceQueryResult> QueryAsync(DataverseEnvironment environment, TraceFilter filter, bool interactive, CancellationToken cancellation)
    {
        string? target = filter.Query(DateTimeOffset.UtcNow);
        var records = new List<TraceRecord>();
        var visited = new HashSet<string>(StringComparer.Ordinal);
        while (!string.IsNullOrEmpty(target))
        {
            if (!visited.Add(target!)) throw new InvalidOperationException("The server returned a repeated results page.");
            using var json = JsonDocument.Parse(await GetAsync(environment, target!, interactive, cancellation).ConfigureAwait(false));
            records.AddRange(json.RootElement.GetProperty("value").EnumerateArray().Select(row => new TraceRecord(row)));
            target = json.RootElement.TryGetProperty("@odata.nextLink", out var next) ? next.GetString() : null;
            if (records.Count >= MaximumRecords)
                return new TraceQueryResult(TraceSnapshots.Order(records.Take(MaximumRecords)), target != null || records.Count > MaximumRecords);
        }
        return new TraceQueryResult(TraceSnapshots.Order(records), false);
    }

    public async Task<string> DetailAsync(DataverseEnvironment environment, Guid id, CancellationToken cancellation)
    {
        using var json = JsonDocument.Parse(await GetAsync(environment, $"plugintracelogs({id:D})", true, cancellation).ConfigureAwait(false));
        return JsonSerializer.Serialize(json.RootElement, new JsonSerializerOptions { WriteIndented = true });
    }

    public async Task<TraceLoggingConfiguration> GetTraceLoggingAsync(DataverseEnvironment environment, CancellationToken cancellation)
    {
        using var json = JsonDocument.Parse(await GetAsync(environment, "organizations?$select=organizationid,plugintracelogsetting&$top=1", true, cancellation).ConfigureAwait(false));
        var organization = json.RootElement.GetProperty("value").EnumerateArray().FirstOrDefault();
        if (organization.ValueKind != JsonValueKind.Object || !organization.TryGetProperty("organizationid", out var idValue) || !Guid.TryParse(idValue.GetString(), out var id))
            throw new InvalidOperationException("The selected environment did not return its organization settings.");
        var value = organization.TryGetProperty("plugintracelogsetting", out var setting) ? setting.GetInt32() : 0;
        if (value is < 0 or > 2) throw new InvalidOperationException("The environment returned an unknown plug-in trace logging setting.");
        return new TraceLoggingConfiguration(id, (TraceLoggingMode)value);
    }

    public async Task SetTraceLoggingAsync(DataverseEnvironment environment, Guid organizationId, TraceLoggingMode mode, CancellationToken cancellation)
    {
        var request = new ODataRequest { Method = "PATCH", Target = $"organizations({organizationId:D})", Body = $"{{\"plugintracelogsetting\":{(int)mode}}}" };
        request.Headers.Add(new KeyValuePair<string, string>("If-Match", "*"));
        await SendAsync(environment, request, true, cancellation).ConfigureAwait(false);
    }
}

internal sealed record TraceQueryResult(IReadOnlyList<TraceRecord> Records, bool Truncated);
internal sealed record TraceLoggingConfiguration(Guid OrganizationId, TraceLoggingMode Mode);
internal enum TraceLoggingMode { Off = 0, Exceptions = 1, All = 2 }
