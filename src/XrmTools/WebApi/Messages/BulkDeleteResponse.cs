#nullable enable
namespace XrmTools.WebApi.Messages;

using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;

public sealed class BulkDeleteResponse : HttpResponseMessage
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
    public Guid JobId
    {
        get
        {
            return (Guid)_jObject[nameof(JobId)];
        }
    }
}
#nullable restore