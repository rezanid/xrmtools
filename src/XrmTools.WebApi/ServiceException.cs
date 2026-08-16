#nullable enable
namespace XrmTools.WebApi;

using System;
using System.Net;
using System.Runtime.Serialization;

[Serializable]
public class ServiceException : Exception
{
    public HttpStatusCode HttpStatusCode { get; set; }

    public string? ReasonPhrase { get; set; }

    public ODataError? ODataError { get; set; }

    public string? Content { get; set; }

    public string? RequestId { get; set; }

    public ServiceException() { }

    public ServiceException(string message) : base(message) { }

    public ServiceException(string message, Exception inner) : base(message, inner) { }

    protected ServiceException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
        HttpStatusCode = (HttpStatusCode)info.GetInt32(nameof(HttpStatusCode));
        ReasonPhrase = info.GetString(nameof(ReasonPhrase));
        Content = info.GetString(nameof(Content));
        RequestId = info.GetString(nameof(RequestId));
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(HttpStatusCode), (int)HttpStatusCode);
        info.AddValue(nameof(ReasonPhrase), ReasonPhrase);
        info.AddValue(nameof(Content), Content);
        info.AddValue(nameof(RequestId), RequestId);
    }
}
#nullable restore