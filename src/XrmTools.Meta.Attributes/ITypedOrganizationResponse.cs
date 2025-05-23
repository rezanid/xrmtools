namespace XrmTools.Extensions
{
    using Microsoft.Xrm.Sdk;
    /// <summary>
    /// Typed organization response interface that allows for the conversion of <see cref="Microsoft.Xrm.Sdk.OrganizationRequest"/> to a typed response.
    /// </summary>
    internal interface ITypedOrganizationResponse
    {
        public void LoadFromOrganizationResponse(OrganizationResponse response);
    }
}