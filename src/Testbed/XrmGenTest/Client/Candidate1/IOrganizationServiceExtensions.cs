using Microsoft.Xrm.Sdk;

namespace XrmGenTest.Client;

internal static partial class IOrganizationServiceExtensions
{
    internal static TResponse Execute<TRequest, TResponse>(this IOrganizationService service, TRequest request)
        where TRequest : CustomRequest<TRequest, TResponse>, new()
        where TResponse : CustomResponse<TResponse>, new()
    {
        var response = service.Execute(request.ToOrganizationRequest());
        return new TResponse().FromOrganizationResponse(response);
    }
}
