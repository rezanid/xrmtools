#nullable enable
namespace XrmTools.WebApi;

using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using XrmTools.Environments;
using XrmTools.Http;
using XrmTools.Logging.Compatibility;

internal interface IWebApiService
{
    Task<HttpResponseMessage> SendAsync(HttpRequestMessage request);
    Task<T> SendAsync<T>(HttpRequestMessage request) where T : HttpResponseMessage;
}


[Export(typeof(IWebApiService))]
[method: ImportingConstructor]
internal class WebApiService(
    IXrmHttpClientFactory httpClientFactory, IEnvironmentProvider environmentProvider, ILogger<WebApiService> logger) : IWebApiService
{
    private bool _disposedValue;
    private string? _sessionToken = null;

    public async Task<Uri?> GetBaseUrlAsync() => (await environmentProvider.GetActiveEnvironmentAsync())?.BaseServiceUrl;

    /// <summary>
    /// Processes requests and returns responses. Manages Service Protection Limit errors.
    /// </summary>
    /// <param name="request">The request to send.</param>
    /// <returns>The response from the HttpClient</returns>
    /// <exception cref="Exception"></exception>
    public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
    {
        // Session token used by elastic tables to enable strong consistency
        // See https://learn.microsoft.com/power-apps/developer/data-platform/use-elastic-tables?tabs=webapi#sending-the-session-token
        if (!string.IsNullOrWhiteSpace(_sessionToken) && request.Method == HttpMethod.Get) {
            request.Headers.Add("MSCRM.SessionToken", _sessionToken);
        }

        // Get the named HttpClient from the IHttpClientFactory
        using var client = await httpClientFactory.CreateClientAsync();
        
        var response = await client.SendAsync(request);

        // Capture the current session token value
        // See https://learn.microsoft.com/power-apps/developer/data-platform/use-elastic-tables?tabs=webapi#getting-the-session-token
        if (response.Headers.Contains("x-ms-session-token"))
        {
            _sessionToken = response.Headers.GetValues("x-ms-session-token")?.FirstOrDefault()?.ToString();
        }

        if (RecommendedDegreeOfParallelism <= 0 && 
            response.Headers.TryGetValues("x-ms-dop-hint", out var maxDopValues) &&
            maxDopValues.FirstOrDefault() is string maxDopValue &&
            int.TryParse(maxDopValue, out var maxDopInt))
        {
            RecommendedDegreeOfParallelism = maxDopInt;
        }

        if (!response.IsSuccessStatusCode && !response.Content.IsMimeMultipartContent())
        {
            throw await ParseExceptionAsync(response);
        }
        return response;
    }

    /// <summary>
    /// Processes requests with typed responses
    /// </summary>
    /// <typeparam name="T">The type derived from HttpResponseMessage</typeparam>
    /// <param name="request">The request</param>
    /// <returns></returns>
    public async Task<T> SendAsync<T>(HttpRequestMessage request) where T : HttpResponseMessage
    {
        var response = await SendAsync(request);

        // 'As' method is Extension of HttpResponseMessage see Extensions.cs
        return response.As<T>();
    }

    public static async Task<ServiceException> ParseExceptionAsync(HttpResponseMessage response)
    {
        string requestId = string.Empty;
        if (response.Headers.Contains("REQ_ID"))
        {
            requestId = response.Headers.GetValues("REQ_ID").FirstOrDefault();
        }

        var content = await response.Content.ReadAsStringAsync();
        ODataError? oDataError = null;

        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            oDataError = JsonSerializer.Deserialize<ODataError>(content, options);
        }
        catch (Exception)
        {
            // Error may not be in correct OData Error format, so keep trying...
        }

        if (oDataError != null && oDataError.Error != null)
        {

            var exception = new ServiceException(oDataError.Error.Message)
            {
                ODataError = oDataError,
                Content = content,
                ReasonPhrase = response.ReasonPhrase,
                HttpStatusCode = response.StatusCode,
                RequestId = requestId
            };
            return exception;
        }
        else
        {
            try
            {
                var oDataException = JsonSerializer.Deserialize<ODataException>(content);

                ServiceException otherException = new(oDataException.Message)
                {
                    Content = content,
                    ReasonPhrase = response.ReasonPhrase,
                    HttpStatusCode = response.StatusCode,
                    RequestId = requestId
                };
                return otherException;

            }
            catch (Exception)
            {

            }

            //When nothing else works
            ServiceException exception = new(response.ReasonPhrase)
            {
                Content = content,
                ReasonPhrase = response.ReasonPhrase,
                HttpStatusCode = response.StatusCode,
                RequestId = requestId
            };
            return exception;
        }
    }

    /// <summary>
    /// The recommended degree of parallelism for the connection.
    /// </summary>
    public int RecommendedDegreeOfParallelism { get; private set; }
}
#nullable restore