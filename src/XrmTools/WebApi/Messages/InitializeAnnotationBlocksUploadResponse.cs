﻿#nullable enable
namespace XrmTools.WebApi.Messages;

using Newtonsoft.Json.Linq;
using System.Net.Http;

/// <summary>
/// Contains the data from the InitializeAnnotationBlocksUploadRequest
/// </summary>
/// <remarks>
/// This class must be instantiated by either:
/// - The Service.SendAsync<T> method
/// - The HttpResponseMessage.As<T> extension in Extensions.cs
/// </remarks>
public sealed class InitializeAnnotationBlocksUploadResponse : HttpResponseMessage
{

    // Cache the async content
    private string? _content;

    // Provides JObject for property getters
    private JObject JObject
    {
        get
        {
            _content ??= Content.ReadAsStringAsync().GetAwaiter().GetResult();

            return JObject.Parse(_content);
        }
    }

    /// <summary>
    /// A token that uniquely identifies a sequence of related data uploads.
    /// </summary>
    public string FileContinuationToken => (string)JObject.GetValue(nameof(FileContinuationToken));

}
#nullable restore