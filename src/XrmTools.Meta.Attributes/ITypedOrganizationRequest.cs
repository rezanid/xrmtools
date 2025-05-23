namespace XrmTools.Extensions
{
    using Microsoft.Xrm.Sdk;
    /// <summary>
    /// Typed organization request interface that allows for the conversion of a typed request to an <see cref="Microsoft.Xrm.Sdk.OrganizationRequest"/>.
    /// </summary>
    internal interface ITypedOrganizationRequest
    {
        public string RequestName { get; }
        OrganizationRequest ToOrganizationRequest();
    }
}