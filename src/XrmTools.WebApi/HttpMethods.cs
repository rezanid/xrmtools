namespace XrmTools.Http;
using System.Net.Http;

internal static class HttpMethods
{
    internal static HttpMethod Patch { get; } = new HttpMethod("PATCH");
}
