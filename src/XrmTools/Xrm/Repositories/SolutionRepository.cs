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

internal class SolutionRepository : XrmRepository, ISolutionRepository
{
    private const string SolutionQueryUnmanaged = "solutions?$filter=ismanaged eq false&$select=solutionid,uniquename,friendlyname,description&$orderby=friendlyname asc";

    public SolutionRepository(IWebApiService service, ILogger logger)
        : base(service, new SlidingCacheConfiguration())
    {
    }

    public async Task<IEnumerable<Solution>> GetUnmanagedAsync(CancellationToken cancellationToken)
    {
        var cacheKey = "Solutions_Unmanaged";

        return await GetOrCreateCacheItemAsync(cacheKey, async () =>
        {
            var response = await service.GetAsync(SolutionQueryUnmanaged, cancellationToken).ConfigureAwait(false);
            var typed = await response.CastAsync<ODataQueryResponse<Solution>>().ConfigureAwait(false);
            if (typed is not null && typed.Value is not null)
            {
                return typed.Value;
            }
            return Enumerable.Empty<Solution>();
        }, cancellationToken).ConfigureAwait(false);
    }
}
