namespace XrmTools.Core.Repositories;

using Humanizer;
using Microsoft.Xrm.Sdk.Metadata;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.Core.Helpers;
using XrmTools.Http;
using XrmTools.Logging.Compatibility;
using XrmTools.Meta.Model;
using XrmTools.Serialization;
using XrmTools.WebApi;
using XrmTools.WebApi.Entities;
using EntityMetadata = Microsoft.Xrm.Sdk.Metadata.EntityMetadata;

internal interface IEntityMetadataRepository : IXrmRepository
{
    /// <summary>
    /// Get all the entities metadata.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task<IEnumerable<EntityMetadata>> GetAsync(CancellationToken cancellationToken);
    Task<IEnumerable<EntityMetadata>> GetByMessageNameAsync(string messageName, CancellationToken cancellationToken);
    Task<EntityMetadata> GetAsync(string entityLogicalName, CancellationToken cancellationToken);
    Task<IEnumerable<SdkMessage>> GetAvailableMessagesAsync(string entityLogicalName, CancellationToken cancellationToken);
    Task<EntityMetadata> GetRelationshipsAsync(string entityLogicalName, CancellationToken cancellationToken);
    Task<IEnumerable<string>> GetEntityNamesAsync(string messageName, CancellationToken cancellationToken = default);
}

internal class EntityMetadataRepository(XrmHttpClient client, IWebApiService service, ILogger logger) : XrmRepository(client, service), IEntityMetadataRepository
{
    private const string entityMetadataQueryAll = "RetrieveAllEntities(EntityFilters=@p1,RetrieveAsIfPublished=@p2)?@p1=Microsoft.Dynamics.CRM.EntityFilters'Entity'&@p2=true";
    private const string entityMetadataQuerySingle = "RetrieveEntity(LogicalName=@p1,EntityFilters=@p2,RetrieveAsIfPublished=@p3,MetadataId=@p4)?@p1='{0}'&@p2=Microsoft.Dynamics.CRM.EntityFilters'Attributes'&@p3=true&@p4=00000000-0000-0000-0000-000000000000";
    private const string entityMessagesQuery =
        "sdkmessages?$select=name,isprivate,executeprivilegename,isvalidforexecuteasync,autotransact,introducedversion" +
        "&$filter=sdkmessageid_sdkmessagefilter/any(n:n/primaryobjecttypecode eq '{0}' and n/iscustomprocessingstepallowed eq true))" +
        "&$expand=sdkmessageid_sdkmessagefilter($filter=primaryobjecttypecode eq '{0}' and iscustomprocessingstepallowed eq true)";
    private const string entityRelationshipsQuery = "EntityDefinitions(LogicalName='{0}')?$select=MetadataId&$expand=ManyToOneRelationships,OneToManyRelationships,ManyToManyRelationships";
    private const string sdkMessageEntityNamesQuery = "sdkmessagefilters?$filter=sdkmessageid/name eq '{0}'&$select=primaryobjecttypecode";

    private static readonly JsonSerializerSettings serializerSetting = new()
    {
        ContractResolver = new PolymorphicContractResolver()
    };

    public async Task<IEnumerable<string>> GetEntityNamesAsync(string messageName, CancellationToken cancellationToken = default)
    {
        var response = await service.GetAsync(sdkMessageEntityNamesQuery.FormatWith(messageName), cancellationToken).ConfigureAwait(false);
        if (response.IsSuccessStatusCode)
        {
            using var content = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            var typed = content.Deserialize<ODataQueryResponse<SdkMessageFilter>>().Value;
            if (typed is not null && typed.Any())
            {
                return typed.Select(e => e.PrimaryObjectTypeCode).Distinct();
            }
        }
        return [];
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<EntityMetadata>> GetAsync(CancellationToken cancellationToken)
    {
        var response = await service.SendAsync(new(HttpMethod.Get, entityMetadataQueryAll), cancellationToken).ConfigureAwait(false);
        var typed = await response.CastAsync<RetrieveAllEntitiesResponse>().ConfigureAwait(false);
        if (typed is not null && typed.EntityMetadata is not null)
        {
            return typed.EntityMetadata;
        }
        return [];
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<EntityMetadata>> GetByMessageNameAsync(string messageName, CancellationToken cancellationToken)
    {
        var response = await service.SendAsync(new(HttpMethod.Get, entityMetadataQueryAll), cancellationToken).ConfigureAwait(false);
        var typed = await response.CastAsync<RetrieveAllEntitiesResponse>().ConfigureAwait(false);
        if (typed is not null && typed.EntityMetadata is not null)
        {
            return typed.EntityMetadata;
        }
        return [];
    }

    /// <summary>
    /// Get the available messages for the entity.
    /// </summary>
    /// <param name="entityLogicalName">The logical name of the entity to retrieve its metadata.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    public async Task<IEnumerable<SdkMessage>> GetAvailableMessagesAsync(string entityLogicalName, CancellationToken cancellationToken)
    {
        var response = await service.SendAsync(new(HttpMethod.Get, entityMessagesQuery.FormatWith(entityLogicalName)), cancellationToken).ConfigureAwait(false);
        var typed = await response.CastAsync<ODataQueryResponse<SdkMessage>>().ConfigureAwait(false);
        if (typed is not null && typed.Value is not null)
        {
            return typed.Value;
        }
        return [];
    }

    /// <summary>
    /// Get the metadata for the entity including its attributes.
    /// </summary>
    /// <param name="entityLogicalName">The logical name of the entity to retrieve its metadata.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    public async Task<EntityMetadata> GetAsync(string entityLogicalName, CancellationToken cancellationToken)
    {
        var response = await service.GetAsync(entityMetadataQuerySingle.FormatWith(entityLogicalName), cancellationToken).ConfigureAwait(false);
        using var content = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        using var reader = new StreamReader(content);
        using var jreader = new JsonTextReader(reader);
        var jobj = (JObject)(await JToken.ReadFromAsync(jreader, cancellationToken).ConfigureAwait(false));
        var serializer = new Newtonsoft.Json.JsonSerializer()
        {
            ContractResolver = new PolymorphicContractResolver(),
        };
        return JsonConvert.DeserializeObject<EntityMetadata>(jobj.GetValue("EntityMetadata").ToString(), serializerSetting);
    }

    /// <summary>
    /// Get the relationships for the entity. 
    /// Check <c>EntityMetadata.OneToManyRelationships</c>, <c>EntityMetadata.ManyToOneRelationships</c>, and <c>EntityMetadata.ManyToManyRelationships</c> properties.
    /// </summary>
    /// <param name="entityLogicalName">The logical name of the entity to retrieve its relationships.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    public async Task<EntityMetadata> GetRelationshipsAsync(string entityLogicalName, CancellationToken cancellationToken)
    {
        var response = await service.GetAsync(entityRelationshipsQuery.FormatWith(entityLogicalName), cancellationToken).ConfigureAwait(false);
        if (response.IsSuccessStatusCode)
        {
            using var content = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            using var reader = new StreamReader(content);
            using var jreader = new JsonTextReader(reader);
            var jobj = (JObject)(await JToken.ReadFromAsync(jreader, cancellationToken).ConfigureAwait(false));
            var serializer = new Newtonsoft.Json.JsonSerializer()
            {
                ContractResolver = new PolymorphicContractResolver(),
            };
            return JsonConvert.DeserializeObject<EntityMetadata>(jobj.GetValue("EntityMetadata").ToString(), serializerSetting);
        }
        return null;
    }
}