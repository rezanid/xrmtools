namespace XrmTools.WebApi.Batch;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

public sealed class BatchResponse
{
    private readonly byte[] content;
    private readonly MediaTypeHeaderValue? contentType;

    private BatchResponse(
        HttpStatusCode statusCode,
        IReadOnlyDictionary<string, IEnumerable<string>> headers,
        byte[] content,
        MediaTypeHeaderValue? contentType)
    {
        StatusCode = statusCode;
        Headers = headers;
        this.content = content;
        this.contentType = contentType;
    }

    public HttpStatusCode StatusCode { get; }

    public IReadOnlyDictionary<string, IEnumerable<string>> Headers { get; }

    /// <summary>
    /// Processes the Multi-part content returned from the batch into a list of responses.
    /// </summary>
    public async Task<List<HttpResponseMessage>> ParseResponseAsync(CancellationToken cancellationToken = default)
    {
        using var batchContent = new ByteArrayContent(content);
        if (contentType is not null)
        {
            batchContent.Headers.ContentType = Clone(contentType);
        }

        return await ParseBatchResponseAsync(batchContent, cancellationToken).ConfigureAwait(false);
    }

    internal static async Task<BatchResponse> FromAsync(HttpResponseMessage raw, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var content = raw.Content is null
            ? []
            : await raw.Content.ReadAsByteArrayAsync().ConfigureAwait(false);

        return new BatchResponse(
            raw.StatusCode,
            raw.Headers.ToHeaderDictionary(),
            content,
            raw.Content?.Headers.ContentType is null ? null : Clone(raw.Content.Headers.ContentType));
    }

    private static async Task<List<HttpResponseMessage>> ParseBatchResponseAsync(HttpContent content, CancellationToken cancellationToken)
    {
        if (content == null) return [];

        // NOTE: MultipartMemoryStreamProvider is still usable via Microsoft.AspNet.WebApi.Client
        var multipart = await content.ReadAsMultipartAsync(cancellationToken);

        if (multipart == null || multipart.Contents == null)
        {
            return [];
        }

        var responses = new List<HttpResponseMessage>();
        
        foreach (var httpContent in multipart.Contents)
        {
            if (httpContent.IsMimeMultipartContent())
            {
                // Handle nested multipart (changeset)
                responses.AddRange(await ParseBatchResponseAsync(httpContent, cancellationToken));
            }
            else
            {
                // ReadAsHttpResponseMessageAsync won't work unless we replace 'Content-Type' header.
                httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/http");
                httpContent.Headers.ContentType.Parameters.Add(new NameValueHeaderValue("msgtype", "response"));

                var httpResponseMessage = await httpContent.ReadAsHttpResponseMessageAsync();

                if (httpResponseMessage != null)
                {
                    responses.Add(httpResponseMessage);
                }
            }
        }

        return responses;
    }

    private static MediaTypeHeaderValue Clone(MediaTypeHeaderValue value)
    {
        var clone = new MediaTypeHeaderValue(value.MediaType);
        foreach (var parameter in value.Parameters)
        {
            clone.Parameters.Add(new NameValueHeaderValue(parameter.Name, parameter.Value));
        }

        foreach (var charset in value.CharSet is null ? Enumerable.Empty<string>() : [value.CharSet])
        {
            clone.CharSet = charset;
        }

        return clone;
    }
}
