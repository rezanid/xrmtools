namespace XrmTools.Core.Repositories;

using Humanizer;
using Microsoft.Xrm.Sdk.Metadata;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.Core.Helpers;
using XrmTools.Http;
using XrmTools.Meta.Model;

internal interface IEntityMetadataRepository : IXrmRepository
{
    Task<IEnumerable<EntityMetadata>> GetAsync(CancellationToken cancellationToken);
    Task<EntityMetadata> GetAsync(string entityLogicalName, CancellationToken cancellationToken);
    Task<IEnumerable<SdkMessage>> GetAvailableMessageAsync(string entityLogicalName, CancellationToken cancellationToken);
}

internal class EntityMetadataRepository(XrmHttpClient client) : XrmRepository(client), IEntityMetadataRepository
{
    private const string entityMetadataQueryAll = "RetrieveAllEntities(EntityFilters=@p1,RetrieveAsIfPublished=@p2)?@p1=Microsoft.Dynamics.CRM.EntityFilters%27Entity%27&@p2=true";
    private const string entityMetadataQuerySingle = "RetrieveEntity(LogicalName=@p1,EntityFilters=@p2,RetrieveAsIfPublished=@p3,MetadataId=@p4)?@p1=%27{0}%27&@p2=Microsoft.Dynamics.CRM.EntityFilters%27Attributes%27&@p3=true&@p4=00000000-0000-0000-0000-000000000000";
    private const string entityMessagesQuery = "sdkmessages?$select=name,isprivate,executeprivilegename,isvalidforexecuteasync,autotransact,introducedversion&$filter=sdkmessageid_sdkmessagefilter/any(n:%20n/primaryobjecttypecode%20eq%20%27{0}%27)";

    public async Task<IEnumerable<EntityMetadata>> GetAsync(CancellationToken cancellationToken)
    {
        var response = await client.GetAsync(entityMetadataQueryAll, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            using var content = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            return content.Deserialize<RetrieveAllEntitiesResponse>().EntityMetadata;
        }
        return [];
    }

    public async Task<EntityMetadata> GetAsync(string entityLogicalName, CancellationToken cancellationToken)
    {
        var response = await client.GetAsync(entityMetadataQuerySingle.FormatWith(entityLogicalName), cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            using var content = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            return content.Deserialize<RetrieveEntityResponse>().EntityMetadata;
        }
        return null;
    }

    public async Task<IEnumerable<SdkMessage>> GetAvailableMessageAsync(string entityLogicalName, CancellationToken cancellationToken)
    {
        var response = await client.GetAsync(entityMessagesQuery.FormatWith(entityLogicalName), cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            using var content = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            return content.Deserialize<ODataQueryResponse<SdkMessage>>().Entities;
        }
        return [];
    }
}