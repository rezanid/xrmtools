namespace XrmTools.WebApi.Messages;

using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.Core.Repositories;
using XrmTools.WebApi;

// WebApiRequest wrapper to call RetrieveMetadataChanges action with optional ClientVersionStamp via GET for simplicity.
// If in future we need full query construction (filters, properties), we can convert to POST with body.
internal sealed class RetrieveMetadataChangesRequest : WebApiRequest<RetrieveMetadataChangesResponse>
{
    private readonly string _clientVersionStamp;
    private const string BaseUrl = "RetrieveMetadataChanges?$select=Timestamp&$expand=EntityMetadata($select=LogicalName,EntitySetName,PrimaryIdAttribute,PrimaryNameAttribute,ObjectTypeCode,SchemaName,LogicalCollectionName,ExternalCollectionName,Attributes,ManyToOneRelationships,OneToManyRelationships,ManyToManyRelationships)";

    public RetrieveMetadataChangesRequest(string clientVersionStamp)
    {
        Method = HttpMethod.Get;
        var url = BaseUrl;
        if (!string.IsNullOrWhiteSpace(clientVersionStamp))
        {
            url += "&ClientVersionStamp='" + System.Uri.EscapeDataString(clientVersionStamp) + "'";
        }
        RequestUri = new System.Uri(url, System.UriKind.Relative);
    }

    public override async Task<RetrieveMetadataChangesResponse> CreateResponseAsync(HttpResponseMessage raw, CancellationToken ct)
    {
        raw.EnsureSuccessStatusCode();
        return await raw.CastAsync<RetrieveMetadataChangesResponse>().ConfigureAwait(false);
    }
}
