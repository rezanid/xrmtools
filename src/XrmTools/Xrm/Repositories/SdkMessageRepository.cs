#nullable enable
namespace XrmTools.Xrm;

using Humanizer;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.Core;
using XrmTools.Core.Helpers;
using XrmTools.Core.Repositories;
using XrmTools.Http;
using XrmTools.Logging.Compatibility;
using XrmTools.Meta.Model;
using XrmTools.WebApi;

internal interface ISdkMessageRepository : IXrmRepository
{
    Task<IEnumerable<SdkMessage>> GetAsync(string entityLogicalName, CancellationToken cancellationToken);
    Task<IEnumerable<SdkMessage>> GetAsync(CancellationToken cancellationToken);
    Task<IEnumerable<SdkMessage>> GetAsync(string[] entityLogicalNames, CancellationToken cancellationToken);
}

internal class SdkMessageRepository(XrmHttpClient client, IWebApiService service, ILogger logger) : XrmRepository(client, service), ISdkMessageRepository
{
    private const string sdkMessageQuery = "sdkmessages?$filter=sdkmessageid_sdkmessagefilter/any(n:({0}) and n/iscustomprocessingstepallowed eq true)&$expand=sdkmessageid_sdkmessagefilter($filter=({1}) and iscustomprocessingstepallowed eq true)";
    private const string sdkMessageQuerySingle = "sdkmessages?$filter=sdkmessageid_sdkmessagefilter/any(n:n/primaryobjecttypecode eq '{0}' and n/iscustomprocessingstepallowed eq true)&$expand=sdkmessageid_sdkmessagefilter($filter=primaryobjecttypecode eq '{0}' and iscustomprocessingstepallowed eq true)";
    private const string sdkMessageQueryAll = "sdkmessages?$filter=sdkmessageid_sdkmessagefilter/any(n:n/iscustomprocessingstepallowed eq true)&$expand=sdkmessageid_sdkmessagefilter($filter=iscustomprocessingstepallowed eq true)";

    public async Task<IEnumerable<SdkMessage>> GetAsync(string[] entityLogicalNames, CancellationToken cancellationToken)
    {
        var sb1 = new StringBuilder();
        var sb2 = new StringBuilder();
        foreach (var name in entityLogicalNames)
        {
            sb1.Append($"n/primaryobjecttypecode eq '{name}' or ");
            sb2.Append($"primaryobjecttypecode eq '{name}' or ");
        }
        sb1.Remove(sb1.Length - 4, 4);
        sb2.Remove(sb2.Length - 4, 4);
        var response = await service.GetAsync(sdkMessageQuery.FormatWith(sb1.ToString(), sb2.ToString()), cancellationToken).ConfigureAwait(false);
        var typed = await response.CastAsync<ODataQueryResponse<SdkMessage>>().ConfigureAwait(false);
        if (typed is not null && typed.Value is not null)
        {
            return typed.Value;
        }
        return [];
    }

    public async Task<IEnumerable<SdkMessage>> GetAsync(string entityLogicalName, CancellationToken cancellationToken)
    {
        var response = await client.GetAsync(string.Format(sdkMessageQuerySingle, entityLogicalName), cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            using var content = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            return content.Deserialize<ODataQueryResponse<SdkMessage>>().Value;

        }
        return [];
    }

    public async Task<IEnumerable<SdkMessage>> GetAsync(CancellationToken cancellationToken)
    {
        var response = await service.GetAsync(sdkMessageQueryAll, cancellationToken).ConfigureAwait(false);
        var typed = await response.CastAsync<ODataQueryResponse<SdkMessage>>().ConfigureAwait(false);
        if (typed is not null && typed.Value is not null)
        {
            return typed.Value;
        }
        return [];
    }
}
#nullable restore