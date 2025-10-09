#nullable enable
namespace XrmTools.FetchXml.Margin;

using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using HorizontalAlignment = System.Windows.HorizontalAlignment;

public class Browser : IDisposable
{
    private const string WebViewVirtualHostName = "xrmtools-webview-host";
    private const string MappedBrowsingFileVirtualHostName = "browsing-file-host";
    private const string SpaVirtualHost = "app";

    public readonly WebView2 webView = new() { HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0), Visibility = Visibility.Hidden };

    // SPA interaction state
    private volatile bool _appReady = false;
    private string _lastFetchXml = string.Empty;
    private FetchQueryResultModel? _lastResult;

    // Messaging
    public event EventHandler<string>? WebMessageReceived; // raw JSON

    // Cache StringBuilder and Regex for better performance
    private static readonly ConcurrentQueue<StringBuilder> _stringBuilderPool = new();
    private static readonly Regex _escapeRegex = new(@"[\\\r\n""]", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly ConcurrentDictionary<string, string> _templateCache = new();

    public Browser()
    {
        webView.Initialized += BrowserInitialized;
        webView.NavigationStarting += BrowserNavigationStarting;
    }

    public void Dispose()
    {
        webView.Initialized -= BrowserInitialized;
        webView.NavigationStarting -= BrowserNavigationStarting;
        if (webView?.CoreWebView2 != null)
        {
            try { webView.CoreWebView2.WebResourceRequested -= CoreWebView2_WebResourceRequested; } catch { }
            try { webView.CoreWebView2.WebMessageReceived -= CoreWebView2_WebMessageReceived; } catch { }
        }
        webView.Dispose();
    }

    private void BrowserInitialized(object sender, EventArgs e)
    {

        ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
        {
            await InitializeWebView2CoreAsync();
            SetVirtualFolderMappings();
            webView.Visibility = Visibility.Visible;

            await NavigateToAppAsync();
        }).FireAndForget();

        async Task InitializeWebView2CoreAsync()
        {
            string tempDir = Path.Combine(Path.GetTempPath(), Assembly.GetExecutingAssembly().GetName().Name);

            // Register custom scheme for SPA to call back into VSIX: webapi://
            var reg = new CoreWebView2CustomSchemeRegistration("webapi")
            {
                TreatAsSecure = true,
                HasAuthorityComponent = true,
            };
            reg.AllowedOrigins.Add("*");
            var options = new CoreWebView2EnvironmentOptions(
                customSchemeRegistrations: [reg]);

            CoreWebView2Environment webView2Environment;
            try
            {
                webView2Environment = await CoreWebView2Environment.CreateAsync(browserExecutableFolder: null, userDataFolder: tempDir, options: options);
            }
            catch (NotSupportedException)
            {
                webView2Environment = await CoreWebView2Environment.CreateAsync(browserExecutableFolder: null, userDataFolder: tempDir, options: null);
            }
            await webView.EnsureCoreWebView2Async(webView2Environment);

            // Hook request handling for custom scheme, navigation, and web messages
            webView.CoreWebView2.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.All);
            webView.CoreWebView2.WebResourceRequested += CoreWebView2_WebResourceRequested;
            webView.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;
        }

        void SetVirtualFolderMappings()
        {
            webView.CoreWebView2.SetVirtualHostNameToFolderMapping(WebViewVirtualHostName, GetFolder(), CoreWebView2HostResourceAccessKind.Allow);
            string baseHref = GetFolder().Replace("\\", "/");
            webView.CoreWebView2.SetVirtualHostNameToFolderMapping(MappedBrowsingFileVirtualHostName, baseHref, CoreWebView2HostResourceAccessKind.Allow);
        }
    }

    private void CoreWebView2_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
    {
        try
        {
            string json = e.WebMessageAsJson; // already returns JSON string
            WebMessageReceived?.Invoke(this, json);
        }
        catch { /* ignore malformed */ }
    }

    public async Task SetHostThemeAsync(bool isDark)
    {
        try
        {
            await webView.EnsureCoreWebView2Async();

            webView.CoreWebView2.Profile.PreferredColorScheme = isDark ? CoreWebView2PreferredColorScheme.Dark : CoreWebView2PreferredColorScheme.Light;
        }
        catch { /* ignore */ }
    }

    private async Task NavigateToAppAsync()
    {
        // Prefer dev server in Debug
#if DEBUG
        var viteUrl = new Uri("http://localhost:56944/");
        if (await IsUpAsync(viteUrl, 15000))
        {
            webView.CoreWebView2.Navigate(viteUrl.ToString());
            webView.CoreWebView2.OpenDevToolsWindow();
            await WaitForAppReadyAsync(15000);
            // If we already have a query, push it
            if (!string.IsNullOrWhiteSpace(_lastFetchXml))
            {
                //_ = SubmitFetchXmlAsync(_lastFetchXml);
                _ = RenderFetchXmlResultAsync(_lastResult);
            }
            return;
        }
#endif
        // Fallback to built files from dist
        var dist = ResolveDistPath();
        if (Directory.Exists(dist))
        {
            webView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                SpaVirtualHost, dist, CoreWebView2HostResourceAccessKind.Allow);
            webView.CoreWebView2.Navigate($"https://{SpaVirtualHost}/index.html");
            await WaitForAppReadyAsync(15000);
            if (!string.IsNullOrWhiteSpace(_lastFetchXml))
            {
                //_ = SubmitFetchXmlAsync(_lastFetchXml);
                _ = RenderFetchXmlResultAsync(_lastResult);
            }
        }
        else
        {
            // Nothing to show; navigate to a blank page with a message
            webView.NavigateToString("<html><body style='font-family:Segoe UI;padding:12px'>FetchXml Viewer UI not found. Build the SPA or run Vite dev server.</body></html>");
        }
    }

    private void CoreWebView2_WebResourceRequested(object sender, CoreWebView2WebResourceRequestedEventArgs e)
    {
        try
        {
            var uri = new Uri(e.Request.Uri);
            if (!uri.Scheme.Equals("webapi", StringComparison.OrdinalIgnoreCase)) return;

            // Basic CORS headers for SPA
            var corsHeaders = new StringBuilder()
                .AppendLine("Access-Control-Allow-Origin: *")
                .AppendLine("Access-Control-Allow-Headers: content-type")
                .AppendLine("Access-Control-Allow-Methods: GET, POST, OPTIONS")
                .ToString();

            // Handle preflight
            if (string.Equals(e.Request.Method, "OPTIONS", StringComparison.OrdinalIgnoreCase))
            {
                e.Response = webView.CoreWebView2.Environment.CreateWebResourceResponse(Stream.Null, 204, "No Content", corsHeaders);
                return;
            }

            // Currently host-side execution path handles fetch logic; return empty placeholder.
            // in future this can route the requests to WebApiService if needed.
            var resultJson = "{\"value\":[]}";
            var bytes = Encoding.UTF8.GetBytes(resultJson);
            var stream = new MemoryStream(bytes);
            e.Response = webView.CoreWebView2.Environment.CreateWebResourceResponse(stream, 200, "OK", $"Content-Type: application/json\r\n{corsHeaders}");
        }
        catch
        {
            // swallow; leave default behavior
        }
    }

    private void BrowserNavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
    {
        ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
        {
            if (e.Uri is not string it || it.StartsWith("data:text/html;") || it.StartsWith("http:")) return;

            e.Cancel = true;

            Uri uri = new(e.Uri);

            // Handle vscmd:// URLs for Visual Studio command execution
            if (uri.Scheme == "vscmd")
            {
                string commandName = (uri.Host + uri.LocalPath).Trim('/');
                try
                {
                    await VS.Commands.ExecuteAsync(commandName);
                    await VS.StatusBar.ShowMessageAsync($"Executed command: {commandName}");
                }
                catch (Exception ex)
                {
                    await VS.StatusBar.ShowMessageAsync($"Failed to execute command '{commandName}': {ex.Message}");
                }
                return;
            }
        }).FireAndForget();
    }

    public async Task RefreshAsync()
    {
        // Re-submit last query if available and app ready
        if (!string.IsNullOrWhiteSpace(_lastFetchXml))
        {
            //await SubmitFetchXmlAsync(_lastFetchXml);
            await RenderFetchXmlResultAsync(_lastResult);
        }
    }

    public async Task SubmitFetchXmlAsync(string fetchXml)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        _lastFetchXml = fetchXml ?? string.Empty;
        if (string.IsNullOrWhiteSpace(fetchXml)) return;

        await WaitForAppReadyAsync(15000);
        if (!_appReady) return;

        string jsArg = ToJsStringLiteral(fetchXml);
        string script = $"(window.fetchXmlQuery && window.fetchXmlQuery({jsArg})) || null;";
        try
        {
            await webView.ExecuteScriptAsync(script);
        }
        catch
        {
            // ignore transient script errors
        }
    }

    public async Task RenderFetchXmlResultAsync(FetchQueryResultModel? result)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        if (_lastFetchXml is null) return;

        await WaitForAppReadyAsync(15000);
        if (!_appReady) return;

        string script = $"(window.renderFetchXmlResult && window.renderFetchXmlResult({result?.Result ?? "null"}, {{elapsedMs: {result?.ElapsedMs}, error: {result?.Error ?? "null"}}})) || null;";
        try
        {
            await webView.ExecuteScriptAsync(script);
        }
        catch
        {
            // ignore transient script errors
        }
    }

    public async Task SetLoadingStateAsync(bool state)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        await WaitForAppReadyAsync(15000);
        if (!_appReady) return;

        try
        {
            await webView.ExecuteScriptAsync($"(window.setLoading && window.setLoading({state.ToString().ToLowerInvariant()}))");
        }
        catch
        {
            // ignore transient script errors
        }
    }

    // Structured messaging helpers
    public async Task PostMessageAsync(object payload)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        try
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(payload);
            webView.CoreWebView2.PostWebMessageAsJson(json);
        }
        catch { }
    }

    public Task NotifyFetchStartedAsync(Guid requestId)
        => ExecuteScriptSafelyAsync($"window.fetchXmlRequestStarted && window.fetchXmlRequestStarted('{requestId}')");

    public Task NotifyFetchCancelledAsync(Guid requestId)
        => PostMessageAsync(new { v = 1, kind = "fetchxml/cancelled", requestId });

    private async Task ExecuteScriptSafelyAsync(string script)
    {
        try
        {
            await webView.ExecuteScriptAsync(script);
        }
        catch { }
    }

    private async Task WaitForAppReadyAsync(int timeoutMs)
    {
        var sw = Stopwatch.StartNew();
        while (sw.ElapsedMilliseconds < timeoutMs)
        {
            if (await IsAppReadyAsync()) return;
            await Task.Delay(300);
        }
    }

    private async Task<bool> IsAppReadyAsync()
    {
        if (_appReady) return true;
        try
        {
            var res = await webView.ExecuteScriptAsync("typeof window.setLoading === 'function'");
            if (res == "true")
            {
                _appReady = true;
                return true;
            }
        }
        catch (Exception) { }
        return false;
    }

    private static string ToJsStringLiteral(string s)
    {
        if (s == null) return "null";
        var sb = new StringBuilder(s.Length + 16);
        sb.Append('\"');
        foreach (var ch in s)
        {
            switch (ch)
            {
                case '\\': sb.Append("\\\\"); break;
                case '"': sb.Append("\\\""); break;
                case '\n': sb.Append("\\n"); break;
                case '\r': sb.Append("\\r"); break;
                case '\t': sb.Append("\\t"); break;
                default:
                    if (ch < ' ') sb.AppendFormat(CultureInfo.InvariantCulture, "\\u{0:X4}", (int)ch);
                    else sb.Append(ch);
                    break;
            }
        }
        sb.Append('\"');
        return sb.ToString();
    }

    private static async Task<bool> IsUpAsync(Uri url, int timeoutMs)
    {
        using var http = new HttpClient();
        var start = DateTime.UtcNow;
        while ((DateTime.UtcNow - start).TotalMilliseconds < timeoutMs)
        {
            try
            {
                var resp = await http.GetAsync(url);
                if (resp.IsSuccessStatusCode) return true;
            }
            catch { }
            await Task.Delay(300);
        }
        return false;
    }

    private static string ResolveDistPath()
    {
        // Try installed VSIX content next to the extension assembly
        var baseDir = GetFolder();
        var probe = Path.Combine(baseDir, "xrmtools.fetchxmlviewer", "dist");
        if (Directory.Exists(probe)) return probe;

        // Try parent folders (dev environment): ../../XrmTools.FetchXmlViewer/dist
        var dir = new DirectoryInfo(baseDir);
        for (int i = 0; i < 6 && dir != null; i++, dir = dir.Parent)
        {
            var candidate = Path.Combine(dir.FullName, "XrmTools.FetchXmlViewer", "dist");
            if (Directory.Exists(candidate)) return candidate;
        }

        return baseDir; // fallback to extension folder
    }

    private static StringBuilder GetOrCreateStringBuilder()
    {
        if (_stringBuilderPool.TryDequeue(out StringBuilder sb))
        {
            return sb;
        }
        return new StringBuilder(2048);
    }

    private static string EscapeForJavaScript(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        return _escapeRegex.Replace(input, m => m.Value switch
        {
            "\\" => "\\\\",
            "\r" => "\\r",
            "\n" => "\\n",
            "\"" => "\\\"",
            _ => m.Value
        });
    }

    public static string GetFolder()
    {
        string assembly = Assembly.GetExecutingAssembly().Location;
        return Path.GetDirectoryName(assembly);
    }

    private static string FindFileRecursively(string folder, string fileName, string fallbackFileName)
    {
        if (string.IsNullOrEmpty(folder)) return fallbackFileName;
        DirectoryInfo dir = new(folder);
        do
        {
            string candidate = Path.Combine(dir.FullName, fileName);
            if (File.Exists(candidate)) return candidate;
            dir = dir.Parent;
        } while (dir != null);
        return fallbackFileName;
    }
}
#nullable disable