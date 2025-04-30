#nullable enable
namespace XrmTools.WebApi.Messages;

using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Text;
using XrmTools.Http;
using XrmTools.Xrm.Model;
using XrmTools.WebApi;
    
/// <summary>
/// Contains the data to upsert a record.
/// </summary>
public sealed class UpsertRequest : HttpRequestMessage
{
    /// <summary>
    /// Initializes the UpsertRequest
    /// </summary>
    /// <param name="entityReference">A reference to a record. This should use alternate keys.</param>
    /// <param name="record">The data to create or update.</param>
    /// <param name="upsertBehavior">Control the upsert behavior.</param>
    public UpsertRequest(
        EntityReference entityReference,
        JObject record,
        UpsertBehavior upsertBehavior = UpsertBehavior.CreateOrUpdate,
        string? solutionUniqueName = null)
    {
        Method = HttpMethods.Patch;
        RequestUri = new Uri(uriString: entityReference.Path, uriKind: UriKind.Relative);
        Content = new StringContent(
                content: record.ToString(),
                encoding: Encoding.UTF8,
                mediaType: "application/json");
        switch (upsertBehavior)
        {
            case UpsertBehavior.PreventCreate:
                Headers.Add("If-Match", "*");
                break;
            case UpsertBehavior.PreventUpdate:
                Headers.Add("If-None-Match", "*");
                break;
        }
        if (!string.IsNullOrEmpty(solutionUniqueName))
        {
            Headers.Add("MSCRM.SolutionUniqueName", solutionUniqueName);
        }
    }
}

/// <summary>
/// Specifies the behavior for an Upsert operation.
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class UpsertRequest<T> : HttpRequestMessage where T : TypedEntity<T>, new()
{
    public UpsertRequest(T record, UpsertBehavior upsertBehavior = UpsertBehavior.CreateOrUpdate)
    {
        Method = HttpMethods.Patch;
        RequestUri = new Uri(uriString: new EntityReference(record.GetEntitySetName(), record.Id).Path, uriKind: UriKind.Relative);
        Content = new StringContent(
                content: Newtonsoft.Json.JsonConvert.SerializeObject(record),
                encoding: Encoding.UTF8,
                mediaType: "application/json");
        switch (upsertBehavior)
        {
            case UpsertBehavior.PreventCreate:
                Headers.Add("If-Match", "*");
                break;
            case UpsertBehavior.PreventUpdate:
                Headers.Add("If-None-Match", "*");
                break;
        }
    }
}

/// <summary>
/// Specifies the behavior for an Upsert operation.
/// </summary>
public enum UpsertBehavior
{
    CreateOrUpdate = 0,
    PreventUpdate = 1,
    PreventCreate = 2
}
#nullable restore