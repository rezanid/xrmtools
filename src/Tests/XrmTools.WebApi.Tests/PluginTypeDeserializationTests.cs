namespace XrmTools.WebApi.Tests;

using FluentAssertions;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using XrmTools.WebApi;
using XrmTools.WebApi.Entities;
using Xunit;

public class PluginTypeDeserializationTests
{
    [Fact]
    public async Task CastAsync_WithExpandedPluginTypePayload_PopulatesStepsImagesAndCustomApis()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(ResponseJson, Encoding.UTF8, "application/json")
        };

        var result = await response.CastAsync<ODataQueryResponse<PluginType>>();

        result.Value.Should().HaveCount(3);

        result.Value[0].Steps.Should().ContainSingle();
        result.Value[0].Steps[0].Name.Should().Be("XrmToolsHelloWorld.AccountMultiGreetingPlugin : PreOperation UpdateMultiple of account");
        result.Value[0].Steps[0].Images.Should().ContainSingle();
        result.Value[0].Steps[0].Images[0].Name.Should().Be("AccountPreImage");

        result.Value[1].Steps.Should().ContainSingle();
        result.Value[1].CustomApi.Should().ContainSingle();
        result.Value[1].CustomApi[0].UniqueName.Should().Be("rn_SuggestNextFollowUp");
        result.Value[1].CustomApi[0].RequestParameters.Should().HaveCount(4);
        result.Value[1].CustomApi[0].ResponseProperties.Should().HaveCount(4);

        result.Value[2].Steps.Should().ContainSingle();
        result.Value[2].CustomApi.Should().BeEmpty();
    }

    private const string ResponseJson = """
{"@odata.context":"https://orgd67e4e96.crm4.dynamics.com/api/data/v9.2/$metadata#plugintypes(name,typename,friendlyname,description,workflowactivitygroupname,modifiedon,plugintype_sdkmessageprocessingstep(name,stage,asyncautodelete,description,filteringattributes,invocationsource,mode,rank,sdkmessageid,statecode,supporteddeployment,modifiedon,sdkmessageprocessingstepid_sdkmessageprocessingstepimage(name,imagetype,messagepropertyname,attributes,entityalias,modifiedon)),CustomAPIId(name,displayname,uniquename,isfunction,bindingtype,workflowsdkstepenabled,isprivate,statecode,allowedcustomprocessingsteptype,executeprivilegename,boundentitylogicalname,description,statuscode,modifiedon,CustomAPIRequestParameters(displayname,uniquename,name,statecode,statuscode,logicalentityname,description,type,isoptional),CustomAPIResponseProperties(displayname,uniquename,name,statecode,statuscode,logicalentityname,description,type)))","value":[{"@odata.etag":"W/\"17447325\"","workflowactivitygroupname":null,"modifiedon":"2026-03-29T21:21:37Z","typename":"XrmToolsHelloWorld.AccountMultiGreetingPlugin","friendlyname":"XrmToolsHelloWorld.AccountMultiGreetingPlugin","name":"XrmToolsHelloWorld.AccountMultiGreetingPlugin","plugintypeid":"868e6b71-8632-527c-ac86-2be797636740","description":null,"plugintype_sdkmessageprocessingstep":[{"sdkmessageprocessingstepid":"8efbde99-5d28-56e5-a9b7-bf3cb6a32f75","modifiedon":"2026-03-29T21:21:37Z","invocationsource":1,"mode":0,"supporteddeployment":0,"statecode":0,"name":"XrmToolsHelloWorld.AccountMultiGreetingPlugin : PreOperation UpdateMultiple of account","asyncautodelete":false,"rank":1,"description":null,"stage":20,"filteringattributes":"name","sdkmessageprocessingstepid_sdkmessageprocessingstepimage":[{"sdkmessageprocessingstepimageid":"14be7028-3f96-5c2e-94f3-6f423e05f477","modifiedon":"2026-03-29T21:21:37Z","name":"AccountPreImage","imagetype":0,"entityalias":"AccountPreImage","messagepropertyname":"Targets","attributes":"name,accountnumber"}]}],"CustomAPIId":[]},{"@odata.etag":"W/\"17447217\"","workflowactivitygroupname":null,"modifiedon":"2026-03-29T21:21:33Z","typename":"XrmToolsHelloWorld.SuggestNextFollowUpApiPlugin","friendlyname":"XrmToolsHelloWorld.SuggestNextFollowUpApiPlugin","name":"XrmToolsHelloWorld.SuggestNextFollowUpApiPlugin","plugintypeid":"ee3a0747-2517-53f0-9d00-fa0a21bf3e74","description":null,"plugintype_sdkmessageprocessingstep":[{"sdkmessageprocessingstepid":"0ff1e742-b52b-f111-88b3-7c1e52370673","modifiedon":"2026-03-29T21:21:35Z","invocationsource":0,"mode":0,"supporteddeployment":0,"statecode":0,"name":"CustomApi 'rn_SuggestNextFollowUp' implementation","asyncautodelete":false,"rank":0,"description":"CustomApi 'rn_SuggestNextFollowUp' implementation","stage":30,"filteringattributes":null,"sdkmessageprocessingstepid_sdkmessageprocessingstepimage":[]}],"CustomAPIId":[{"displayname":"Suggest Next Follow Up","modifiedon":"2026-03-29T21:21:34Z","uniquename":"rn_SuggestNextFollowUp","isfunction":false,"bindingtype":0,"workflowsdkstepenabled":false,"isprivate":false,"statecode":0,"customapiid":"f579bb98-220c-58f3-bfba-152cd687f068","name":"SuggestNextFollowUp","allowedcustomprocessingsteptype":2,"executeprivilegename":null,"description":"Suggests the next follow-up action for a given entity.","boundentitylogicalname":null,"statuscode":1,"CustomAPIRequestParameters":[{"displayname":"Target","uniquename":"Target","customapirequestparameterid":"c4aca45a-7500-5e97-a76f-267cca5443cc","statecode":0,"name":"Target","logicalentityname":null,"description":null,"isoptional":false,"type":5,"statuscode":1},{"displayname":"UrgencyOverride","uniquename":"UrgencyOverride","customapirequestparameterid":"33903a6e-05f6-57d9-9bfa-ab4a973dcda3","statecode":0,"name":"UrgencyOverride","logicalentityname":null,"description":null,"isoptional":true,"type":7,"statuscode":1},{"displayname":"LastInteractionDate","uniquename":"LastInteractionDate","customapirequestparameterid":"155be82c-5a3a-5e28-9b3e-b87a7b44f85e","statecode":0,"name":"LastInteractionDate","logicalentityname":null,"description":null,"isoptional":true,"type":1,"statuscode":1},{"displayname":"PreferredChannel","uniquename":"PreferredChannel","customapirequestparameterid":"0969fad0-af16-51da-9f94-f2028fe7aea9","statecode":0,"name":"PreferredChannel","logicalentityname":null,"description":null,"isoptional":true,"type":9,"statuscode":1}],"CustomAPIResponseProperties":[{"displayname":"SuggestedChannel","uniquename":"SuggestedChannel","customapiresponsepropertyid":"8b19246e-c7f4-5ca2-a379-2cb99916ae0f","statecode":0,"name":"SuggestedChannel","logicalentityname":null,"description":null,"statuscode":1,"type":9},{"displayname":"ImportanceScore","uniquename":"ImportanceScore","customapiresponsepropertyid":"f37af7ea-1a08-5f82-8364-51370abf72ff","statecode":0,"name":"ImportanceScore","logicalentityname":null,"description":null,"statuscode":1,"type":7},{"displayname":"Reason","uniquename":"Reason","customapiresponsepropertyid":"ab8550ec-41d7-52ec-80cf-56e70d9ebe79","statecode":0,"name":"Reason","logicalentityname":null,"description":null,"statuscode":1,"type":10},{"displayname":"SuggestedFollowUpDate","uniquename":"SuggestedFollowUpDate","customapiresponsepropertyid":"f5a7d038-6054-561e-a025-e18ee30208a1","statecode":0,"name":"SuggestedFollowUpDate","logicalentityname":null,"description":null,"statuscode":1,"type":1}]}]},{"@odata.etag":"W/\"17447331\"","workflowactivitygroupname":null,"modifiedon":"2026-03-29T21:21:37Z","typename":"XrmToolsHelloWorld.AccountGreetingPlugin","friendlyname":"XrmToolsHelloWorld.AccountGreetingPlugin","name":"XrmToolsHelloWorld.AccountGreetingPlugin","plugintypeid":"708a3810-41f9-5469-8b6a-fc4117ab87d2","description":null,"plugintype_sdkmessageprocessingstep":[{"sdkmessageprocessingstepid":"7c5397b5-b581-57b8-8900-e6368194c9dc","modifiedon":"2026-03-29T21:21:37Z","invocationsource":1,"mode":0,"supporteddeployment":0,"statecode":0,"name":"XrmToolsHelloWorld.AccountGreetingPlugin : PreOperation Create of account","asyncautodelete":false,"rank":1,"description":null,"stage":20,"filteringattributes":"name,description,industrycode,rn_suggestedfollowup","sdkmessageprocessingstepid_sdkmessageprocessingstepimage":[]}],"CustomAPIId":[]}]} 
""";
}
