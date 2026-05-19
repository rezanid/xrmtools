namespace XrmTools.Xrm;

using System.Threading;
using System.Threading.Tasks;
using XrmTools.Core;
using XrmTools.Core.Repositories;
using XrmTools.Logging.Compatibility;
using XrmTools.WebApi;
using XrmTools.WebApi.Messages;

internal interface ISystemRepository : IXrmRepository
{
    Task<WhoAmIResponse> WhoAmIAsync(CancellationToken cancellationToken);
}

internal class SystemRepository(IWebApiService service, ILogger logger) : XrmRepository(service), ISystemRepository
{
    public async Task<WhoAmIResponse> WhoAmIAsync(CancellationToken cancellationToken = default)
    {
        logger.LogTrace("Executing WhoAmI request.");
        var result = await service.SendAsync(new WhoAmIRequest(), cancellationToken: cancellationToken).ConfigureAwait(false);
        logger.LogTrace("WhoAmI request completed.");
        return result;
    }
}