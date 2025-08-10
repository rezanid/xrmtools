namespace XrmTools.WebApi.Messages;

using Newtonsoft.Json.Linq;
using System.Collections.Generic;

/// <summary>
/// Contains the data from the ExecuteCosmosSqlQueryResponse message.
/// </summary>
public class ExecuteCosmosSqlQueryResponse
{
    public string? PagingCookie { get; set; }
    public bool HasMore { get; set; }
    public List<JObject> Result { get; set; }
}