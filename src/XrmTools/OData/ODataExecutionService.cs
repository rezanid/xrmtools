#nullable enable
namespace XrmTools.OData;

using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.Environments;
using XrmTools.Http;
using XrmTools.Options;

internal sealed class ODataResponse
{
    public string Status { get; set; } = "";
    public string Headers { get; set; } = "";
    public string Body { get; set; } = "";
    public string Request { get; set; } = "";
    public string Raw { get; set; } = "";
    public int StatusCode { get; set; }
    public double ElapsedMilliseconds { get; set; }
    public long ContentByteLength { get; set; }
}

internal interface IODataTransport
{
    Task<ODataResponse> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken);
}

[Export(typeof(IODataTransport))]
internal sealed class ODataTransport : IODataTransport
{
    public async Task<ODataResponse> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var options = await GeneralOptions.GetLiveInstanceAsync();
        using var handler = new HttpClientHandler { AllowAutoRedirect = false, UseCookies = false, UseDefaultCredentials = false };
        handler.UseProxy = !string.IsNullOrWhiteSpace(options.Proxy);
        if (handler.UseProxy) handler.Proxy = new WebProxy(options.Proxy);
        using var client = new HttpClient(handler) { Timeout = TimeSpan.FromMinutes(2), MaxResponseContentBufferSize = 4 * 1024 * 1024 };
        var watch = Stopwatch.StartNew();
        // One send only: never retry mutations or forward bearer credentials across redirects.
        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken).ConfigureAwait(false);
        watch.Stop();
        var bytes = response.Content == null ? Array.Empty<byte>() : await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
        string body = response.Content == null ? "" : await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        return new ODataResponse
        {
            Status = $"{(int)response.StatusCode} {response.ReasonPhrase}",
            StatusCode = (int)response.StatusCode,
            ElapsedMilliseconds = watch.Elapsed.TotalMilliseconds,
            ContentByteLength = bytes.LongLength,
            Headers = ODataResponseFormatter.Headers(response.Headers) + ODataResponseFormatter.Headers(response.Content?.Headers),
            Body = body,
            Request = $"{request.Method} {request.RequestUri}",
            Raw = await ODataResponseFormatter.RawAsync(request, response, body).ConfigureAwait(false),
        };
    }
}

[Export]
internal sealed class ODataExecutionService
{
    private readonly IEnvironmentSelection environments;
    private readonly IXrmHttpClientFactory authentication;
    private readonly IODataTransport transport;

    [ImportingConstructor]
    public ODataExecutionService(IEnvironmentSelection environments, IXrmHttpClientFactory authentication, IODataTransport transport)
    {
        this.environments = environments;
        this.authentication = authentication;
        this.transport = transport;
    }

    public Task<DataverseEnvironment?> GetEnvironmentAsync() => environments.GetSelectedEnvironmentAsync();

    public async Task<ODataResponse> SendAsync(ODataDocument document, ODataRequest request, DataverseEnvironment expected, CancellationToken cancellationToken)
    {
        var resolved = document.Resolve(request);
        var captured = expected with { };
        await VerifyEnvironmentAsync(captured, cancellationToken);
        // Validate the destination and syntax before invoking authentication.
        using var validation = ODataRequestBuilder.Build(resolved, captured.BaseServiceUrl!, "validation");
        var token = await authentication.PreAuthenticateAsync(captured, true, cancellationToken);
        if (token == null || string.IsNullOrEmpty(token.AccessToken) || token.ExpiresOn <= DateTimeOffset.UtcNow)
            throw new InvalidOperationException("No valid access token was acquired. Sign in and try again.");
        await VerifyEnvironmentAsync(captured, cancellationToken);
        using var message = ODataRequestBuilder.Build(resolved, captured.BaseServiceUrl!, token.AccessToken);
        var response = await transport.SendAsync(message, cancellationToken);
        return response;
    }

    private async Task VerifyEnvironmentAsync(DataverseEnvironment expected, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var current = await environments.GetSelectedEnvironmentAsync();
        if (current?.IsValid != true || current.ConnectionString != expected.ConnectionString)
            throw new InvalidOperationException("The environment changed. Review the selected environment and send again.");
        cancellationToken.ThrowIfCancellationRequested();
    }
}
