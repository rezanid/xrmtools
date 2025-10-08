#nullable enable
namespace XrmTools.WebApi;

using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.Environments;
using XrmTools.Http;
using XrmTools.Logging.Compatibility;
using XrmTools.WebApi.Entities;

public interface IWebApiService
{
    Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken);
    Task<HttpResponseMessage> GetAsync(string uri, CancellationToken cancellationToken);
    Task<ODataQueryResponse<T>> QueryAsync<T>(string odataQuery, CancellationToken cancellationToken = default) where T : Entity<T>;
    Task<T> SendAsync<T>(HttpRequestMessage request) where T : HttpResponseMessage;
    Task<Uri?> GetBaseUrlAsync();
}

[Export(typeof(IWebApiService))]
[method: ImportingConstructor]
public class WebApiService(
    IXrmHttpClientFactory httpClientFactory, IEnvironmentProvider environmentProvider, ILogger<WebApiService> logger) : IWebApiService
{
    private string? _sessionToken = null;

    public async Task<Uri?> GetBaseUrlAsync() => (await environmentProvider.GetActiveEnvironmentAsync().ConfigureAwait(false))?.BaseServiceUrl;

    /// <summary>
    /// Processes requests and returns responses. Manages Service Protection Limit errors.
    /// </summary>
    /// <param name="request">The request to send.</param>
    /// <returns>The response from the HttpClient</returns>
    /// <exception cref="Exception"></exception>
    public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
    {
        // Session token used by elastic tables to enable strong consistency
        // See https://learn.microsoft.com/power-apps/developer/data-platform/use-elastic-tables?tabs=webapi#sending-the-session-token
        if (!string.IsNullOrWhiteSpace(_sessionToken) && request.Method == HttpMethod.Get) {
            request.Headers.Add("MSCRM.SessionToken", _sessionToken);
        }

        using var client = await httpClientFactory.CreateClientAsync().ConfigureAwait(false);

        // without buffering, it proved to be unreliable. The request content would be not readable.
        //var response =  await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead,  cancellationToken).ConfigureAwait(false);
        //await response.Content.LoadIntoBufferAsync().ConfigureAwait(false);

        var response =  await client.SendAsync(request, cancellationToken).ConfigureAwait(false);

        // Capture the current session token value
        // See https://learn.microsoft.com/power-apps/developer/data-platform/use-elastic-tables?tabs=webapi#getting-the-session-token
        if (response.Headers.Contains("x-ms-session-token"))
        {
            _sessionToken = response.Headers.GetValues("x-ms-session-token")?.FirstOrDefault()?.ToString();
        }

        if (RecommendedDegreeOfParallelism <= 0 && 
            response.Headers.TryGetValues("x-ms-dop-hint", out var maxDopValues) &&
            maxDopValues.FirstOrDefault() is string maxDopValue &&
            int.TryParse(maxDopValue, out var maxDopInt))
        {
            RecommendedDegreeOfParallelism = maxDopInt;
        }

        if (!response.IsSuccessStatusCode && !response.Content.IsMimeMultipartContent())
        {
            var exception = await response.AsServiceExceptionAsync();
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            logger.LogError(exception, "Error in Web API call.");//: {Message}", exception.Message);
            throw exception;
        }
        return response;
    }

    /// <summary>
    /// Processes requests with typed responses
    /// </summary>
    /// <typeparam name="T">The type derived from HttpResponseMessage</typeparam>
    /// <param name="request">The request</param>
    /// <returns></returns>
    public async Task<T> SendAsync<T>(HttpRequestMessage request) where T : HttpResponseMessage
    {
        var response = await SendAsync(request).ConfigureAwait(false);

        // 'As' method is Extension of HttpResponseMessage see Extensions.cs
        return response.As<T>();
    }

    public async Task<TResponse> SendAsync<TResponse>(
        WebApiRequest<TResponse> request,
        CancellationToken ct = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        using var raw = await SendAsync(request as HttpRequestMessage, ct)
            .ConfigureAwait(false);
        // Optional policy: move/replace with your own error handling.
        raw.EnsureSuccessStatusCode();

        // Let the request create the typed response (no reflection).
        return await request.CreateResponseAsync(raw, ct).ConfigureAwait(false);
    }

    public async Task<HttpResponseMessage> GetAsync(string uri, CancellationToken cancellationToken = default)
        => await SendAsync(new HttpRequestMessage(HttpMethod.Get, new Uri(uri, UriKind.Relative)), cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// Performs an query with a given OData query string and deserialized the result.
    /// </summary>
    /// <typeparam name="T">Type of the entity to serialize the result of the query.</typeparam>
    /// <param name="odataQuery">OData query.</param>
    /// <returns>OData response that include deserialized list of records in <see cref="ODataQueryResponse{T}.Value"/> property.</returns>
    public async Task<ODataQueryResponse<T>> QueryAsync<T>(string odataQuery, CancellationToken cancellationToken = default) where T : Entity<T>
    {
        var response = await GetAsync(odataQuery, cancellationToken).ConfigureAwait(false);
        return await response.CastAsync<ODataQueryResponse<T>>().ConfigureAwait(false);
    }

    /// <summary>
    /// The recommended degree of parallelism for the connection.
    /// </summary>
    public int RecommendedDegreeOfParallelism { get; private set; }
}
#nullable restore