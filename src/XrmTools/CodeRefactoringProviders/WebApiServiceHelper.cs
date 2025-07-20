namespace XrmTools.CodeRefactoringProviders;

using System.Linq;
using System.Threading.Tasks;
using XrmTools.WebApi;
using XrmTools.WebApi.Entities;
using XrmTools.WebApi.Methods;

internal static class WebApiServiceHelper
{
    public static async Task<CustomApi> GetCustomApiDefinitionAsync(this IWebApiService webApi, string customApiName)
    {
        var response = await webApi.RetrieveMultipleAsync<CustomApi>(
            $"customapis?$select=uniquename,name,displayname,description,allowedcustomprocessingsteptype,bindingtype,boundentitylogicalname,executeprivilegename,isfunction,isprivate" +
            $"&$filter=uniquename eq '{customApiName}'&$expand=CustomAPIRequestParameters($select=uniquename,name,displayname,description,type,logicalentityname,isoptional),CustomAPIResponseProperties($select=uniquename,name,displayname,description,type,logicalentityname)")
            .ConfigureAwait(false);
        return response?.Value?.FirstOrDefault();
    }

    public static async Task<PluginType> GetPluginDefinitionAsync(this IWebApiService webApi, string pluginTypeName)
    {
        var odataQuery =
        $"{PluginType.Metadata.EntitySetName}?" +
        $"$filter=typename eq '{pluginTypeName}'" +
        "&$select=plugintypeid,name,typename,friendlyname,description,workflowactivitygroupname&" +
        "$expand=plugintype_sdkmessageprocessingstep(" +
            "$select=sdkmessageprocessingstepid,name,stage,asyncautodelete,description,filteringattributes,invocationsource,mode,rank,sdkmessageid,statecode,supporteddeployment;" +
            "$expand=sdkmessageprocessingstepid_sdkmessageprocessingstepimage(" +
                "$select=sdkmessageprocessingstepimageid,name,imagetype,messagepropertyname,attributes,entityalias)," +
            "sdkmessagefilterid($select=primaryobjecttypecode)," +
            "sdkmessageid($select=name))";
        var response = await webApi.RetrieveMultipleAsync<PluginType>(odataQuery, 2);
        return response?.Value?.FirstOrDefault();
    }
}
