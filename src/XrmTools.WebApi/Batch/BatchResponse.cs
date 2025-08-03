namespace XrmTools.WebApi.Batch;

using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Threading;

public class BatchResponse : HttpResponseMessage
{
    /// <summary>
    /// Processes the Multi-part content returned from the batch into a list of responses.
    /// </summary>
    public Task<List<HttpResponseMessage>> ParseResponseAsync(CancellationToken cancellationToken = default)
        => ParseBatchResponseAsync(Content, cancellationToken);

    private static async Task<List<HttpResponseMessage>> ParseBatchResponseAsync(HttpContent content, CancellationToken cancellationToken)
    {
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
}
