namespace XrmTools.Core.Repositories;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.Logging.Compatibility;
using XrmTools.Meta.Model.Configuration;
using XrmTools.WebApi;

internal interface IPluginAssemblyRepository : IXrmRepository
{
    Task<IEnumerable<PluginAssemblyConfig>> GetAsync(CancellationToken cancellationToken);
}

internal class PluginAssemblyRepository(IWebApiService service, ILogger logger) : XrmRepository(service), IPluginAssemblyRepository
{
    public async Task<IEnumerable<PluginAssemblyConfig>> GetAsync(CancellationToken cancellationToken)
    {
        var response = await service.GetAsync("pluginassemblies?$select=pluginassemblyid,name,publickeytoken,solutionid,version,isolationmode,sourcetype", cancellationToken).ConfigureAwait(false);
        var typed = await response.CastAsync<ODataQueryResponse<PluginAssemblyConfig>>().ConfigureAwait(false);
        if (typed is not null && typed.Value is not null)
        {
            return typed.Value;
        }
        return [];
    }
}