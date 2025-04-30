#nullable enable
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
using System.Reflection;
using System.Net;
using XrmTools.Options;

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
        return environment == null || environment == DataverseEnvironment.Empty ? throw new InvalidOperationException("No environment selected.") : await CreateClientAsync(environment);
    }

    public async Task<XrmHttpClient> CreateClientAsync(DataverseEnvironment environment)
    {
        if (environment == null) throw new ArgumentNullException(nameof(environment));

        if (environment != DataverseEnvironment.Empty && string.IsNullOrEmpty(environment.ConnectionString))
        {
            throw new InvalidOperationException($"Environment '{environment.Name}' connection string is empty.");
        }
        var handlerEntry = _handlerPool.GetOrAdd(environment, _ => new Lazy<HttpMessageHandlerEntry>(() => CreateHandlerEntry(environment))).Value;

        handlerEntry.IncrementUsage();
        var client = new XrmHttpClient(handlerEntry.Handler, handlerEntry.DecrementUsage);
        await ConfigureClientAsync(client, environment);

        return client;
    }

    private async Task ConfigureClientAsync(XrmHttpClient client, DataverseEnvironment environment)
    {
        client.Timeout = TimeSpan.FromMinutes(60);
        if (environment != DataverseEnvironment.Empty)
        {
            client.BaseAddress = environment.BaseServiceUrl!;
            client.DefaultRequestHeaders.Add("Prefer", "odata.include-annotations=*");
            var assemblyName = Assembly.GetExecutingAssembly().GetName();
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(assemblyName.Name.Replace(" ", ""), assemblyName.Version.ToString()));
            client.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
            client.DefaultRequestHeaders.Add("OData-Version", "4.0");
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            if (!_tokenCache.TryGetValue(environment.ConnectionString!, out var authResult) || authResult.ExpiresOn <= timeProvider.GetUtcNow())
            {
                authResult = await authenticationService.AuthenticateAsync(environment, msg => logger.LogInformation(msg), CancellationToken.None);
                _tokenCache[environment.ConnectionString!] = authResult;
            }
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);
        }
    }

    private HttpMessageHandlerEntry CreateHandlerEntry(DataverseEnvironment environment)
    {
        //var handler = new PolicyHttpMessageHandler(CreateDefaultPolicy());
        var handler = environment == DataverseEnvironment.Empty ?
            new PolicyHandler(new HttpClientHandler() { AllowAutoRedirect = false }, CreateDefaultPolicy()) :
            new PolicyHandler(CreateHandler(environment), CreateDefaultPolicy());
        return new (handler, timeProvider.GetUtcNow());
    }

    private HttpClientHandler CreateHandler(DataverseEnvironment environment)
    {
        var proxyAddress = GeneralOptions.Instance.Proxy;
        var useProxy = !string.IsNullOrWhiteSpace(proxyAddress);
        var handler = new HttpClientHandler()
        {
            AllowAutoRedirect = false,
            UseCookies = environment.AllowCookies,
        };
        if (useProxy)
        {
            handler.Proxy = new WebProxy(proxyAddress);
            handler.UseProxy = true;
        }
        else
        {
            handler.UseProxy = false;
        }
        return handler;
    }

    private async Task RecycleHandlersAsync()
    {
        try
        {
            foreach (var kvp in _handlerPool)
            {
                var environment = kvp.Key;
                var entry = kvp.Value;
                if (entry.IsValueCreated && entry.Value.CanDispose())
                {
                    await _semaphore.WaitAsync();
                    try
                    {
                        if (_handlerPool.TryUpdate(environment, new Lazy<HttpMessageHandlerEntry>(() => CreateHandlerEntry(environment)), entry))
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
                Timeout = TimeSpan.FromSeconds(60.0),
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