#nullable enable
namespace XrmTools.Xrm;

using Humanizer;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.Core;
using XrmTools.Core.Helpers;
using XrmTools.Core.Repositories;
using XrmTools.Http;
using XrmTools.Logging.Compatibility;
using XrmTools.Meta.Model;
using XrmTools.WebApi;
using XrmTools.WebApi.Entities;

internal interface ISdkMessageRepository : IXrmRepository
{
    Task<IEnumerable<SdkMessage>> GetForEntityAsync(string entityLogicalName, CancellationToken cancellationToken);
    Task<IEnumerable<SdkMessage>> GetCustomProcessingStepAllowedAsync(CancellationToken cancellationToken);
    Task<IEnumerable<SdkMessage>> GetForEntitiesAsync(string[] entityLogicalNames, CancellationToken cancellationToken);
    Task<IEnumerable<SdkMessage>> GetVisibleWithDescendantsAsync(CancellationToken cancellationToken);
    Task<SdkMessage?> GetByNameWithDescendantsAsync(string name, CancellationToken cancellationToken);
    Task<IEnumerable<SdkMessage>> GetVisibleWithoutDescendantsAsync(CancellationToken cancellationToken);
}

internal class SdkMessageRepository(XrmHttpClient client, IWebApiService service, ILogger logger) : XrmRepository(client, service), ISdkMessageRepository
{
    private const string sdkMessageQueryForEntities = "sdkmessages?$filter=sdkmessageid_sdkmessagefilter/any(n:({0}) and n/iscustomprocessingstepallowed eq true)&$expand=sdkmessageid_sdkmessagefilter($filter=({1}) and iscustomprocessingstepallowed eq true)";
    private const string sdkMessageQuerySingle = "sdkmessages?$filter=sdkmessageid_sdkmessagefilter/any(n:n/primaryobjecttypecode eq '{0}' and n/iscustomprocessingstepallowed eq true)&$expand=sdkmessageid_sdkmessagefilter($filter=primaryobjecttypecode eq '{0}' and iscustomprocessingstepallowed eq true)";
    private const string sdkMessageQueryForPlugins = "sdkmessages?$filter=sdkmessageid_sdkmessagefilter/any(n:n/iscustomprocessingstepallowed eq true)&$expand=sdkmessageid_sdkmessagefilter($filter=iscustomprocessingstepallowed eq true)";
    private const string SdkMessageQueryVisible_NoDescendants = "sdkmessages?" +
        "$filter=message_sdkmessagepair/any(p:p/endpoint eq '2011/Organization.svc') and " +
            "sdkmessageid_sdkmessagefilter/any(f:f/isvisible eq true)" +
        "&$select=name,isprivate,sdkmessageid,customizationlevel";
    private const string SdkMessageQueryVisible = "sdkmessages?" +
        "$filter=message_sdkmessagepair/any(p:p/endpoint eq '2011/Organization.svc') and " +
            "sdkmessageid_sdkmessagefilter/any(f:f/isvisible eq true)" +
        "&$expand=message_sdkmessagepair(" +
            "$filter=endpoint eq '2011/Organization.svc';" +
            "$select=sdkmessagepairid,namespace;" +
            "$expand=messagepair_sdkmessagerequest(" +
                "$select=sdkmessagerequestid,name;" +
                "$expand=messagerequest_sdkmessagerequestfield(" +
                    "$select=name,optional,position,publicname,clrparser" +
                ")," +
                "messagerequest_sdkmessageresponse(" +
                    "$select=sdkmessageresponseid;" +
                    "$expand=messageresponse_sdkmessageresponsefield(" +
                        "$select=publicname,value,clrformatter,name,position" +
                    ")" +
                ")" +
            ")" +
        ")," +
        "sdkmessageid_sdkmessagefilter($select=isvisible)" +
        "&$select=name,isprivate,sdkmessageid,customizationlevel";
    private const string SdkMessageQueryByName = "sdkmessages?" +
        "$filter=name eq '{0}' and message_sdkmessagepair/any(p:p/endpoint eq '2011/Organization.svc') and " +
            "sdkmessageid_sdkmessagefilter/any(f:f/isvisible eq true)" +
        "&$expand=message_sdkmessagepair(" +
            "$filter=endpoint eq '2011/Organization.svc';" +
            "$select=sdkmessagepairid,namespace;" +
            "$expand=messagepair_sdkmessagerequest(" +
                "$select=sdkmessagerequestid,name;" +
                "$expand=messagerequest_sdkmessagerequestfield(" +
                    "$select=name,optional,position,publicname,clrparser" +
                ")," +
                "messagerequest_sdkmessageresponse(" +
                    "$select=sdkmessageresponseid;" +
                    "$expand=messageresponse_sdkmessageresponsefield(" +
                        "$select=publicname,value,clrformatter,name,position" +
                    ")" +
                ")" +
            ")" +
        ")," +
        "sdkmessageid_sdkmessagefilter($select=isvisible)" +
        "&$select=name,isprivate,sdkmessageid,customizationlevel";
    public async Task<IEnumerable<SdkMessage>> GetForEntitiesAsync(string[] entityLogicalNames, CancellationToken cancellationToken)
    {
        var sb1 = new StringBuilder();
        var sb2 = new StringBuilder();
        foreach (var name in entityLogicalNames)
        {
            sb1.Append($"n/primaryobjecttypecode eq '{name}' or ");
            sb2.Append($"primaryobjecttypecode eq '{name}' or ");
        }
        sb1.Remove(sb1.Length - 4, 4);
        sb2.Remove(sb2.Length - 4, 4);
        var response = await service.GetAsync(sdkMessageQueryForEntities.FormatWith(sb1.ToString(), sb2.ToString()), cancellationToken).ConfigureAwait(false);
        var typed = await response.CastAsync<ODataQueryResponse<SdkMessage>>().ConfigureAwait(false);
        if (typed is not null && typed.Value is not null)
        {
            return typed.Value;
        }
        return [];
    }

    public async Task<IEnumerable<SdkMessage>> GetForEntityAsync(string entityLogicalName, CancellationToken cancellationToken)
    {
        var response = await client.GetAsync(string.Format(sdkMessageQuerySingle, entityLogicalName), cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            using var content = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            return content.Deserialize<ODataQueryResponse<SdkMessage>>().Value;

        }
        return [];
    }

    public async Task<IEnumerable<SdkMessage>> GetCustomProcessingStepAllowedAsync(CancellationToken cancellationToken)
    {
        var response = await service.GetAsync(sdkMessageQueryForPlugins, cancellationToken).ConfigureAwait(false);
        var typed = await response.CastAsync<ODataQueryResponse<SdkMessage>>().ConfigureAwait(false);
        if (typed is not null && typed.Value is not null)
        {
            return typed.Value;
        }
        return [];
    }

    public async Task<IEnumerable<SdkMessage>> GetVisibleWithDescendantsAsync(CancellationToken cancellationToken)
    {
        var response = await service.GetAsync(SdkMessageQueryVisible, cancellationToken).ConfigureAwait(false);
        var typed = await response.CastAsync<ODataQueryResponse<SdkMessage>>().ConfigureAwait(false);
        if (typed is not null && typed.Value is not null)
        {
            return typed.Value;
        }
        return [];
    }

    //TODO: Needs pagination support, because server will paginate with 11 records per page by default.
    public async Task<IEnumerable<SdkMessage>> GetVisibleWithoutDescendantsAsync(CancellationToken cancellationToken)
    {
        var response = await service.GetAsync(SdkMessageQueryVisible_NoDescendants, cancellationToken).ConfigureAwait(false);
        var typed = await response.CastAsync<ODataQueryResponse<SdkMessage>>().ConfigureAwait(false);
        if (typed is not null && typed.Value is not null)
        {
            return typed.Value;
        }
        return [];
    }

    public async Task<SdkMessage?> GetByNameWithDescendantsAsync(string name, CancellationToken cancellationToken)
    {
        var response = await service.GetAsync(string.Format(SdkMessageQueryByName, name), cancellationToken).ConfigureAwait(false);
        var typed = await response.CastAsync<ODataQueryResponse<SdkMessage>>().ConfigureAwait(false);
        if (typed is not null && typed.Value is not null && typed.Value.Any())
        {
            return typed.Value.FirstOrDefault();
        }
        return null;
    }

}
#nullable restore