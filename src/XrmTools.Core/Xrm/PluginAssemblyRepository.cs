namespace XrmTools.Core.Repositories;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.Core.Helpers;
using XrmTools.Http;
using XrmTools.Meta.Model;
using XrmTools.Xrm.Model;

internal interface IPluginAssemblyRepository : IXrmRepository
{
    Task<IEnumerable<PluginAssemblyConfig>> GetAsync(CancellationToken cancellationToken);
}

internal class PluginAssemblyRepository(XrmHttpClient client) : XrmRepository(client), IPluginAssemblyRepository
{
    public async Task<IEnumerable<PluginAssemblyConfig>> GetAsync(CancellationToken cancellationToken)
    {
        var response = await client.GetAsync("pluginassemblies?$select=pluginassemblyid,name,publickeytoken,solutionid,version,isolationmode,sourcetype", cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            using var content = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            return content.Deserialize<ODataQueryResponse<PluginAssemblyConfig>>().Entities;
        }
        return [];
    }
}