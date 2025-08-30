#nullable enable
namespace XrmTools.WebApi.Batch;

using System;
using System.Collections.Generic;
using System.Net.Http;

public class ChangeSet
{
    private readonly List<HttpRequestMessage> requests = [];

    public ChangeSet(List<HttpRequestMessage> requests) => Requests = requests;

    /// <summary>
    /// Sets Requests to send with the change set
    /// </summary>
    public List<HttpRequestMessage> Requests
    {
        set {
            requests.Clear();
            value.ForEach(x => {
                if (x.Method == HttpMethod.Get)
                {
                    throw new ArgumentException("ChangeSets cannot contain GET requests.");
                }
                requests.Add(x);
            });                 
        }
        get => requests;
    }

}
#nullable restore