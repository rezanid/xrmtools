using Microsoft.Xrm.Sdk;

namespace XrmGenTest.Client;

internal abstract class CustomResponse<T>
    where T : CustomResponse<T>, new()
{
    public abstract T FromOrganizationResponse(OrganizationResponse response);
}