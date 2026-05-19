namespace XrmTools.Xrm.Repositories;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.Core;
using XrmTools.Core.Repositories;
using XrmTools.Logging.Compatibility;
using XrmTools.WebApi;
using XrmTools.WebApi.Entities;

internal interface ISolutionRepository : IXrmRepository
{
    Task<IEnumerable<Solution>> GetUnmanagedAsync(CancellationToken cancellationToken);
}

internal class SolutionRepository(IWebApiService service, ILogger logger) : XrmRepository(service, new SlidingCacheConfiguration()), ISolutionRepository
{
    private const string SolutionQueryUnmanaged = "solutions?$filter=ismanaged eq false&$select=solutionid,uniquename,friendlyname,description&$orderby=friendlyname asc";

    public async Task<IEnumerable<Solution>> GetUnmanagedAsync(CancellationToken cancellationToken)
    {
        logger.LogTrace("Retrieving unmanaged solutions from Dataverse.");
        var cacheKey = "Solutions_Unmanaged";

        return await GetOrCreateCacheItemAsync(cacheKey, async () =>
        {
            var typed = await service.QueryAsync<Solution>(SolutionQueryUnmanaged, cancellationToken).ConfigureAwait(false);
            if (typed is not null && typed.Value is not null)
            {
                logger.LogTrace("Found {Count} unmanaged solutions.", typed.Value.Count());
                return typed.Value;
            }
            logger.LogTrace("No unmanaged solutions found.");
            return Enumerable.Empty<Solution>();
        }, cancellationToken).ConfigureAwait(false);
    }
}
