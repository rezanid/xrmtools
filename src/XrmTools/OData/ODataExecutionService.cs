#nullable enable
namespace XrmTools.OData;

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Microsoft.Identity.Client;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.Environments;
using XrmTools.Http;
using XrmTools.Options;

internal sealed class ODataResponse : IDisposable
{
    public string Status { get; set; } = "";
    public string Headers { get; set; } = "";
    public string Body { get; set; } = "";
    public bool IsJson { get; set; }
    public ODataStoredBody? StoredBody { get; set; }
    public void Dispose()
    {
        StoredBody?.Release();
        StoredBody = null;
    }
    public IReadOnlyDictionary<string, string> CapturableHeaders { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    public string? Principal { get; set; }
    public bool CanCapture { get; set; }
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
    internal const int PreviewLimit = 256 * 1024;
    internal const long DownloadLimit = 100L * 1024 * 1024;
    public async Task<ODataResponse> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var options = await GeneralOptions.GetLiveInstanceAsync();
        using var handler = new HttpClientHandler { AllowAutoRedirect = false, UseCookies = false, UseDefaultCredentials = false,
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate };
        handler.UseProxy = !string.IsNullOrWhiteSpace(options.Proxy);
        if (handler.UseProxy) handler.Proxy = new WebProxy(options.Proxy);
        using var client = new HttpClient(handler);
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromMinutes(2));
        return await SendCoreAsync(client, request, timeout.Token).ConfigureAwait(false);
    }

    internal static async Task<ODataResponse> SendCoreAsync(HttpClient client, HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // .NET Framework HttpClient disposes request content after sending.
        // Capture the redacted display before dispatch, so formatting cannot reread disposed content.
        var capturedRequest = await ODataResponseFormatter.RequestAsync(request).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        var watch = Stopwatch.StartNew();
        // One send only: never retry mutations or forward bearer credentials across redirects.
        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var header in response.Headers.Concat(response.Content?.Headers ?? Enumerable.Empty<KeyValuePair<string, IEnumerable<string>>>()))
            if (!ODataCapturedResponse.IsCredentialHeader(header.Key)) headers[header.Key] = string.Join(", ", header.Value);
        var result = new ODataResponse
        {
            CapturableHeaders = headers,
            Status = $"{(int)response.StatusCode} {response.ReasonPhrase}",
            StatusCode = (int)response.StatusCode,
            Headers = ODataResponseFormatter.Headers(response.Headers) + ODataResponseFormatter.Headers(response.Content?.Headers),
            Request = $"{request.Method} {request.RequestUri}",
        };
        try
        {
            if (response.Content != null)
                await ReadBodyAsync(response.Content, result, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            result.ElapsedMilliseconds = watch.Elapsed.TotalMilliseconds;
            string raw = ODataResponseFormatter.Raw(capturedRequest, response, result.Body);
            result.Raw = raw.Length <= PreviewLimit ? raw : raw.Substring(0, PreviewLimit) + "\r\n[Preview truncated]";
            if (result.StoredBody != null && raw.Length <= PreviewLimit) result.Raw += "\r\n[Response body preview truncated]";
            return result;
        }
        catch { result.Dispose(); throw; }
    }

    private static async Task ReadBodyAsync(HttpContent content, ODataResponse result, CancellationToken token)
    {
        if (content.Headers.ContentEncoding.Any(value => !value.Equals("identity", StringComparison.OrdinalIgnoreCase)))
            throw new FormatException("The response uses an unsupported content encoding.");
        var media = content.Headers.ContentType?.MediaType;
        result.IsJson = media != null && (media.Equals("application/json", StringComparison.OrdinalIgnoreCase) ||
            media.EndsWith("+json", StringComparison.OrdinalIgnoreCase));
        var charset = content.Headers.ContentType?.CharSet?.Trim('"');
        var encoding = string.IsNullOrEmpty(charset) ? Encoding.UTF8 : Encoding.GetEncoding(charset);
        using var preview = new MemoryStream();
        using var input = await content.ReadAsStreamAsync().ConfigureAwait(false);
        using var abortRead = token.Register(() => input.Dispose());
        var buffer = new byte[64 * 1024];
        string? path = null;
        FileStream? file = null;
        try
        {
            while (true)
            {
                token.ThrowIfCancellationRequested();
                int count = await input.ReadAsync(buffer, 0, buffer.Length, token).ConfigureAwait(false);
                if (count == 0) break;
                result.ContentByteLength += count;
                if (result.ContentByteLength > DownloadLimit)
                    throw new InvalidOperationException("The decoded response exceeded the 100 MiB download limit.");
                int previewCount = (int)Math.Min(count, PreviewLimit - preview.Length);
                if (file == null && previewCount < count)
                {
                    string candidate = Path.Combine(Path.GetTempPath(), $"XrmTools-OData-{Guid.NewGuid():N}.tmp");
                    file = new FileStream(candidate, FileMode.CreateNew, FileAccess.Write, FileShare.None, buffer.Length, useAsync: true);
                    path = candidate;
                    preview.Position = 0;
                    await preview.CopyToAsync(file, buffer.Length, token).ConfigureAwait(false);
                }
                if (file != null) await file.WriteAsync(buffer, 0, count, token).ConfigureAwait(false);
                preview.Write(buffer, 0, previewCount);
            }
            file?.Dispose();
            file = null;
            preview.Position = 0;
            using var reader = new StreamReader(preview, encoding, detectEncodingFromByteOrderMarks: true);
            result.Body = reader.ReadToEnd();
            if (path != null)
            {
                result.StoredBody = new ODataStoredBody(path, encoding);
                path = null;
            }
        }
        catch (Exception) when (token.IsCancellationRequested) { throw new OperationCanceledException(token); }
        finally
        {
            file?.Dispose();
            if (path != null) File.Delete(path);
        }
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

    public async Task<ODataResponse> SendAsync(ODataDocument document, ODataRequest request, DataverseEnvironment expected, CancellationToken cancellationToken,
        ODataCaptureSession? session = null, ODataCaptureSession.Run run = default)
    {
        var used = new List<ODataCapturedResponse>();
        var resolved = await Task.Run(() => document.Resolve(request, name => session?.Get(name), response => used.Add(response), cancellationToken), cancellationToken);
        var captured = expected with { };
        await VerifyEnvironmentAsync(captured, cancellationToken);
        using var validation = ODataRequestBuilder.Build(resolved, captured.BaseServiceUrl!, "validation");
        var token = await authentication.PreAuthenticateAsync(captured, true, cancellationToken);
        if (token == null || string.IsNullOrEmpty(token.AccessToken) || token.ExpiresOn <= DateTimeOffset.UtcNow)
            throw new InvalidOperationException("No valid access token was acquired. Sign in and try again.");
        await VerifyEnvironmentAsync(captured, cancellationToken);
        var principal = Principal(token);
        if (used.Count > 0)
        {
            if (principal == null || used.Any(value => value.Principal == null || !string.Equals(value.Principal, principal, StringComparison.Ordinal)))
                throw new InvalidOperationException("Cannot verify that the authenticated identity matches the captured response. Sign in with the same account and manually send the named request again.");
            if (session == null || !session.AreCurrent(used, run.Epoch))
                throw new InvalidOperationException("A referenced response or environment changed. Manually send the named request again.");
        }
        cancellationToken.ThrowIfCancellationRequested();
        using var message = ODataRequestBuilder.Build(resolved, captured.BaseServiceUrl!, token.AccessToken);
        var response = await transport.SendAsync(message, cancellationToken);
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            response.Principal = principal;
            if (session != null && request.RequestName.Length != 0 && response.StatusCode >= 200 && response.StatusCode < 300)
            {
                var current = await environments.GetSelectedEnvironmentAsync();
                cancellationToken.ThrowIfCancellationRequested();
                response.CanCapture = SameEnvironment(current, captured);
                if (!response.CanCapture) session.SetEnvironment(current);
            }
            return response;
        }
        catch { response.Dispose(); throw; }
    }

    internal static string? Principal(AuthenticationResult result)
    {
        var account = result.Account?.HomeAccountId?.Identifier;
        if (!string.IsNullOrWhiteSpace(account) && !string.IsNullOrWhiteSpace(result.TenantId))
            return "user:" + result.TenantId.ToLowerInvariant() + ":" + account.ToLowerInvariant();
        if (result.Account != null) return null;
        // Client-credential results have no account; use the tenant, service-principal object ID and client ID from the MSAL-issued token.
        var parts = result.AccessToken.Split('.');
        if (parts.Length != 3) return null;
        try
        {
            var payload = parts[1].Replace('-', '+').Replace('_', '/');
            payload = payload.PadRight((payload.Length + 3) / 4 * 4, '=');
            using var json = JsonDocument.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(payload)));
            var root = json.RootElement;
            if (root.ValueKind != JsonValueKind.Object || root.TryGetProperty("scp", out _) ||
                (root.TryGetProperty("idtyp", out var idtyp) && (idtyp.ValueKind != JsonValueKind.String || idtyp.GetString() != "app")) ||
                !Claim(root, "tid", out var tenant) || !Claim(root, "oid", out var oid) ||
                !(Claim(root, "appid", out var app) || Claim(root, "azp", out app))) return null;
            if (!string.IsNullOrWhiteSpace(result.TenantId) && !string.Equals(result.TenantId, tenant, StringComparison.OrdinalIgnoreCase)) return null;
            return "app:" + tenant.ToLowerInvariant() + ":" + oid.ToLowerInvariant() + ":" + app.ToLowerInvariant();
        }
        catch (FormatException) { return null; }
        catch (JsonException) { return null; }
    }

    private static bool Claim(JsonElement root, string name, out string value)
    {
        value = "";
        if (!root.TryGetProperty(name, out var claim) || claim.ValueKind != JsonValueKind.String) return false;
        value = claim.GetString() ?? "";
        return !string.IsNullOrWhiteSpace(value);
    }

    private static bool SameEnvironment(DataverseEnvironment? current, DataverseEnvironment expected) =>
        current?.IsValid == true && current.ConnectionString == expected.ConnectionString && current.Name == expected.Name;

    private async Task VerifyEnvironmentAsync(DataverseEnvironment expected, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var current = await environments.GetSelectedEnvironmentAsync();
        if (!SameEnvironment(current, expected))
            throw new InvalidOperationException("The environment changed. Review the selected environment and send again.");
        cancellationToken.ThrowIfCancellationRequested();
    }
}
