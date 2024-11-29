namespace XrmTools.Xrm;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.Core;
using XrmTools.Core.Helpers;
using XrmTools.Core.Repositories;
using XrmTools.Http;
using XrmTools.Meta.Model;

internal interface ISystemRepository : IXrmRepository
{
    Task<WhoAmIResponse> WhoAmIAsync(CancellationToken cancellationToken);
}

internal class SystemRepository(XrmHttpClient client) : XrmRepository(client), ISystemRepository
{
    private const string whoamiQuery = "WhoAmI";

    public async Task<WhoAmIResponse> WhoAmIAsync(CancellationToken cancellationToken = default)
    {
        var response = await client.GetAsync(whoamiQuery, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            using var content = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            return content.Deserialize<WhoAmIResponse>();
        }
        return null;
    }
}
