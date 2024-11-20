#nullable enable
namespace XrmTools.Http;
using System;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading;
using Polly;
using Microsoft.Extensions.Http;
using XrmTools.Environments;
using XrmTools.Logging.Compatibility;
using System.ComponentModel.Composition;
using XrmTools.Authentication;
using System.Net.Http.Headers;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Identity.Client;
using Microsoft.VisualStudio.Threading;

[method: ImportingConstructor]
internal class XrmHttpClientFactory(
    TimeProvider timeProvider,
    IEnvironmentProvider environmentProvider,
    IAuthenticationService authenticationService,
    ILogger<XrmHttpClientFactory> logger) : IXrmHttpClientFactory, IAsyncDisposable, IDisposable
{
    private readonly ConcurrentDictionary<DataverseEnvironment, HttpClientConfig> _clientConfigs = new();
    private readonly ConcurrentDictionary<DataverseEnvironment, HttpClientEntry> _clients = new();
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly Timer timer = new AsyncTimer(async _ => await RecycleHandlersAsync(), TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1), timeProvider);
    private readonly ConcurrentDictionary<DataverseEnvironment, Lazy<HttpMessageHandlerEntry>> _handlerPool = new();
    private readonly ConcurrentDictionary<string, AuthenticationResult> _tokenCache = new();
    private readonly TimeProvider timeProvider = timeProvider;
    private readonly IEnvironmentProvider environmentProvider = environmentProvider;
    private readonly IAuthenticationService authenticationService = authenticationService;
    private readonly ILogger<XrmHttpClientFactory> logger = logger;
    private bool disposedValue;

    public async Task<XrmHttpClient> CreateHttpClientAsync()
    {
        var environment = await environmentProvider.GetActiveEnvironmentAsync();
        return environment == null ? throw new InvalidOperationException("No environment selected.") : await CreateHttpClientAsync(environment);
    }

    public async Task<XrmHttpClient> CreateHttpClientAsync(DataverseEnvironment environment)
    {
        if (string.IsNullOrEmpty(environment.ConnectionString))
        {
            throw new InvalidOperationException($"Environment '{environment.Name}' connection string is empty.");
        }
        var handlerEntry = _handlerPool.GetOrAdd(environment, _ => new Lazy<HttpMessageHandlerEntry>(() => CreateHandlerEntry())).Value;

        handlerEntry.IncrementUsage();
        var client = new XrmHttpClient(handlerEntry.Handler, handlerEntry.DecrementUsage);
        await ConfigureClientAsync(client, environment);

        _ = RemoveHandlerIfUnusedAsync(environment, handlerEntry);

        return client;
    }

    private async Task ConfigureClientAsync(XrmHttpClient client, DataverseEnvironment environment)
    {
        client.Timeout = TimeSpan.FromSeconds(100);
        var authParams = AuthenticationParameters.Parse(environment.ConnectionString);

        if (!_tokenCache.TryGetValue(environment.ConnectionString!, out var authResult) || authResult.ExpiresOn <= timeProvider.GetUtcNow())
        {
            authResult = await authenticationService.AuthenticateAsync(authParams, msg => logger.LogInformation(msg), CancellationToken.None);
            _tokenCache[environment.ConnectionString!] = authResult;
        }
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);
    }

    private HttpMessageHandlerEntry CreateHandlerEntry()
    {
        var handler = new PolicyHttpMessageHandler(CreateDefaultPolicy());
        return new (handler, timeProvider.GetUtcNow());
    }

    private async Task RecycleHandlersAsync()
    {
        try
        {
            foreach (var kvp in _handlerPool)
            {
                var name = kvp.Key;
                var entry = kvp.Value;
                if (_clientConfigs.TryGetValue(name, out var config) && config != null && entry.IsValueCreated)
                {
                    if (entry.Value.CanDispose())
                    {
                        await _semaphore.WaitAsync();
                        try
                        {
                            _handlerPool[name] = new Lazy<HttpMessageHandlerEntry>(CreateHandlerEntry);
                            //if (_clients.TryUpdate(name, new HttpClientEntry(new AsyncLazy<HttpClient>(async () => await CreateHttpClientAsync(config), null), timeProvider.GetUtcNow()), entry))
                            //{
                            //    await entry.Client.DisposeValueAsync();
                            //}
                        }
                        finally
                        {
                            _semaphore.Release();
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Exception during handler recycling: {ex.Message}");
        }
    }

    private async Task RemoveHandlerIfUnusedAsync(DataverseEnvironment environment, HttpMessageHandlerEntry handlerEntry)
    {
        await Task.Delay(handlerEntry.Lifetime);
        if (handlerEntry.CanDispose() && _handlerPool.TryRemove(environment, out var lazyHandler) && lazyHandler.IsValueCreated)
        {
            if (lazyHandler.Value.Handler is IAsyncDisposable asyncDisposable)
            {
                await asyncDisposable.DisposeAsync().ConfigureAwait(false);
            }
            else if (lazyHandler.Value.Handler is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }

    private IAsyncPolicy<HttpResponseMessage> CreateDefaultPolicy()
        => new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddRateLimiter(new HttpRateLimiterStrategyOptions() { Name = "Standard-RateLimiter" })
            .AddTimeout(new HttpTimeoutStrategyOptions() { Name = "Standard-TotalRequestTimeout" })
            .AddRetry(new HttpRetryStrategyOptions() { Name = "Standard-Retry" })
            .AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions() { Name = "Standard-CircuitBreaker" })
            .AddTimeout(new HttpTimeoutStrategyOptions
            {
                Timeout = TimeSpan.FromSeconds(10.0),
                Name = "Standard-AttemptTimeout"
            }).Build().AsAsyncPolicy();

    #region IDisposable Implementation
    private void Dispose(bool disposing)
    {
        if (!disposedValue && disposing)
        {
            foreach (var handlerEntry in _handlerPool.Values)
            {
                if (handlerEntry.IsValueCreated && handlerEntry.Value is IDisposable disposable)
                {
                    //handlerEntry.Value.DecrementUsage();
                    if (handlerEntry.Value.CanDispose())
                    {
                        disposable.Dispose();
                    }
                }
            }
            disposedValue = true;
        }
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        foreach (var handlerEntry in _handlerPool.Values)
        {
            if (handlerEntry.IsValueCreated)
            {
                //handlerEntry.Value.DecrementUsage();
                if (handlerEntry.Value.CanDispose())
                {
                    if (handlerEntry.Value.Handler is IAsyncDisposable asyncDisposable)
                    {
                        await asyncDisposable.DisposeAsync().ConfigureAwait(false);
                    }
                    else if (handlerEntry.Value.Handler is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                }
            }
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore().ConfigureAwait(false);
        Dispose(disposing: false);
        GC.SuppressFinalize(this);
    }
    #endregion
}
#nullable restore