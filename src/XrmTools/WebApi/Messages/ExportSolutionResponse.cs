﻿#nullable enable
namespace XrmTools.WebApi.Messages;

using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;

// This class must be instantiated by either:
// - The Service.SendAsync<T> method
// - The HttpResponseMessage.As<T> extension in Extensions.cs

/// <summary>
/// Contains the data from the ExportSolutionRequest
/// </summary>
public sealed class ExportSolutionResponse : HttpResponseMessage
{

    //Provides JObject for property getters
    private JObject _jObject
    {
        get
        {
            return JObject.Parse(Content.ReadAsStringAsync().GetAwaiter().GetResult());
        }
    }

    /// <summary>
    /// The contents of the downloaded file.
    /// </summary>
    public byte[] ExportSolutionFile
    {
        get
        {
            string filestring = _jObject["ExportSolutionFile"].ToString();
            return Convert.FromBase64String(filestring);
        }
    }
}
#nullable restore