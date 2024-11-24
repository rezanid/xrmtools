namespace XrmTools.Xrm.Repositories;

using Humanizer;
using Microsoft.Xrm.Sdk.Metadata;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.Helpers;
using XrmTools.Http;

internal interface IEntityMetadataRepository
{
    Task<IEnumerable<EntityMetadata>> GetAsync(CancellationToken cancellationToken);
    Task<EntityMetadata> GetAsync(string entityLogicalName, CancellationToken cancellationToken);
}

internal class EntityMetadataRepository(XrmHttpClient client) : IEntityMetadataRepository
{
    private const string entityMetadataQueryAll = "https://aguflowq.crm4.dynamics.com/api/data/v9.2/RetrieveAllEntities(EntityFilters=@p1,RetrieveAsIfPublished=@p2)?@p1=Microsoft.Dynamics.CRM.EntityFilters%27Entity%27&@p2=true";
    private const string entityMetadataQuerySingle = "https://aguflowq.crm4.dynamics.com/api/data/v9.2/RetrieveAllEntities(EntityFilters=@p1,RetrieveAsIfPublished=@p2)?@p1=Microsoft.Dynamics.CRM.EntityFilters%27Entity%27&@p2=true";
    private readonly XrmHttpClient client = client;

    public async Task<IEnumerable<EntityMetadata>> GetAsync(CancellationToken cancellationToken)
    {
        var response = await client.GetAsync(entityMetadataQueryAll, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            using var content = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            return content.Deserialize<IEnumerable<EntityMetadata>>();
        }
        return [];
    }

    public async Task<EntityMetadata> GetAsync(string entityLogicalName, CancellationToken cancellationToken)
    {
        var response = await client.GetAsync(entityMetadataQuerySingle.FormatWith(entityLogicalName), cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            using var content = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            return content.Deserialize<EntityMetadata>();
        }
        return null;
    }
}