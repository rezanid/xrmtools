﻿#nullable enable
namespace XrmTools.WebApi;

public class ODataException
{
    public string? Message { get; set; }
    public string? ExceptionMessage { get; set; }
    public string? StackTrace { get; set; }
    public string? ErrorCode { get; set; }
}
#nullable restore