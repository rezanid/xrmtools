namespace XrmTools.Xrm;

using System.Threading;
using System.Threading.Tasks;
using XrmTools.Core;
using XrmTools.Core.Repositories;
using XrmTools.Http;
using XrmTools.Logging.Compatibility;
using XrmTools.WebApi;
using XrmTools.WebApi.Messages;

internal interface ISystemRepository : IXrmRepository
{
    Task<Meta.Model.WhoAmIResponse> WhoAmIAsync(CancellationToken cancellationToken);
}

//internal class SystemRepository(XrmHttpClient client, ILogger logger) : XrmRepository(client), ISystemRepository
internal class SystemRepository(XrmHttpClient client, IWebApiService service, ILogger logger) : XrmRepository(client, service), ISystemRepository
{
    public async Task<Meta.Model.WhoAmIResponse> WhoAmIAsync(CancellationToken cancellationToken = default)
    {
        var response = await service.SendAsync(new WhoAmIRequest(), cancellationToken).ConfigureAwait(false);
        return await response.CastAsync<Meta.Model.WhoAmIResponse>();
    }
}
