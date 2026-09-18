#nullable enable
namespace XrmTools.OData;

using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

/// <summary>Formats the managed HTTP exchange, not a byte-for-byte wire capture.</summary>
internal static class ODataResponseFormatter
{
    public static string Time(double milliseconds) => milliseconds < 1000 ? $"{milliseconds:0.##} ms" : $"{milliseconds / 1000:0.##} s";
    public static string Size(long bytes) => bytes < 1024 ? $"{bytes} B" : bytes < 1024 * 1024 ? $"{bytes / 1024d:0.#} KB" : $"{bytes / (1024d * 1024):0.#} MB";

    public static string Headers(HttpHeaders? headers)
    {
        if (headers == null) return "";
        var text = new StringBuilder();
        foreach (var header in headers.OrderBy(h => h.Key, StringComparer.OrdinalIgnoreCase))
        {
            bool sensitive = header.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase) ||
                header.Key.Equals("Proxy-Authorization", StringComparison.OrdinalIgnoreCase) ||
                header.Key.Equals("Cookie", StringComparison.OrdinalIgnoreCase) ||
                header.Key.Equals("Set-Cookie", StringComparison.OrdinalIgnoreCase);
            text.Append(header.Key).Append(": ").AppendLine(sensitive ? "[redacted]" : string.Join(", ", header.Value));
        }
        return text.ToString();
    }

    public static async Task<string> RawAsync(HttpRequestMessage request, HttpResponseMessage response, string responseBody)
    {
        var text = new StringBuilder("=== REQUEST ===").AppendLine()
            .AppendLine($"{request.Method} {request.RequestUri} HTTP/{request.Version}")
            .Append(Headers(request.Headers)).Append(Headers(request.Content?.Headers)).AppendLine();
        if (request.Content != null) text.AppendLine(await request.Content.ReadAsStringAsync().ConfigureAwait(false));
        text.AppendLine().AppendLine("=== RESPONSE ===")
            .AppendLine($"HTTP/{response.Version} {(int)response.StatusCode} {response.ReasonPhrase}")
            .Append(Headers(response.Headers)).Append(Headers(response.Content?.Headers)).AppendLine().Append(responseBody);
        return text.ToString();
    }
}
