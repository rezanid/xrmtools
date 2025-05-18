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
}
