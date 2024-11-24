namespace XrmTools.Xrm.Repositories;

using Humanizer;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.Helpers;
using XrmTools.Http;
using XrmTools.Meta.Model;
using XrmTools.Xrm.Model;

internal interface IPluginTypeRepository
{
    Task<IEnumerable<PluginTypeConfig>> GetAsync(Guid pluginassemblyid, CancellationToken cancellationToken);
}

internal class PluginTypeRepository(XrmHttpClient client) : IPluginTypeRepository
{
    private const string plugintypeQuery = "plugintypes?" +
        "$filter=_pluginassemblyid_value eq '{0}'" +
        "&$select=plugintypeid,name,typename,friendlyname,description,workflowactivitygroupname&" +
        "$expand=plugintype_sdkmessageprocessingstep(" +
            "$select=sdkmessageprocessingstepid,name,stage,asyncautodelete,description,filteringattributes,invocationsource,mode,rank,sdkmessageid,statecode,supporteddeployment;" +
            "$expand=sdkmessageprocessingstepid_sdkmessageprocessingstepimage(" +
                "$select=sdkmessageprocessingstepimageid,name,imagetype,messagepropertyname,attributes,entityalias))";
    private readonly XrmHttpClient client = client;

    public async Task<IEnumerable<PluginTypeConfig>> GetAsync(Guid pluginassemblyid, CancellationToken cancellationToken)
    {
        var response = await client.GetAsync(plugintypeQuery.FormatWith(pluginassemblyid), cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            using var content = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            return content.Deserialize<ODataQueryResponse<PluginTypeConfig>>().Entities;
        }
        return [];
    }
}