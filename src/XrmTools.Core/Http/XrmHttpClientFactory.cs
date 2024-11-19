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
using Microsoft.VisualStudio.Threading;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.RateLimiting;
using Polly.RateLimiting;
using Microsoft.Extensions.Http.Resilience;
using Polly.Retry;

internal class XrmHttpClientFactory : IXrmHttpClientFactory, System.IAsyncDisposable, IDisposable
{
    private readonly ConcurrentDictionary<DataverseEnvironment, HttpClientConfig> _clientConfigs = new();
    private readonly ConcurrentDictionary<DataverseEnvironment, HttpClientEntry> _clients = new();
    private readonly AsyncTimer timer;
    private readonly object _lock = new();
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly TimeProvider timeProvider;
    private readonly IEnvironmentProvider environmentProvider;
    private readonly IAuthenticationService authenticationService;
    private readonly ILogger<XrmHttpClientFactory> logger;
    private bool disposedValue;

    [ImportingConstructor]
    public XrmHttpClientFactory(
        TimeProvider timeProvider, 
        IEnvironmentProvider environmentProvider,
        IAuthenticationService authenticationService,
        ILogger<XrmHttpClientFactory> logger)
    {
        this.timeProvider = timeProvider;
        timer = new AsyncTimer(async _ => await RecycleHandlersAsync(), TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1), timeProvider);
        this.environmentProvider = environmentProvider;
        this.authenticationService = authenticationService;
        this.logger = logger;
    }

    public async Task<HttpClient> CreateHttpClientAsync()
    {
        var environment = await environmentProvider.GetActiveEnvironmentAsync();
        return environment == null ? throw new InvalidOperationException("No environment selected.") : await CreateHttpClientAsync(environment);
    }

    public async Task<HttpClient> CreateHttpClientAsync(DataverseEnvironment environment)
    {
        if (string.IsNullOrEmpty(environment.ConnectionString))
        {
            throw new InvalidOperationException($"Environment '{environment.Name}' connection string is empty.");
        }
        if (!_clientConfigs.TryGetValue(environment, out var config))
        {
            config = CreateDefaultConfig(environment);
        }
        var entry = _clients.GetOrAdd(environment, _ => new HttpClientEntry(new AsyncLazy<HttpClient>(async () => await CreateHttpClientAsync(config), null), timeProvider.GetUtcNow()));
        return await entry.Client.GetValueAsync();
    }

    public void RegisterClient(DataverseEnvironment environment, Func<HttpClient, Task> configureClient, TimeSpan? handlerLifetime = null, IAsyncPolicy<HttpResponseMessage>? resiliencePolicy = null, params DelegatingHandler[] customHandlers)
        => _clientConfigs[environment] = new HttpClientConfig
        {
            ConfigureClient = configureClient,
            HandlerLifetime = handlerLifetime ?? TimeSpan.FromMinutes(2),
            ResiliencePolicy = resiliencePolicy == null ? null : Policy.WrapAsync(resiliencePolicy),
            CustomHandlers = customHandlers
        };

    private async Task<HttpClient> CreateHttpClientAsync(HttpClientConfig config)
    {
        var handler = new HttpClientHandler();

        HttpMessageHandler pipeline = handler;
        if (config.CustomHandlers != null && config.CustomHandlers.Length > 0)
        {
            for (int i = config.CustomHandlers.Length - 1; i >= 0; i--)
            {
                config.CustomHandlers[i].InnerHandler = pipeline;
                pipeline = config.CustomHandlers[i];
            }
        }

        var client = new HttpClient(new PolicyHttpMessageHandler(config.ResiliencePolicy) { InnerHandler = pipeline }, disposeHandler: true);
        if (config.ConfigureClient != null)
        {
            await config.ConfigureClient(client);
        }

        return client;
    }


    internal async Task RecycleHandlersAsync()
    {
        try
        {
            foreach (var kvp in _clients)
            {
                var name = kvp.Key;
                var entry = kvp.Value;
                if (_clientConfigs.TryGetValue(name, out var config) && config != null && entry.Client.IsValueCreated)
                {
                    var handlerLifetimeExceeded = timeProvider.GetUtcNow() - entry.CreatedAt >= config.HandlerLifetime;
                    if (handlerLifetimeExceeded)
                    {
                        await _semaphore.WaitAsync();
                        try
                        {
                            if (_clients.TryUpdate(name, new HttpClientEntry(new AsyncLazy<HttpClient>(async () => await CreateHttpClientAsync(config), null), timeProvider.GetUtcNow()), entry))
                            {
                                await entry.Client.DisposeValueAsync();
                            }
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

    private HttpClientConfig CreateDefaultConfig(DataverseEnvironment environment)
    => _clientConfigs[environment] = new HttpClientConfig
    {
        HandlerLifetime = TimeSpan.FromMinutes(5),
        ConfigureClient = async client =>
        {
            client.Timeout = TimeSpan.FromSeconds(100);
            var authParams = AuthenticationParameters.Parse(environment.ConnectionString);
            var authResult = await authenticationService.AuthenticateAsync(authParams, msg => logger.LogInformation(msg), CancellationToken.None);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);
        },
        ResiliencePolicy = CreateDefaultPolicy()
    };

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
            timer?.Dispose();

            foreach (var client in _clients.Values)
            {
                if (client.Client.IsValueCreated && client.Client is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
            disposedValue = true;
        }
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        foreach (var client in _clients.Values)
        {
            if (client.Client.IsValueCreated)
            {
                await client.Client.DisposeValueAsync().ConfigureAwait(false);
            }
        }

        if (timer is System.IAsyncDisposable disposable)
        {
            await disposable.DisposeAsync().ConfigureAwait(false);
        }
        else
        {
            timer?.Dispose();
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
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