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
}

internal class EntityMetadataRepository(XrmHttpClient client) : XrmRepository(client), IEntityMetadataRepository
{
    private const string entityMetadataQueryAll = "RetrieveAllEntities(EntityFilters=@p1,RetrieveAsIfPublished=@p2)?@p1=Microsoft.Dynamics.CRM.EntityFilters%27Entity%27&@p2=true";
    private const string entityMetadataQuerySingle = "RetrieveEntity(LogicalName=@p1,EntityFilters=@p2,RetrieveAsIfPublished=@p3,MetadataId=@p4)?@p1=%27{0}%27&@p2=Microsoft.Dynamics.CRM.EntityFilters%27Attributes%27&@p3=true&@p4=00000000-0000-0000-0000-000000000000";
    private const string entityMessagesQuery = "sdkmessages?$select=name,isprivate,executeprivilegename,isvalidforexecuteasync,autotransact,introducedversion&$filter=sdkmessageid_sdkmessagefilter/any(n:%20n/primaryobjecttypecode%20eq%20%27{0}%27)";

    private static readonly JsonSerializerSettings serializerSetting = new()
    {
        ContractResolver = new PolymorphicContractResolver()
    };

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
}