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
        logger.LogTrace("Retrieving plugin assembly configurations from Dataverse.");
        var typed = await service.QueryAsync<PluginAssemblyConfig>("pluginassemblies?$select=pluginassemblyid,name,publickeytoken,solutionid,version,isolationmode,sourcetype", cancellationToken).ConfigureAwait(false);
        if (typed is not null && typed.Value is not null)
        {
            logger.LogTrace("Plugin assembly configurations retrieved successfully.");
            return typed.Value;
        }
        
        logger.LogTrace("No plugin assembly configurations found.");
        return [];
    }
}