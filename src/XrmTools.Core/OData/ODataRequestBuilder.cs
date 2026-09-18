#nullable enable
namespace XrmTools.OData;

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

internal static class ODataRequestBuilder
{
    public static HttpRequestMessage Build(ODataRequest request, Uri serviceRoot, string token)
    {
        if (serviceRoot.Scheme != Uri.UriSchemeHttps) throw new InvalidOperationException("The environment must use HTTPS.");
        // A leading slash is relative to the Dataverse Web API root, not the host root.
        Uri target;
        if (request.Target.StartsWith("//", StringComparison.Ordinal) || request.Target.Contains("\\"))
            throw new FormatException("Network-path URLs and backslashes are not supported.");
        if (request.Target.StartsWith("/api/", StringComparison.OrdinalIgnoreCase)) target = new Uri(new Uri(serviceRoot.GetLeftPart(UriPartial.Authority)), request.Target);
        else if (request.Target.StartsWith("/", StringComparison.Ordinal)) target = new Uri(serviceRoot, request.Target.TrimStart('/'));
        else if (!Uri.TryCreate(request.Target, UriKind.Absolute, out target!)) target = new Uri(serviceRoot, request.Target);
        if (!string.Equals(target.Scheme, serviceRoot.Scheme, StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(target.Host, serviceRoot.Host, StringComparison.OrdinalIgnoreCase) || target.Port != serviceRoot.Port ||
            target.UserInfo.Length != 0 || target.Fragment.Length != 0 ||
            !target.AbsolutePath.StartsWith(serviceRoot.AbsolutePath, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Requests must target the selected environment's Web API root.");

        var message = new HttpRequestMessage(new HttpMethod(request.Method), target);
        try
        {
            if (request.Body.Length > 0) message.Content = new StringContent(request.Body, Encoding.UTF8, "application/json");
            foreach (var header in request.Headers)
            {
                if (header.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase) || header.Key.Equals("Host", StringComparison.OrdinalIgnoreCase) ||
                    header.Key.Equals("Content-Length", StringComparison.OrdinalIgnoreCase) || header.Key.Equals("Transfer-Encoding", StringComparison.OrdinalIgnoreCase) ||
                    header.Key.Equals("Proxy-Authorization", StringComparison.OrdinalIgnoreCase))
                    throw new FormatException($"Header '{header.Key}' is managed by Xrm Tools and cannot be overridden.");
                if (header.Value.Contains("\r") || header.Value.Contains("\n")) throw new FormatException("Header values cannot contain newlines.");
                if (header.Key.StartsWith("Content-", StringComparison.OrdinalIgnoreCase))
                {
                    message.Content ??= new ByteArrayContent([]);
                    message.Content.Headers.Remove(header.Key);
                    message.Content.Headers.Add(header.Key, header.Value);
                }
                else message.Headers.Add(header.Key, header.Value);
            }
            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            if (message.Headers.Accept.Count == 0) message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            if (!message.Headers.Contains("OData-Version")) message.Headers.Add("OData-Version", "4.0");
            if (!message.Headers.Contains("OData-MaxVersion")) message.Headers.Add("OData-MaxVersion", "4.0");
            return message;
        }
        catch { message.Dispose(); throw; }
    }
}
