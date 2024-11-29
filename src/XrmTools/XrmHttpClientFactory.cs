﻿#nullable enable
namespace XrmTools.Http;
using System;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading;
using Polly;
using XrmTools.Environments;
using XrmTools.Logging.Compatibility;
using System.ComponentModel.Composition;
using XrmTools.Authentication;
using System.Net.Http.Headers;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Identity.Client;
using Microsoft.VisualStudio.Threading;

[Export(typeof(IXrmHttpClientFactory))]
internal class XrmHttpClientFactory : IXrmHttpClientFactory, System.IAsyncDisposable, IDisposable
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly AsyncTimer timer;
    private readonly ConcurrentDictionary<DataverseEnvironment, Lazy<HttpMessageHandlerEntry>> _handlerPool = new();
    private readonly ConcurrentDictionary<string, AuthenticationResult> _tokenCache = new();
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
        timer = new AsyncTimer(async _ => await RecycleHandlersAsync(), TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1), timeProvider);
        this.timeProvider = timeProvider;
        this.environmentProvider = environmentProvider;
        this.authenticationService = authenticationService;
        this.logger = logger;
    }

    public async Task<XrmHttpClient> CreateClientAsync()
    {
        var environment = await environmentProvider.GetActiveEnvironmentAsync();
        return environment == null ? throw new InvalidOperationException("No environment selected.") : await CreateClientAsync(environment);
    }

    public async Task<XrmHttpClient> CreateClientAsync(DataverseEnvironment environment)
    {
        if (string.IsNullOrEmpty(environment.ConnectionString))
        {
            throw new InvalidOperationException($"Environment '{environment.Name}' connection string is empty.");
        }
        var handlerEntry = _handlerPool.GetOrAdd(environment, _ => new Lazy<HttpMessageHandlerEntry>(() => CreateHandlerEntry())).Value;

        handlerEntry.IncrementUsage();
        var client = new XrmHttpClient(handlerEntry.Handler, handlerEntry.DecrementUsage);
        await ConfigureClientAsync(client, environment);

        return client;
    }

    private async Task ConfigureClientAsync(XrmHttpClient client, DataverseEnvironment environment)
    {
        client.Timeout = TimeSpan.FromMinutes(10);
        client.BaseAddress = new Uri(new Uri(environment.Url), "/api/data/v9.2/");
        if (!_tokenCache.TryGetValue(environment.ConnectionString!, out var authResult) || authResult.ExpiresOn <= timeProvider.GetUtcNow())
        {
            authResult = await authenticationService.AuthenticateAsync(environment, msg => logger.LogInformation(msg), CancellationToken.None);
            _tokenCache[environment.ConnectionString!] = authResult;
        }
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);
    }

    private HttpMessageHandlerEntry CreateHandlerEntry()
    {
        //var handler = new PolicyHttpMessageHandler(CreateDefaultPolicy());
        var handler = new PolicyHandler(new HttpClientHandler(), CreateDefaultPolicy());
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
                if (entry.IsValueCreated && entry.Value.CanDispose())
                {
                    await _semaphore.WaitAsync();
                    try
                    {
                        if (_handlerPool.TryUpdate(name, new Lazy<HttpMessageHandlerEntry>(CreateHandlerEntry), entry))
                        {
                            entry.Value.Handler.Dispose();
                        }
                    }
                    finally
                    {
                        _semaphore.Release();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Exception during handler recycling: {ex.Message}");
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
            timer.Dispose();
            foreach (var handlerEntry in _handlerPool.Values)
            {
                if (handlerEntry.IsValueCreated && handlerEntry.Value is IDisposable disposable)
                {
                    //handlerEntry.Value.DecrementUsage();
                    disposable.Dispose();
                }
            }
            _handlerPool.Clear();
            disposedValue = true;
        }
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        timer.Dispose();
        foreach (var handlerEntry in _handlerPool.Values)
        {
            if (handlerEntry.IsValueCreated)
            {
                if (handlerEntry.Value.Handler is System.IAsyncDisposable asyncDisposable)
                {
                    await asyncDisposable.DisposeAsync().ConfigureAwait(false);
                }
                else if (handlerEntry.Value.Handler is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }
        _handlerPool.Clear();
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