#nullable enable
namespace XrmTools.Http;
using Microsoft.Identity.Client;
using Microsoft.VisualStudio.Threading;
using Polly;
//using Polly.RateLimiting;
using Polly.Timeout;
using System;
using System.Collections.Concurrent;
using System.ComponentModel.Composition;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.Authentication;
using XrmTools.Environments;
using XrmTools.Logging.Compatibility;
using XrmTools.Options;

[Export(typeof(IXrmHttpClientFactory))]
internal class XrmHttpClientFactory : IXrmHttpClientFactory, System.IAsyncDisposable, IDisposable
{
    private static readonly TimeSpan TokenExpirySkew = TimeSpan.FromMinutes(2);

    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly AsyncTimer timer;
    private readonly ConcurrentDictionary<DataverseEnvironment, Lazy<HttpMessageHandlerEntry>> _handlerPool = new();
    private readonly ConcurrentDictionary<string, AuthenticationResult> _tokenCache = new();
    // Ensures only one authentication flow runs per environment/connection string at a time
    private readonly ConcurrentDictionary<string, AsyncLazy<AuthenticationResult>> _inflightAuth = new();
    private bool disposedValue;

    TimeProvider TimeProvider { get; set; } = TimeProvider.System;

    [Import]
    IEnvironmentProvider EnvironmentProvider { get; set; } = null!;
    [Import]
    IAuthenticationService AuthenticationService { get; set; } = null!;
    [Import]
    ILogger<XrmHttpClientFactory> Logger { get; set; } = null!;

    public XrmHttpClientFactory()
    {
        timer = new AsyncTimer(async _ => await RecycleHandlersAsync(), TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1), TimeProvider);
    }

    // Explicit interface methods without CancellationToken for compatibility
    public Task<XrmHttpClient> CreateClientAsync() => CreateClientAsync(cancellationToken: default);
    public Task<XrmHttpClient> CreateClientAsync(DataverseEnvironment environment) => CreateClientAsync(environment, cancellationToken: default);

    public async Task<XrmHttpClient> CreateClientAsync(CancellationToken cancellationToken = default)
    {
        var environment = await EnvironmentProvider.GetActiveEnvironmentAsync(true);
        return environment == null || environment == DataverseEnvironment.Empty ? throw new InvalidOperationException("No environment selected.") : await CreateClientAsync(environment, cancellationToken);
    }

    public async Task<AuthenticationResult?> PreAuthenticateAsync(DataverseEnvironment environment, bool allowInteraction, CancellationToken cancellationToken = default)
    {
        if (environment == null) throw new ArgumentNullException(nameof(environment));

        if (environment != DataverseEnvironment.Empty && string.IsNullOrEmpty(environment.ConnectionString))
        {
            throw new InvalidOperationException($"Environment '{environment.Name}' connection string is empty.");
        }

        // Authenticate with a timeout to avoid waiting indefinitely, and deduplicate concurrent requests per environment
        AuthenticationResult? authResult = null;
        if (environment != DataverseEnvironment.Empty && (!_tokenCache.TryGetValue(environment.ConnectionString!, out authResult) || authResult.ExpiresOn <= TimeProvider.GetUtcNow().Add(TokenExpirySkew)))
        {
            var key = environment.ConnectionString!;
            var lazy = _inflightAuth.GetOrAdd(key, _ => new AsyncLazy<AuthenticationResult>(async () =>
            {
                var authTimeout = TimeSpan.FromSeconds(60);
                using var timeoutCts = new CancellationTokenSource(authTimeout);

                try
                {
                    // Use only the timeout CTS for the shared single-flight to avoid one caller cancelling others
                    var result = await AuthenticationService.AuthenticateAsync(environment, allowInteraction, msg => Logger.LogInformation(msg), timeoutCts.Token).ConfigureAwait(false);
                    _tokenCache[key] = result;
                    return result;
                }
                catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
                {
                    throw new TimeoutException("Authentication timed out.");
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Authentication failed.");
                    throw;
                }
            }, null));

            try
            {
                // Allow the current caller to cancel waiting without cancelling the shared authentication operation.
                var sharedAuthTask = lazy.GetValueAsync();
                if (cancellationToken.CanBeCanceled)
                {
                    var cancelTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                    using (cancellationToken.Register(() => cancelTcs.TrySetResult(true)))
                    {
                        var completed = await Task.WhenAny(sharedAuthTask, cancelTcs.Task).ConfigureAwait(false);
                        if (completed != sharedAuthTask)
                        {
                            throw new OperationCanceledException(cancellationToken);
                        }
                    }
                }
                authResult = await sharedAuthTask.ConfigureAwait(false);
            }
            finally
            {
                // Ensure future calls can trigger a new auth when needed (e.g., after expiry or on failure)
                _inflightAuth.TryRemove(key, out _);
            }
        }

        return authResult;
    }

    public async Task<XrmHttpClient> CreateClientAsync(DataverseEnvironment environment, CancellationToken cancellationToken = default)
    {
        if (environment == null) throw new ArgumentNullException(nameof(environment));

        if (environment != DataverseEnvironment.Empty && string.IsNullOrEmpty(environment.ConnectionString))
        {
            throw new InvalidOperationException($"Environment '{environment.Name}' connection string is empty.");
        }

        AuthenticationResult? authResult = await PreAuthenticateAsync(environment, true, cancellationToken).ConfigureAwait(false);

        var handlerEntry = _handlerPool.GetOrAdd(environment, _ => new Lazy<HttpMessageHandlerEntry>(() => CreateHandlerEntry(environment))).Value;

        handlerEntry.IncrementUsage();
        var client = new XrmHttpClient(handlerEntry.Handler, handlerEntry.DecrementUsage);
        ConfigureClient(client, environment, authResult?.AccessToken);

        return client;
    }

    private void ConfigureClient(XrmHttpClient client, DataverseEnvironment environment, string? accessToken)
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
            if (!string.IsNullOrEmpty(accessToken)) client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }
    }

    private HttpMessageHandlerEntry CreateHandlerEntry(DataverseEnvironment environment)
    {
        //var handler = new PolicyHttpMessageHandler(CreateDefaultPolicy());
        var handler = environment == DataverseEnvironment.Empty ?
            new PolicyHandler(new HttpClientHandler() { AllowAutoRedirect = false }, CreateDefaultPolicy()) :
            new PolicyHandler(CreateHandler(environment), CreateDefaultPolicy());
        return new (handler, TimeProvider.GetUtcNow());
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
            Logger.LogError(ex, $"Exception during handler recycling: {ex.Message}");
        }
    }

    private IAsyncPolicy<HttpResponseMessage> CreateDefaultPolicy()
        => new ResiliencePipelineBuilder<HttpResponseMessage>()
            //.AddRateLimiter(new HttpRateLimiterStrategyOptions() { Name = "Standard-RateLimiter" }) or:
            //.AddRateLimiter(new RateLimiterStrategyOptions() { Name = "Standard-RateLimiter" })
            .AddTimeout(new TimeoutStrategyOptions() { Name = "Standard-TotalRequestTimeout" })
            .AddRetry(new Resiliency.HttpRetryStrategyOptions() { Name = "Standard-Retry" })
            .AddCircuitBreaker(new Resiliency.HttpCircuitBreakerStrategyOptions() { Name = "Standard-CircuitBreaker" })
            .AddTimeout(new TimeoutStrategyOptions
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