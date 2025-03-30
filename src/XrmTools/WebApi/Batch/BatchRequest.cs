namespace XrmTools.WebApi.Batch;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;

public class BatchRequest : HttpRequestMessage
{
    /// <summary>
    /// BatchRequest constructor
    /// </summary>
    /// <param name="serviceBaseAddress">The Service.BaseAddress value.</param>
    public BatchRequest(Uri serviceBaseAddress)
    {
        Method = HttpMethod.Post;
        RequestUri = new Uri(uriString: "$batch", uriKind: UriKind.Relative);
        Content = new MultipartContent("mixed", $"batch_{Guid.NewGuid()}");
        ServiceBaseAddress = serviceBaseAddress;
    }

    private bool continueOnError;
    private readonly Uri ServiceBaseAddress;

    /// <summary>
    /// Sets the Prefer: odata.continue-on-error request header for the request.
    /// </summary>
    public bool ContinueOnError
    {
        get => continueOnError;
        set
        {
            if (continueOnError == value) return;
            if (value)
            {
                Headers.Add("Prefer", "odata.continue-on-error");
            }
            else
            {
                Headers.Remove("Prefer");
            }
            continueOnError = value;
        }
    }

    /// <summary>
    /// Sets the ChangeSets to be included in the request. Each <see cref="ChangeSet"/> represents a transaction.
    /// </summary>
    public List<ChangeSet> ChangeSets
    {
        set
        {
            value.ForEach(changeSet =>
            {
                var content = new MultipartContent("mixed", $"changeset_{Guid.NewGuid()}");

                int count = 1;
                changeSet.Requests.ForEach(request =>
                {
                    var messageContent = ToMessageContent(request);
                    messageContent.Headers.Add("Content-ID", count.ToString());

                    content.Add(messageContent);

                    count++;
                });
                ((MultipartContent)Content).Add(content);
            });
        }
    }

    /// <summary>
    /// Sets any requests to be sent outside of any ChangeSet
    /// </summary>
    public List<HttpRequestMessage> Requests
    {
        set
        {
            value.ForEach(request =>
            {
                ((MultipartContent)Content).Add(ToMessageContent(request));

            });
        }
    }

    /// <summary>
    /// Converts an HttpRequestMessage to HttpMessageContent
    /// </summary>
    /// <param name="request">The HttpRequestMessage to convert.</param>
    /// <returns>HttpMessageContent with the correct headers.</returns>
    private HttpMessageContent ToMessageContent(HttpRequestMessage request)
    {

        //Relative URI is not allowed with MultipartContent
        request.RequestUri = new Uri(
            baseUri: ServiceBaseAddress,
            relativeUri: request.RequestUri.ToString());

        if (request.Content != null)
        {
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            request.Content.Headers.ContentType.Parameters.Add(new("type", "entry"));
        }

        var messageContent = new HttpMessageContent(request);
        messageContent.Headers.ContentType = new("application/http");
        messageContent.Headers.Add("Content-Transfer-Encoding", "binary");

        return messageContent;
    }
}
