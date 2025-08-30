namespace XrmTools.WebApi.Messages;
using System.Net.Http;
using System;
using XrmTools.WebApi.Types;

/// <summary>
/// Retrieves metadata information about all the entities.
/// </summary>
public sealed class RetrieveAllEntitiesRequest : HttpRequestMessage
{
    const string _urlFormat = "RetrieveAllEntities(EntityFilters=@p1,RetrieveAsIfPublished=@p2)?@p1=Microsoft.Dynamics.CRM.EntityFilters'{0}'&@p2=true";
    public RetrieveAllEntitiesRequest(EntityFilters filters, bool retrieveAsIfPublished = true)
    {
        Method = HttpMethod.Get;
        RequestUri = new Uri(
            $"RetrieveAllEntities(EntityFilters=@p1,RetrieveAsIfPublished=@p2)?@p1=Microsoft.Dynamics.CRM.EntityFilters'{filters.ToString()}'&@p2={retrieveAsIfPublished}",
            uriKind: UriKind.Relative);
    }
}