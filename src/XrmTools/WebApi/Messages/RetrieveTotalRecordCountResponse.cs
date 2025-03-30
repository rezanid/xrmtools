#nullable enable
namespace XrmTools.WebApi.Messages;

using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using XrmTools.WebApi.Types;
using EntityRecordCountCollection = Types.EntityRecordCountCollection;

/// <summary>
/// Contains the response from the RetrieveTotalRecordCountRequest 
/// </summary>
/// <remarks>
/// This class must be instantiated by either:
/// - The Service.SendAsync<T> method
/// - The HttpResponseMessage.As<T> extension in Extensions.cs
/// </remarks>
public sealed class RetrieveTotalRecordCountResponse : HttpResponseMessage
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

    /// <summary>
    /// Gets the collection of results for the RetrieveTotalRecordCount Function.
    /// </summary>
    public EntityRecordCountCollection EntityRecordCountCollection => JsonConvert.DeserializeObject<EntityRecordCountCollection>(_jObject["EntityRecordCountCollection"].ToString());
}
#nullable restore