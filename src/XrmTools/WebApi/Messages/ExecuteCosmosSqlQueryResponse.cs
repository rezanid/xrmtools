#nullable enable
namespace XrmTools.WebApi.Messages;

using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net.Http;

// This class must be instantiated by either:
// - The Service.SendAsync<T> method
// - The HttpResponseMessage.As<T> extension in Extensions.cs

/// <summary>
/// Contains the data from the ExecuteCosmosSqlQueryRequest
/// </summary>

public class ExecuteCosmosSqlQueryResponse : HttpResponseMessage
{

    // Cache the async content
    private string? _content;

    //Provides JObject for property getters
    private JObject _jObject
    {
        get
        {
            _content ??= Content.ReadAsStringAsync().GetAwaiter().GetResult();

            return JObject.Parse(_content);
        }
    }

    public string PagingCookie => _jObject[nameof(PagingCookie)].ToString();
    public bool HasMore => (bool)_jObject[nameof(HasMore)];
    public List<JObject> Result => _jObject[nameof(Result)].ToObject<List<JObject>>();
}
#nullable restore