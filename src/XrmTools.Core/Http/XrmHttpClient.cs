#nullable enable
namespace XrmTools.Http;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Threading;

internal class XrmHttpClient(HttpMessageHandler handler, Action disposeCallback) : HttpMessageInvoker(handler, false), IDisposable
{
    private bool disposedValue;
    private readonly Action disposeCallback = disposeCallback;
    private readonly HttpClient httpClient = new (handler, disposeHandler: false);

    //
    // Summary:
    //     Gets or sets the base address of Uniform Resource Identifier (URI) of the Internet
    //     resource used when sending requests.
    //
    // Returns:
    //     The base address of Uniform Resource Identifier (URI) of the Internet resource
    //     used when sending requests.
    public Uri BaseAddress { get => httpClient.BaseAddress; set => httpClient.BaseAddress = value; }
    //
    // Summary:
    //     Gets the headers which should be sent with each request.
    //
    // Returns:
    //     The headers which should be sent with each request.
    public HttpRequestHeaders DefaultRequestHeaders { get => httpClient.DefaultRequestHeaders; }
    //
    // Summary:
    //     Gets or sets the timespan to wait before the request times out.
    //
    // Returns:
    //     The timespan to wait before the request times out.
    //
    // Exceptions:
    //   T:System.ArgumentOutOfRangeException:
    //     The timeout specified is less than or equal to zero and is not System.Threading.Timeout.InfiniteTimeSpan.
    //
    //
    //   T:System.InvalidOperationException:
    //     An operation has already been started on the current instance.
    //
    //   T:System.ObjectDisposedException:
    //     The current instance has been disposed.
    public TimeSpan Timeout { get => httpClient.Timeout; set => httpClient.Timeout = value; }
    //
    // Summary:
    //     Gets or sets the maximum number of bytes to buffer when reading the response
    //     content.
    //
    // Returns:
    //     The maximum number of bytes to buffer when reading the response content. The
    //     default value for this property is 2 gigabytes.
    //
    // Exceptions:
    //   T:System.ArgumentOutOfRangeException:
    //     The size specified is less than or equal to zero.
    //
    //   T:System.InvalidOperationException:
    //     An operation has already been started on the current instance.
    //
    //   T:System.ObjectDisposedException:
    //     The current instance has been disposed.
    public long MaxResponseContentBufferSize { get => httpClient.MaxResponseContentBufferSize; set => httpClient.MaxResponseContentBufferSize = value; }

    //
    // Summary:
    //     Cancel all pending requests on this instance.
    public void CancelPendingRequests() => httpClient.CancelPendingRequests();
    //
    // Summary:
    //     Send a DELETE request to the specified Uri as an asynchronous operation.
    //
    // Parameters:
    //   requestUri:
    //     The Uri the request is sent to.
    //
    // Returns:
    //     The task object representing the asynchronous operation.
    //
    // Exceptions:
    //   T:System.ArgumentNullException:
    //     The requestUri is null.
    //
    //   T:System.InvalidOperationException:
    //     The request message was already sent by the System.Net.Http.HttpClient instance.
    //
    //
    //   T:System.Net.Http.HttpRequestException:
    //     The request failed due to an underlying issue such as network connectivity, DNS
    //     failure, server certificate validation or timeout.
    public Task<HttpResponseMessage> DeleteAsync(string requestUri) => httpClient.DeleteAsync(requestUri);
    //
    // Summary:
    //     Send a DELETE request to the specified Uri with a cancellation token as an asynchronous
    //     operation.
    //
    // Parameters:
    //   requestUri:
    //     The Uri the request is sent to.
    //
    //   cancellationToken:
    //     A cancellation token that can be used by other objects or threads to receive
    //     notice of cancellation.
    //
    // Returns:
    //     The task object representing the asynchronous operation.
    //
    // Exceptions:
    //   T:System.ArgumentNullException:
    //     The requestUri is null.
    //
    //   T:System.InvalidOperationException:
    //     The request message was already sent by the System.Net.Http.HttpClient instance.
    //
    //
    //   T:System.Net.Http.HttpRequestException:
    //     The request failed due to an underlying issue such as network connectivity, DNS
    //     failure, server certificate validation or timeout.
    public Task<HttpResponseMessage> DeleteAsync(string requestUri, CancellationToken cancellationToken) 
        => httpClient.DeleteAsync(requestUri, cancellationToken);
    //
    // Summary:
    //     Send a DELETE request to the specified Uri with a cancellation token as an asynchronous
    //     operation.
    //
    // Parameters:
    //   requestUri:
    //     The Uri the request is sent to.
    //
    //   cancellationToken:
    //     A cancellation token that can be used by other objects or threads to receive
    //     notice of cancellation.
    //
    // Returns:
    //     The task object representing the asynchronous operation.
    //
    // Exceptions:
    //   T:System.ArgumentNullException:
    //     The requestUri is null.
    //
    //   T:System.InvalidOperationException:
    //     The request message was already sent by the System.Net.Http.HttpClient instance.
    //
    //
    //   T:System.Net.Http.HttpRequestException:
    //     The request failed due to an underlying issue such as network connectivity, DNS
    //     failure, server certificate validation or timeout.
    public Task<HttpResponseMessage> DeleteAsync(Uri requestUri, CancellationToken cancellationToken) 
        => httpClient.DeleteAsync(requestUri, cancellationToken);
    //
    // Summary:
    //     Send a DELETE request to the specified Uri as an asynchronous operation.
    //
    // Parameters:
    //   requestUri:
    //     The Uri the request is sent to.
    //
    // Returns:
    //     The task object representing the asynchronous operation.
    //
    // Exceptions:
    //   T:System.ArgumentNullException:
    //     The requestUri is null.
    //
    //   T:System.InvalidOperationException:
    //     The request message was already sent by the System.Net.Http.HttpClient instance.
    //
    //
    //   T:System.Net.Http.HttpRequestException:
    //     The request failed due to an underlying issue such as network connectivity, DNS
    //     failure, server certificate validation or timeout.
    public Task<HttpResponseMessage> DeleteAsync(Uri requestUri) => httpClient.DeleteAsync(requestUri);
    //
    // Summary:
    //     Send a GET request to the specified Uri with an HTTP completion option and a
    //     cancellation token as an asynchronous operation.
    //
    // Parameters:
    //   requestUri:
    //     The Uri the request is sent to.
    //
    //   completionOption:
    //     An HTTP completion option value that indicates when the operation should be considered
    //     completed.
    //
    //   cancellationToken:
    //     A cancellation token that can be used by other objects or threads to receive
    //     notice of cancellation.
    //
    // Returns:
    //     The task object representing the asynchronous operation.
    //
    // Exceptions:
    //   T:System.ArgumentNullException:
    //     The requestUri is null.
    //
    //   T:System.Net.Http.HttpRequestException:
    //     The request failed due to an underlying issue such as network connectivity, DNS
    //     failure, server certificate validation or timeout.
    public Task<HttpResponseMessage> GetAsync(string requestUri, HttpCompletionOption completionOption, CancellationToken cancellationToken) 
        => httpClient.GetAsync(requestUri, cancellationToken);
    //
    // Summary:
    //     Send a GET request to the specified Uri with a cancellation token as an asynchronous
    //     operation.
    //
    // Parameters:
    //   requestUri:
    //     The Uri the request is sent to.
    //
    //   cancellationToken:
    //     A cancellation token that can be used by other objects or threads to receive
    //     notice of cancellation.
    //
    // Returns:
    //     The task object representing the asynchronous operation.
    //
    // Exceptions:
    //   T:System.ArgumentNullException:
    //     The requestUri is null.
    //
    //   T:System.Net.Http.HttpRequestException:
    //     The request failed due to an underlying issue such as network connectivity, DNS
    //     failure, server certificate validation or timeout.
    public Task<HttpResponseMessage> GetAsync(Uri requestUri, CancellationToken cancellationToken) 
        => httpClient.GetAsync(requestUri, cancellationToken);
    //
    // Summary:
    //     Send a GET request to the specified Uri with an HTTP completion option and a
    //     cancellation token as an asynchronous operation.
    //
    // Parameters:
    //   requestUri:
    //     The Uri the request is sent to.
    //
    //   completionOption:
    //     An HTTP completion option value that indicates when the operation should be considered
    //     completed.
    //
    //   cancellationToken:
    //     A cancellation token that can be used by other objects or threads to receive
    //     notice of cancellation.
    //
    // Returns:
    //     The task object representing the asynchronous operation.
    //
    // Exceptions:
    //   T:System.ArgumentNullException:
    //     The requestUri is null.
    //
    //   T:System.Net.Http.HttpRequestException:
    //     The request failed due to an underlying issue such as network connectivity, DNS
    //     failure, server certificate validation or timeout.
    public Task<HttpResponseMessage> GetAsync(Uri requestUri, HttpCompletionOption completionOption, CancellationToken cancellationToken)
        => httpClient.GetAsync(requestUri, completionOption, cancellationToken);
    //
    // Summary:
    //     Send a GET request to the specified Uri with an HTTP completion option as an
    //     asynchronous operation.
    //
    // Parameters:
    //   requestUri:
    //     The Uri the request is sent to.
    //
    //   completionOption:
    //     An HTTP completion option value that indicates when the operation should be considered
    //     completed.
    //
    // Returns:
    //     The task object representing the asynchronous operation.
    //
    // Exceptions:
    //   T:System.ArgumentNullException:
    //     The requestUri is null.
    //
    //   T:System.Net.Http.HttpRequestException:
    //     The request failed due to an underlying issue such as network connectivity, DNS
    //     failure, server certificate validation or timeout.
    public Task<HttpResponseMessage> GetAsync(Uri requestUri, HttpCompletionOption completionOption)
        => httpClient.GetAsync(requestUri, completionOption);
    //
    // Summary:
    //     Send a GET request to the specified Uri with an HTTP completion option as an
    //     asynchronous operation.
    //
    // Parameters:
    //   requestUri:
    //     The Uri the request is sent to.
    //
    //   completionOption:
    //     An HTTP completion option value that indicates when the operation should be considered
    //     completed.
    //
    // Returns:
    //     The task object representing the asynchronous operation.
    //
    // Exceptions:
    //   T:System.ArgumentNullException:
    //     The requestUri is null.
    //
    //   T:System.Net.Http.HttpRequestException:
    //     The request failed due to an underlying issue such as network connectivity, DNS
    //     failure, server certificate validation or timeout.
    public Task<HttpResponseMessage> GetAsync(string requestUri, HttpCompletionOption completionOption)
        => httpClient.GetAsync(requestUri, completionOption);
    //
    // Summary:
    //     Send a GET request to the specified Uri as an asynchronous operation.
    //
    // Parameters:
    //   requestUri:
    //     The Uri the request is sent to.
    //
    // Returns:
    //     The task object representing the asynchronous operation.
    //
    // Exceptions:
    //   T:System.ArgumentNullException:
    //     The requestUri is null.
    //
    //   T:System.Net.Http.HttpRequestException:
    //     The request failed due to an underlying issue such as network connectivity, DNS
    //     failure, server certificate validation or timeout.
    public Task<HttpResponseMessage> GetAsync(Uri requestUri) => httpClient.GetAsync(requestUri);
    //
    // Summary:
    //     Send a GET request to the specified Uri as an asynchronous operation.
    //
    // Parameters:
    //   requestUri:
    //     The Uri the request is sent to.
    //
    // Returns:
    //     The task object representing the asynchronous operation.
    //
    // Exceptions:
    //   T:System.ArgumentNullException:
    //     The requestUri is null.
    //
    //   T:System.Net.Http.HttpRequestException:
    //     The request failed due to an underlying issue such as network connectivity, DNS
    //     failure, server certificate validation or timeout.
    public Task<HttpResponseMessage> GetAsync(string requestUri) => httpClient.GetAsync(requestUri);
    //
    // Summary:
    //     Send a GET request to the specified Uri with a cancellation token as an asynchronous
    //     operation.
    //
    // Parameters:
    //   requestUri:
    //     The Uri the request is sent to.
    //
    //   cancellationToken:
    //     A cancellation token that can be used by other objects or threads to receive
    //     notice of cancellation.
    //
    // Returns:
    //     The task object representing the asynchronous operation.
    //
    // Exceptions:
    //   T:System.ArgumentNullException:
    //     The requestUri is null.
    //
    //   T:System.Net.Http.HttpRequestException:
    //     The request failed due to an underlying issue such as network connectivity, DNS
    //     failure, server certificate validation or timeout.
    public Task<HttpResponseMessage> GetAsync(string requestUri, CancellationToken cancellationToken)
        => httpClient.GetAsync(requestUri, cancellationToken);
    //
    // Summary:
    //     Sends a GET request to the specified Uri and return the response body as a byte
    //     array in an asynchronous operation.
    //
    // Parameters:
    //   requestUri:
    //     The Uri the request is sent to.
    //
    // Returns:
    //     The task object representing the asynchronous operation.
    //
    // Exceptions:
    //   T:System.ArgumentNullException:
    //     The requestUri is null.
    //
    //   T:System.Net.Http.HttpRequestException:
    //     The request failed due to an underlying issue such as network connectivity, DNS
    //     failure, server certificate validation or timeout.
    public Task<byte[]> GetByteArrayAsync(string requestUri) => httpClient.GetByteArrayAsync(requestUri);
    //
    // Summary:
    //     Send a GET request to the specified Uri and return the response body as a byte
    //     array in an asynchronous operation.
    //
    // Parameters:
    //   requestUri:
    //     The Uri the request is sent to.
    //
    // Returns:
    //     The task object representing the asynchronous operation.
    //
    // Exceptions:
    //   T:System.ArgumentNullException:
    //     The requestUri is null.
    //
    //   T:System.Net.Http.HttpRequestException:
    //     The request failed due to an underlying issue such as network connectivity, DNS
    //     failure, server certificate validation or timeout.
    public Task<byte[]> GetByteArrayAsync(Uri requestUri) => httpClient.GetByteArrayAsync(requestUri);
    //
    // Summary:
    //     Send a GET request to the specified Uri and return the response body as a stream
    //     in an asynchronous operation.
    //
    // Parameters:
    //   requestUri:
    //     The Uri the request is sent to.
    //
    // Returns:
    //     The task object representing the asynchronous operation.
    //
    // Exceptions:
    //   T:System.ArgumentNullException:
    //     The requestUri is null.
    //
    //   T:System.Net.Http.HttpRequestException:
    //     The request failed due to an underlying issue such as network connectivity, DNS
    //     failure, server certificate validation or timeout.
    public Task<Stream> GetStreamAsync(Uri requestUri) => httpClient.GetStreamAsync(requestUri);
    //
    // Summary:
    //     Send a GET request to the specified Uri and return the response body as a stream
    //     in an asynchronous operation.
    //
    // Parameters:
    //   requestUri:
    //     The Uri the request is sent to.
    //
    // Returns:
    //     The task object representing the asynchronous operation.
    //
    // Exceptions:
    //   T:System.ArgumentNullException:
    //     The requestUri is null.
    //
    //   T:System.Net.Http.HttpRequestException:
    //     The request failed due to an underlying issue such as network connectivity, DNS
    //     failure, server certificate validation or timeout.
    public Task<Stream> GetStreamAsync(string requestUri) => httpClient.GetStreamAsync(requestUri);
    //
    // Summary:
    //     Send a GET request to the specified Uri and return the response body as a string
    //     in an asynchronous operation.
    //
    // Parameters:
    //   requestUri:
    //     The Uri the request is sent to.
    //
    // Returns:
    //     The task object representing the asynchronous operation.
    //
    // Exceptions:
    //   T:System.ArgumentNullException:
    //     The requestUri is null.
    //
    //   T:System.Net.Http.HttpRequestException:
    //     The request failed due to an underlying issue such as network connectivity, DNS
    //     failure, server certificate validation or timeout.
    public Task<string> GetStringAsync(string requestUri) => httpClient.GetStringAsync(requestUri);
    //
    // Summary:
    //     Send a GET request to the specified Uri and return the response body as a string
    //     in an asynchronous operation.
    //
    // Parameters:
    //   requestUri:
    //     The Uri the request is sent to.
    //
    // Returns:
    //     The task object representing the asynchronous operation.
    //
    // Exceptions:
    //   T:System.ArgumentNullException:
    //     The requestUri is null.
    //
    //   T:System.Net.Http.HttpRequestException:
    //     The request failed due to an underlying issue such as network connectivity, DNS
    //     failure, server certificate validation or timeout.
    public Task<string> GetStringAsync(Uri requestUri) => httpClient.GetStringAsync(requestUri);
    //
    // Summary:
    //     Send a POST request with a cancellation token as an asynchronous operation.
    //
    // Parameters:
    //   requestUri:
    //     The Uri the request is sent to.
    //
    //   content:
    //     The HTTP request content sent to the server.
    //
    //   cancellationToken:
    //     A cancellation token that can be used by other objects or threads to receive
    //     notice of cancellation.
    //
    // Returns:
    //     The task object representing the asynchronous operation.
    //
    // Exceptions:
    //   T:System.ArgumentNullException:
    //     The requestUri is null.
    //
    //   T:System.Net.Http.HttpRequestException:
    //     The request failed due to an underlying issue such as network connectivity, DNS
    //     failure, server certificate validation or timeout.
    public Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content, CancellationToken cancellationToken)
        => httpClient.PostAsync(requestUri, content, cancellationToken);
    //
    // Summary:
    //     Send a POST request with a cancellation token as an asynchronous operation.
    //
    // Parameters:
    //   requestUri:
    //     The Uri the request is sent to.
    //
    //   content:
    //     The HTTP request content sent to the server.
    //
    //   cancellationToken:
    //     A cancellation token that can be used by other objects or threads to receive
    //     notice of cancellation.
    //
    // Returns:
    //     The task object representing the asynchronous operation.
    //
    // Exceptions:
    //   T:System.ArgumentNullException:
    //     The requestUri is null.
    //
    //   T:System.Net.Http.HttpRequestException:
    //     The request failed due to an underlying issue such as network connectivity, DNS
    //     failure, server certificate validation or timeout.
    public Task<HttpResponseMessage> PostAsync(Uri requestUri, HttpContent content, CancellationToken cancellationToken)
        => httpClient.PostAsync(requestUri, content, cancellationToken);
    //
    // Summary:
    //     Send a POST request to the specified Uri as an asynchronous operation.
    //
    // Parameters:
    //   requestUri:
    //     The Uri the request is sent to.
    //
    //   content:
    //     The HTTP request content sent to the server.
    //
    // Returns:
    //     The task object representing the asynchronous operation.
    //
    // Exceptions:
    //   T:System.ArgumentNullException:
    //     The requestUri is null.
    //
    //   T:System.Net.Http.HttpRequestException:
    //     The request failed due to an underlying issue such as network connectivity, DNS
    //     failure, server certificate validation or timeout.
    public Task<HttpResponseMessage> PostAsync(Uri requestUri, HttpContent content) => httpClient.PostAsync(requestUri, content);
    //
    // Summary:
    //     Send a POST request to the specified Uri as an asynchronous operation.
    //
    // Parameters:
    //   requestUri:
    //     The Uri the request is sent to.
    //
    //   content:
    //     The HTTP request content sent to the server.
    //
    // Returns:
    //     The task object representing the asynchronous operation.
    //
    // Exceptions:
    //   T:System.ArgumentNullException:
    //     The requestUri is null.
    //
    //   T:System.Net.Http.HttpRequestException:
    //     The request failed due to an underlying issue such as network connectivity, DNS
    //     failure, server certificate validation or timeout.
    public Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content) => httpClient.PostAsync(requestUri, content);
    //
    // Summary:
    //     Send a PUT request with a cancellation token as an asynchronous operation.
    //
    // Parameters:
    //   requestUri:
    //     The Uri the request is sent to.
    //
    //   content:
    //     The HTTP request content sent to the server.
    //
    //   cancellationToken:
    //     A cancellation token that can be used by other objects or threads to receive
    //     notice of cancellation.
    //
    // Returns:
    //     The task object representing the asynchronous operation.
    //
    // Exceptions:
    //   T:System.ArgumentNullException:
    //     The requestUri is null.
    //
    //   T:System.Net.Http.HttpRequestException:
    //     The request failed due to an underlying issue such as network connectivity, DNS
    //     failure, server certificate validation or timeout.
    public Task<HttpResponseMessage> PutAsync(string requestUri, HttpContent content, CancellationToken cancellationToken)
        => httpClient.PutAsync(requestUri, content, cancellationToken);
    //
    // Summary:
    //     Send a PUT request to the specified Uri as an asynchronous operation.
    //
    // Parameters:
    //   requestUri:
    //     The Uri the request is sent to.
    //
    //   content:
    //     The HTTP request content sent to the server.
    //
    // Returns:
    //     The task object representing the asynchronous operation.
    //
    // Exceptions:
    //   T:System.ArgumentNullException:
    //     The requestUri is null.
    //
    //   T:System.Net.Http.HttpRequestException:
    //     The request failed due to an underlying issue such as network connectivity, DNS
    //     failure, server certificate validation or timeout.
    public Task<HttpResponseMessage> PutAsync(string requestUri, HttpContent content) => httpClient.PutAsync(requestUri, content);
    //
    // Summary:
    //     Send a PUT request with a cancellation token as an asynchronous operation.
    //
    // Parameters:
    //   requestUri:
    //     The Uri the request is sent to.
    //
    //   content:
    //     The HTTP request content sent to the server.
    //
    //   cancellationToken:
    //     A cancellation token that can be used by other objects or threads to receive
    //     notice of cancellation.
    //
    // Returns:
    //     The task object representing the asynchronous operation.
    //
    // Exceptions:
    //   T:System.ArgumentNullException:
    //     The requestUri is null.
    //
    //   T:System.Net.Http.HttpRequestException:
    //     The request failed due to an underlying issue such as network connectivity, DNS
    //     failure, server certificate validation or timeout.
    public Task<HttpResponseMessage> PutAsync(Uri requestUri, HttpContent content, CancellationToken cancellationToken)
        => httpClient.PutAsync(requestUri, content, cancellationToken);
    //
    // Summary:
    //     Send a PUT request to the specified Uri as an asynchronous operation.
    //
    // Parameters:
    //   requestUri:
    //     The Uri the request is sent to.
    //
    //   content:
    //     The HTTP request content sent to the server.
    //
    // Returns:
    //     The task object representing the asynchronous operation.
    //
    // Exceptions:
    //   T:System.ArgumentNullException:
    //     The requestUri is null.
    //
    //   T:System.Net.Http.HttpRequestException:
    //     The request failed due to an underlying issue such as network connectivity, DNS
    //     failure, server certificate validation or timeout.
    public Task<HttpResponseMessage> PutAsync(Uri requestUri, HttpContent content)
        => httpClient.PutAsync(requestUri, content);
    //
    // Summary:
    //     Send an HTTP request as an asynchronous operation.
    //
    // Parameters:
    //   request:
    //     The HTTP request message to send.
    //
    // Returns:
    //     The task object representing the asynchronous operation.
    //
    // Exceptions:
    //   T:System.ArgumentNullException:
    //     The request is null.
    //
    //   T:System.InvalidOperationException:
    //     The request message was already sent by the System.Net.Http.HttpClient instance.
    //
    //
    //   T:System.Net.Http.HttpRequestException:
    //     The request failed due to an underlying issue such as network connectivity, DNS
    //     failure, server certificate validation or timeout.
    public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        => httpClient.SendAsync(request);
    //
    // Summary:
    //     Send an HTTP request as an asynchronous operation.
    //
    // Parameters:
    //   request:
    //     The HTTP request message to send.
    //
    //   cancellationToken:
    //     The cancellation token to cancel operation.
    //
    // Returns:
    //     The task object representing the asynchronous operation.
    //
    // Exceptions:
    //   T:System.ArgumentNullException:
    //     The request is null.
    //
    //   T:System.InvalidOperationException:
    //     The request message was already sent by the System.Net.Http.HttpClient instance.
    //
    //
    //   T:System.Net.Http.HttpRequestException:
    //     The request failed due to an underlying issue such as network connectivity, DNS
    //     failure, server certificate validation or timeout.
    public override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        => httpClient.SendAsync(request, cancellationToken);
    //
    // Summary:
    //     Send an HTTP request as an asynchronous operation.
    //
    // Parameters:
    //   request:
    //     The HTTP request message to send.
    //
    //   completionOption:
    //     When the operation should complete (as soon as a response is available or after
    //     reading the whole response content).
    //
    // Returns:
    //     The task object representing the asynchronous operation.
    //
    // Exceptions:
    //   T:System.ArgumentNullException:
    //     The request is null.
    //
    //   T:System.InvalidOperationException:
    //     The request message was already sent by the System.Net.Http.HttpClient instance.
    //
    //
    //   T:System.Net.Http.HttpRequestException:
    //     The request failed due to an underlying issue such as network connectivity, DNS
    //     failure, server certificate validation or timeout.
    public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, HttpCompletionOption completionOption)
        => httpClient.SendAsync(request, completionOption);
    //
    // Summary:
    //     Send an HTTP request as an asynchronous operation.
    //
    // Parameters:
    //   request:
    //     The HTTP request message to send.
    //
    //   completionOption:
    //     When the operation should complete (as soon as a response is available or after
    //     reading the whole response content).
    //
    //   cancellationToken:
    //     The cancellation token to cancel operation.
    //
    // Returns:
    //     The task object representing the asynchronous operation.
    //
    // Exceptions:
    //   T:System.ArgumentNullException:
    //     The request is null.
    //
    //   T:System.InvalidOperationException:
    //     The request message was already sent by the System.Net.Http.HttpClient instance.
    //
    //
    //   T:System.Net.Http.HttpRequestException:
    //     The request failed due to an underlying issue such as network connectivity, DNS
    //     failure, server certificate validation or timeout.
    public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, HttpCompletionOption completionOption, CancellationToken cancellationToken)
        => httpClient.SendAsync(request, completionOption, cancellationToken);

    #region IDisposable Implementation

    protected override void Dispose(bool disposing)
    {
        if (!disposedValue && disposing)
        {
            disposeCallback.Invoke();
            httpClient.Dispose();
            disposedValue = true;
        }
        base.Dispose(disposing);
    }

    #endregion
}
#nullable restore