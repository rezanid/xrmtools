﻿#nullable enable
namespace XrmTools.WebApi;

using System;
using System.Net;

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
}
#nullable restore