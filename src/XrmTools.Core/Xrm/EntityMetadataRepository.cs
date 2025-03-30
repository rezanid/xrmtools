namespace XrmTools.Core.Repositories;

using Humanizer;
using Microsoft.Xrm.Sdk.Metadata;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Runtime;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using XrmTools.Core.Helpers;
using XrmTools.Http;
using XrmTools.Meta.Model;
using XrmTools.Serialization;

internal interface IEntityMetadataRepository : IXrmRepository
{
    Task<IEnumerable<EntityMetadata>> GetAsync(CancellationToken cancellationToken);
    Task<EntityMetadata> GetAsync(string entityLogicalName, CancellationToken cancellationToken);
    Task<IEnumerable<SdkMessage>> GetAvailableMessageAsync(string entityLogicalName, CancellationToken cancellationToken);
    Task<ExpandoObject> GetAsExpandoAsync(string entityLogicalName, CancellationToken cancellationToken);
    Task<EntityMetadata> GetRelationshipsAsync(string entityLogicalName, CancellationToken cancellationToken);
}

internal class EntityMetadataRepository(XrmHttpClient client) : XrmRepository(client), IEntityMetadataRepository
{
    private const string entityMetadataQueryAll = "RetrieveAllEntities(EntityFilters=@p1,RetrieveAsIfPublished=@p2)?@p1=Microsoft.Dynamics.CRM.EntityFilters'Entity'&@p2=true";
    private const string entityMetadataQuerySingle = "RetrieveEntity(LogicalName=@p1,EntityFilters=@p2,RetrieveAsIfPublished=@p3,MetadataId=@p4)?@p1='{0}'&@p2=Microsoft.Dynamics.CRM.EntityFilters'Attributes'&@p3=true&@p4=00000000-0000-0000-0000-000000000000";
    private const string entityMessagesQuery =
        "sdkmessages?$select=name,isprivate,executeprivilegename,isvalidforexecuteasync,autotransact,introducedversion" +
        "&$filter=sdkmessageid_sdkmessagefilter/any(n:n/primaryobjecttypecode eq '{0}' and n/iscustomprocessingstepallowed eq true))" +
        "&$expand=sdkmessageid_sdkmessagefilter($filter=primaryobjecttypecode eq '{0}' and iscustomprocessingstepallowed eq true)";
    private const string entityRelationshipsQuery = "EntityDefinitions(LogicalName='{0}')?$select=MetadataId&$expand=ManyToOneRelationships,OneToManyRelationships,ManyToManyRelationships";

    private static readonly JsonSerializerSettings serializerSetting = new()
    {
        ContractResolver = new PolymorphicContractResolver()
    };

    /// <summary>
    /// Get all the entities metadata.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
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

    /// <summary>
    /// Get the available messages for the entity.
    /// </summary>
    /// <param name="entityLogicalName">The logical name of the entity to retrieve its metadata.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
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

    /// <summary>
    /// Get the metadata for the entity including its attributes.
    /// </summary>
    /// <param name="entityLogicalName">The logical name of the entity to retrieve its metadata.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    public async Task<EntityMetadata> GetAsync(string entityLogicalName, CancellationToken cancellationToken)
    {
        var response = await client.GetAsync(entityMetadataQuerySingle.FormatWith(entityLogicalName), cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            using var content = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            using var reader = new StreamReader(content);
            using var jreader = new JsonTextReader(reader);
            var jobj = (JObject)(await JToken.ReadFromAsync(jreader, cancellationToken));
            var serializer = new Newtonsoft.Json.JsonSerializer()
            {
                ContractResolver = new PolymorphicContractResolver(),
            };
            return JsonConvert.DeserializeObject<EntityMetadata>(jobj.GetValue("EntityMetadata").ToString(), serializerSetting);
        }
        return null;
    }
    //TODO: Do a benchmark in comparison to the EntityMetadataRepository.GetAsync method.
    public async Task<ExpandoObject> GetAsExpandoAsync(string entityLogicalName, CancellationToken cancellationToken)
    {
        var response = await client.GetAsync(entityMetadataQuerySingle.FormatWith(entityLogicalName), cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            using var content = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            using var reader = new StreamReader(content);
            using var jreader = new JsonTextReader(reader);
            var jobj = (JObject)(await JToken.ReadFromAsync(jreader, cancellationToken));
            var serializer = new Newtonsoft.Json.JsonSerializer()
            {
                ContractResolver = new PolymorphicContractResolver(),
            };
            dynamic resp = serializer.Deserialize<ExpandoObject>(jreader);
            return resp.EntityMetadata;
        }
        return null;
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
        var response = await client.GetAsync(entityMetadataQuerySingle.FormatWith(entityLogicalName), cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            using var content = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            using var reader = new StreamReader(content);
            using var jreader = new JsonTextReader(reader);
            var jobj = (JObject)(await JToken.ReadFromAsync(jreader, cancellationToken));
            var serializer = new Newtonsoft.Json.JsonSerializer()
            {
                ContractResolver = new PolymorphicContractResolver(),
            };
            return JsonConvert.DeserializeObject<EntityMetadata>(jobj.GetValue("EntityMetadata").ToString(), serializerSetting);
        }
        return null;
    }
}