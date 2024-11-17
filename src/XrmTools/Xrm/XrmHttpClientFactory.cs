﻿#nullable enable
namespace XrmTools.Xrm;
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
using XrmTools.Xrm.Auth;
using System.Net.Http.Headers;

internal class XrmHttpClientFactory : IXrmHttpClientFactory, IDisposable
{
    private readonly ConcurrentDictionary<string, HttpClientConfig> _clientConfigs = new();
    private readonly ConcurrentDictionary<string, HttpClientEntry> _clients = new();
    private readonly ITimer timer;
    private readonly object _lock = new();
    private readonly TimeProvider timeProvider;
    private readonly IEnvironmentProvider environmentProvider;
    private readonly IAuthenticationService authenticationService;
    private readonly ILogger<XrmHttpClientFactory> logger;

    [ImportingConstructor]
    public XrmHttpClientFactory(
        TimeProvider timeProvider, 
        IEnvironmentProvider environmentProvider,
        IAuthenticationService authenticationService,
        ILogger<XrmHttpClientFactory> logger)
    {
        this.timeProvider = timeProvider;
        timer = timeProvider.CreateTimer(_ => RecycleHandlers(), null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
        this.environmentProvider = environmentProvider;
        this.authenticationService = authenticationService;
        this.logger = logger;
    }

    public async Task<HttpClient> CreateHttpClientAsync()
    {
        var environment = await environmentProvider.GetActiveEnvironmentAsync();
        return environment == null ? throw new InvalidOperationException("No environment selected.") : await CreateHttpClientAsync(environment);
    }

    private async Task<HttpClient> CreateHttpClientAsync(DataverseEnvironment environment)
    {
        var name = environment.ConnectionString;
        if (string.IsNullOrEmpty(name))
        {
            throw new InvalidOperationException($"Environment '{environment.Name}' connection string is empty.");
        }
        if (!_clientConfigs.TryGetValue(name!, out var config))
        {
            config = CreateDefaultConfig(name!);
        }
        var authParams = AuthenticationParameters.Parse(environment.ConnectionString);
        var authResult = await authenticationService.AuthenticateAsync(authParams, msg => logger.LogInformation(msg), CancellationToken.None);
        var client = _clients.GetOrAdd(name!, new HttpClientEntry(new Lazy<HttpClient>(() => CreateHttpClient(config)), timeProvider.GetUtcNow())).Client.Value;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);
        return client;
    }

    private HttpClient CreateHttpClient(HttpClientConfig config)
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
        config.ConfigureClient?.Invoke(client);

        return client;
    }

    private void RecycleHandlers()
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
                        lock (_lock)
                        {
                            if (_clients.TryUpdate(name, new HttpClientEntry(new Lazy<HttpClient>(() => CreateHttpClient(config)), timeProvider.GetUtcNow()), entry))
                            {
                                entry.Client.Value.Dispose();
                            }
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

    private HttpClientConfig CreateDefaultConfig(string name)
        => _clientConfigs[name] = new HttpClientConfig
        {
            HandlerLifetime = TimeSpan.FromMinutes(5),
            ConfigureClient = client =>
            {
                client.Timeout = TimeSpan.FromSeconds(100);
            }
        };

    public void RegisterClient(string name, Action<HttpClient> configureClient, TimeSpan? handlerLifetime = null, IAsyncPolicy<HttpResponseMessage>? resiliencePolicy = null, params DelegatingHandler[] customHandlers)
        => _clientConfigs[name] = new HttpClientConfig
        {
            ConfigureClient = configureClient,
            HandlerLifetime = handlerLifetime ?? TimeSpan.FromMinutes(2),
            ResiliencePolicy = resiliencePolicy == null ? null : Policy.WrapAsync(resiliencePolicy),
            CustomHandlers = customHandlers
        };

    public void RegisterClient<T>(Action<HttpClient> configureClient, TimeSpan? handlerLifetime = null, IAsyncPolicy<HttpResponseMessage>? resiliencePolicy = null, params DelegatingHandler[] customHandlers) where T : class
    {
        string typeName = typeof(T).Name;
        RegisterClient(typeName, configureClient, handlerLifetime, resiliencePolicy, customHandlers);
    }

    public void Dispose()
    {
        timer.Dispose();
        foreach (var client in _clients.Values)
        {
            if (client.Client.IsValueCreated)
            {
                client.Client.Value.Dispose();
            }
        }
    }
}
#nullable restore
