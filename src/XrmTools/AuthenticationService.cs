﻿namespace XrmTools.Authentication;
using Microsoft.Identity.Client;
using System;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.Http;
using XrmTools.Tokens;
using System.Net.Http;
using System.ComponentModel.Composition;

[Export(typeof(IAuthenticationService))]
internal class AuthenticationService : IAuthenticationService
{
    [Import]
    ITokenExpanderService TokenExpander { get; set; }

    [Import]
    Lazy<IXrmHttpClientFactory> HttpClientFactory { get; set; }

    public IAuthenticator Authenticator { get; set; } = new ClientAppAuthenticator
    {
        NextAuthenticator = new DeviceCodeAuthenticator
        {
            NextAuthenticator = new IntegratedAuthenticator()
        }
    };

    public async Task<AuthenticationResult> AuthenticateAsync(
        DataverseEnvironment environment,
        Action<string> onMessageForUser = default,
        CancellationToken cancellationToken = default)
    {
        if (environment == null) { throw new ArgumentNullException(nameof(environment)); }
        if (!environment.IsValid) 
        { 
            throw new InvalidOperationException("Authentication failed. The current environment is not valid. Please make sure the environment configuration is correct in Tools > Options > Xrm Tools."); 
        }
        var current = Authenticator;
        var connectionString = TokenExpander.ExpandTokens(environment.ConnectionString);
        var authParams = await EnsureTenantAsync(AuthenticationParameters.Parse(connectionString));
        while (current != null && !current.CanAuthenticate(authParams))
        {
            current = current.NextAuthenticator;
        }
        if (current == null)
        {
            throw new InvalidOperationException("Unable to detect required authentication flow. Please check the input parameters and try again.");
        }
        return await current?.AuthenticateAsync(authParams, onMessageForUser, cancellationToken);
    }

    private async Task<AuthenticationParameters> EnsureTenantAsync(AuthenticationParameters authParams)
    {
        if (string.IsNullOrEmpty(authParams.Tenant))
        {
            var url = authParams.Resource;
            using var httpClient = await HttpClientFactory.Value.CreateClientAsync(DataverseEnvironment.Empty);
            var response = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, url)).ConfigureAwait(false);
            var authUrl = response.Headers.Location;
            var tenantId = authUrl.AbsolutePath[1..authUrl.AbsolutePath.IndexOf('/', 1)];
            authParams.Tenant = tenantId;
        }
        return authParams;
    }
}