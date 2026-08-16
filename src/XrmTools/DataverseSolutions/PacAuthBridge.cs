#nullable enable
namespace XrmTools.DataverseSolutions;

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.Authentication;
using XrmTools.Environments;
using XrmTools.Http;
using XrmTools.Logging.Compatibility;
using XrmTools.Tokens;

public interface IPacAuthBridge
{
    Task EnsurePacProfileForCurrentEnvironmentAsync(CancellationToken cancellationToken);
}

internal interface IPacAuthUserInteraction
{
    Task<bool> ConfirmProfileCreationAsync(string environmentUrl, bool browserSignInMayBeRequired);
}

[Export(typeof(IPacAuthBridge))]
[method: ImportingConstructor]
internal sealed class PacAuthBridge(
    IEnvironmentProvider environmentProvider,
    ITokenExpanderService tokenExpanderService,
    IXrmHttpClientFactory httpClientFactory,
    IPacCli pacCli,
    IPacAuthUserInteraction pacAuthUserInteraction,
    ILogger<PacAuthBridge> logger) : IPacAuthBridge
{
    private readonly IEnvironmentProvider _environmentProvider = environmentProvider;
    private readonly ITokenExpanderService _tokenExpanderService = tokenExpanderService;
    private readonly IXrmHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly IPacCli _pacCli = pacCli;
    private readonly IPacAuthUserInteraction _pacAuthUserInteraction = pacAuthUserInteraction;
    private readonly ILogger<PacAuthBridge> _logger = logger;

    public async Task EnsurePacProfileForCurrentEnvironmentAsync(CancellationToken cancellationToken)
    {
        var environment = await _environmentProvider.GetActiveEnvironmentAsync(true).ConfigureAwait(false)
            ?? throw new InvalidOperationException("No active environment selected. Select a Dataverse environment and try again.");

        if (string.IsNullOrWhiteSpace(environment.Url))
        {
            throw new InvalidOperationException("The current environment does not have a valid URL.");
        }

        var normalizedEnvironmentUrl = DataverseEnvironmentUrl.Normalize(environment.Url);
        var profiles = await _pacCli.ListAuthProfilesAsync(cancellationToken).ConfigureAwait(false);
        var matchingProfiles = FindMatchingProfiles(profiles, normalizedEnvironmentUrl);

        if (matchingProfiles.Count > 0)
        {
            if (matchingProfiles.Count > 1)
            {
                _logger.LogWarning($"Multiple PAC auth profiles match {normalizedEnvironmentUrl}. Selecting profile index {matchingProfiles[0].Index}.");
            }

            await _pacCli.SelectAuthProfileAsync(matchingProfiles[0], cancellationToken).ConfigureAwait(false);
            return;
        }

        var authParameters = await ResolveAuthenticationParametersAsync(environment, cancellationToken).ConfigureAwait(false);
        var createRequest = CreateProfileRequest(environment, authParameters);

        var confirmed = await _pacAuthUserInteraction
            .ConfirmProfileCreationAsync(environment.Url, browserSignInMayBeRequired: string.IsNullOrWhiteSpace(createRequest.ClientSecret))
            .ConfigureAwait(false);
        if (!confirmed)
        {
            throw new OperationCanceledException("PAC authentication profile creation was canceled by the user.", cancellationToken);
        }

        await _pacCli.CreateAuthProfileAsync(createRequest, cancellationToken).ConfigureAwait(false);

        profiles = await _pacCli.ListAuthProfilesAsync(cancellationToken).ConfigureAwait(false);
        matchingProfiles = FindMatchingProfiles(profiles, normalizedEnvironmentUrl);
        if (matchingProfiles.Count == 0)
        {
            throw new InvalidOperationException($"PAC did not create a usable auth profile for {normalizedEnvironmentUrl}.");
        }

        await _pacCli.SelectAuthProfileAsync(matchingProfiles[0], cancellationToken).ConfigureAwait(false);
    }

    internal static IReadOnlyList<PacAuthProfile> FindMatchingProfiles(IReadOnlyList<PacAuthProfile> profiles, string normalizedEnvironmentUrl)
    {
        if (profiles is null) throw new ArgumentNullException(nameof(profiles));
        if (normalizedEnvironmentUrl is null) throw new ArgumentNullException(nameof(normalizedEnvironmentUrl));

        return profiles
            .Where(profile => DataverseEnvironmentUrl.TryNormalize(profile.EnvironmentUrl, out var normalizedProfileUrl)
                && string.Equals(normalizedProfileUrl, normalizedEnvironmentUrl, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(profile => profile.IsActive)
            .ThenBy(profile => profile.Index)
            .ToArray();
    }

    private async Task<AuthenticationParameters> ResolveAuthenticationParametersAsync(
        DataverseEnvironment environment,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(environment.ConnectionString))
        {
            throw new InvalidOperationException($"The current environment '{environment.Name ?? environment.Url}' does not have a connection string.");
        }

        var expandedConnectionString = _tokenExpanderService.ExpandTokens(environment.ConnectionString);
        var authParameters = AuthenticationParameters.Parse(expandedConnectionString);
        return await AuthenticationParameterResolver.EnsureTenantAsync(authParameters, _httpClientFactory, cancellationToken).ConfigureAwait(false);
    }

    private static PacAuthCreateRequest CreateProfileRequest(DataverseEnvironment environment, AuthenticationParameters authParameters)
    {
        if (string.IsNullOrWhiteSpace(environment.Url))
        {
            throw new InvalidOperationException("The current environment does not have a valid URL.");
        }

        if (!string.IsNullOrWhiteSpace(authParameters.CertificateThumbprint))
        {
            throw new NotSupportedException("XrmTools cannot create a PAC auth profile automatically for certificate-thumbprint authentication. PAC requires a certificate file path and password, so create the PAC profile manually and try again.");
        }

        return new PacAuthCreateRequest
        {
            ProfileName = BuildProfileName(environment),
            EnvironmentUrl = environment.Url,
            ApplicationId = string.Equals(authParameters.ClientId, AuthenticationParameters.DefaultClientId, StringComparison.OrdinalIgnoreCase)
                ? null
                : authParameters.ClientId,
            ClientSecret = authParameters.ClientSecret,
            TenantId = authParameters.Tenant,
            UseDeviceCode = false
        };
    }

    private static string BuildProfileName(DataverseEnvironment environment)
    {
        var seed = !string.IsNullOrWhiteSpace(environment.Name)
            ? environment.Name!
            : new Uri(environment.Url!).Host;

        var normalized = $"XrmTools {seed}".Trim();
        return normalized.Length <= 30 ? normalized : normalized[..30];
    }
}
#nullable restore
