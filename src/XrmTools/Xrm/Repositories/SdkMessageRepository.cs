#nullable enable
namespace XrmTools.Xrm;

using Humanizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.Core;
using XrmTools.Core.Helpers;
using XrmTools.Core.Repositories;
using XrmTools.Logging.Compatibility;
using XrmTools.WebApi;
using XrmTools.WebApi.Entities;

internal interface ISdkMessageRepository : IXrmRepository
{
    Task<IEnumerable<SdkMessage>> GetForEntityAsync(string entityLogicalName, CancellationToken cancellationToken);
    Task<IEnumerable<SdkMessage>> GetCustomProcessingStepAllowedAsync(CancellationToken cancellationToken);
    Task<IEnumerable<SdkMessage>> GetForEntitiesAsync(string[] entityLogicalNames, CancellationToken cancellationToken);
    Task<IEnumerable<SdkMessage>> GetVisibleWithDescendantsAsync(CancellationToken cancellationToken);
    Task<SdkMessage?> GetByNameWithDescendantsAsync(string name, CancellationToken cancellationToken);
    Task<SdkMessage?> GetByNameWithDescendantsNoFiltersAsync(string name, CancellationToken cancellationToken);
    Task<IEnumerable<SdkMessage>> GetVisibleWithoutDescendantsAsync(CancellationToken cancellationToken);
}

internal class SdkMessageRepository(IWebApiService service, ILogger logger) : XrmRepository(service, new SlidingCacheConfiguration()), ISdkMessageRepository
{
    private const string sdkMessageQueryForEntities = "sdkmessages?$filter=sdkmessageid_sdkmessagefilter/any(n:({0}) and n/iscustomprocessingstepallowed eq true)&$expand=sdkmessageid_sdkmessagefilter($filter=({1}) and iscustomprocessingstepallowed eq true)";
    private const string sdkMessageQuerySingle = "sdkmessages?$filter=sdkmessageid_sdkmessagefilter/any(n:n/primaryobjecttypecode eq '{0}' and n/iscustomprocessingstepallowed eq true)&$expand=sdkmessageid_sdkmessagefilter($filter=primaryobjecttypecode eq '{0}' and iscustomprocessingstepallowed eq true)";
    private const string sdkMessageQueryForPlugins_WithExpand = "sdkmessages?$select=name&$filter=sdkmessageid_sdkmessagefilter/any(n:n/iscustomprocessingstepallowed eq true)&$expand=sdkmessageid_sdkmessagefilter($filter=iscustomprocessingstepallowed eq true)";
    private const string sdkMessageQueryForPlugins = "sdkmessages?$count=true&$select=name&$filter=sdkmessageid_sdkmessagefilter/any(f:f/iscustomprocessingstepallowed eq true)";
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

    private const string SdkMessageQueryByNameNoFilters = "sdkmessages?" +
        "$filter=name eq '{0}'" + 
        "&$expand=message_sdkmessagepair(" +
            "$select=sdkmessagepairid,namespace;" +
            "$expand=messagepair_sdkmessagerequest(" +
                "$select=sdkmessagerequestid,name;" +
                "$expand=messagerequest_sdkmessagerequestfield(" +
                    "$select=name,optional,position,publicname,clrparser,parser" +
                ")," +
                "messagerequest_sdkmessageresponse(" +
                    "$select=sdkmessageresponseid;" +
                    "$expand=messageresponse_sdkmessageresponsefield(" +
                        "$select=publicname,value,clrformatter,name,position" + //formatter,parameterbindinginformation
                    ")" +
                ")" +
            ")" +
        ")" +
        "&$select=name,isprivate,sdkmessageid,customizationlevel";

    public async Task<IEnumerable<SdkMessage>> GetForEntitiesAsync(string[] entityLogicalNames, CancellationToken cancellationToken)
    {
        var cacheKey = $"SdkMessages_ForEntities_{string.Join(",", entityLogicalNames.OrderBy(x => x))}";
        
        return await GetOrCreateCacheItemAsync(cacheKey, async () =>
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
            return Enumerable.Empty<SdkMessage>();
        }, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IEnumerable<SdkMessage>> GetForEntityAsync(string entityLogicalName, CancellationToken cancellationToken)
    {
        var cacheKey = $"SdkMessages_ForEntity_{entityLogicalName}";
        
        return await GetOrCreateCacheItemAsync(cacheKey, async () =>
        {
            var response = await service.GetAsync(string.Format(sdkMessageQuerySingle, entityLogicalName), cancellationToken).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                using var content = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                return content.Deserialize<ODataQueryResponse<SdkMessage>>().Value;
            }
            return Enumerable.Empty<SdkMessage>();
        }, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IEnumerable<SdkMessage>> GetCustomProcessingStepAllowedAsync(CancellationToken cancellationToken)
    {
        var cacheKey = "SdkMessages_CustomProcessingStepAllowed";
        
        return await GetOrCreateCacheItemAsync(cacheKey, async () =>
        {
            var response = await service.GetAsync(sdkMessageQueryForPlugins, cancellationToken).ConfigureAwait(false);
            var typed = await response.CastAsync<ODataQueryResponse<SdkMessage>>().ConfigureAwait(false);
            if (typed is not null && typed.Value is not null)
            {
                return typed.Value;
            }
            return Enumerable.Empty<SdkMessage>();
        }, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IEnumerable<SdkMessage>> GetVisibleWithDescendantsAsync(CancellationToken cancellationToken)
    {
        var cacheKey = "SdkMessages_VisibleWithDescendants";
        
        return await GetOrCreateCacheItemAsync(cacheKey, async () =>
        {
            var response = await service.GetAsync(SdkMessageQueryVisible, cancellationToken).ConfigureAwait(false);
            var typed = await response.CastAsync<ODataQueryResponse<SdkMessage>>().ConfigureAwait(false);
            if (typed is not null && typed.Value is not null)
            {
                return typed.Value;
            }
            return Enumerable.Empty<SdkMessage>();
        }, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IEnumerable<SdkMessage>> GetVisibleWithoutDescendantsAsync(CancellationToken cancellationToken)
    {
        var cacheKey = "SdkMessages_VisibleWithoutDescendants";
        
        return await GetOrCreateCacheItemAsync(cacheKey, async () =>
        {
            var response = await service.GetAsync(SdkMessageQueryVisible_NoDescendants, cancellationToken).ConfigureAwait(false);
            var typed = await response.CastAsync<ODataQueryResponse<SdkMessage>>().ConfigureAwait(false);
            if (typed is not null && typed.Value is not null)
            {
                return typed.Value;
            }
            return Enumerable.Empty<SdkMessage>();
        }, cancellationToken).ConfigureAwait(false);
    }

    public async Task<SdkMessage?> GetByNameWithDescendantsAsync(string name, CancellationToken cancellationToken)
    {
        var cacheKey = $"SdkMessage_ByName_{name}";
        
        return await GetOrCreateCacheItemAsync(cacheKey, async () =>
        {
            var response = await service.GetAsync(string.Format(SdkMessageQueryByName, name), cancellationToken).ConfigureAwait(false);
            var typed = await response.CastAsync<ODataQueryResponse<SdkMessage>>().ConfigureAwait(false);
            if (typed is not null && typed.Value is not null && typed.Value.Any())
            {
                return typed.Value.FirstOrDefault();
            }
            return null;
        }, cancellationToken).ConfigureAwait(false);
    }

    public async Task<SdkMessage?> GetByNameWithDescendantsNoFiltersAsync(string name, CancellationToken cancellationToken)
    {
        var cacheKey = $"SdkMessage_ByName_NoFilters_{name}";

        return await GetOrCreateCacheItemAsync(cacheKey, async () =>
        {
            HttpResponseMessage? response;
            try
            {
                response = await service.GetAsync(string.Format(SdkMessageQueryByNameNoFilters, name), cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to retrieve SdkMessage metadata from Dataverse.");
                return null;
            }
            var typed = await response.CastAsync<ODataQueryResponse<SdkMessage>>().ConfigureAwait(false);
            if (typed is not null && typed.Value is not null && typed.Value.Any())
            {
                return typed.Value.FirstOrDefault();
            }
            return null;
        }, cancellationToken).ConfigureAwait(false);
    }
}
#nullable restore