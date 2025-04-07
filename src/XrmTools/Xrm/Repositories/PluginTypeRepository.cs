namespace XrmTools.Core.Repositories;

using Humanizer;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.Core.Helpers;
using XrmTools.Http;
using XrmTools.Logging.Compatibility;
using XrmTools.Meta.Model;
using XrmTools.WebApi;
using XrmTools.Xrm.Model;

internal interface IPluginTypeRepository : IXrmRepository
{
    Task<IEnumerable<PluginTypeConfig>> GetAsync(Guid pluginassemblyid, CancellationToken cancellationToken);
}

internal class PluginTypeRepository(XrmHttpClient client, IWebApiService service, ILogger logger) : XrmRepository(client, service), IPluginTypeRepository
{
    private const string plugintypeQuery = "plugintypes?" +
        "$filter=_pluginassemblyid_value eq '{0}'" +
        "&$select=plugintypeid,name,typename,friendlyname,description,workflowactivitygroupname&" +
        "$expand=plugintype_sdkmessageprocessingstep(" +
            "$select=sdkmessageprocessingstepid,name,stage,asyncautodelete,description,filteringattributes,invocationsource,mode,rank,sdkmessageid,statecode,supporteddeployment;" +
            "$expand=sdkmessageprocessingstepid_sdkmessageprocessingstepimage(" +
                "$select=sdkmessageprocessingstepimageid,name,imagetype,messagepropertyname,attributes,entityalias))";

    public async Task<IEnumerable<PluginTypeConfig>> GetAsync(Guid pluginassemblyid, CancellationToken cancellationToken)
    {
        var response = await service.GetAsync(plugintypeQuery.FormatWith(pluginassemblyid), cancellationToken).ConfigureAwait(false);
        var typed = await response.CastAsync<ODataQueryResponse<PluginTypeConfig>>().ConfigureAwait(false);
        if (typed is not null && typed.Entities is not null)
        {
            return typed.Entities;
        }
        return [];
    }
}