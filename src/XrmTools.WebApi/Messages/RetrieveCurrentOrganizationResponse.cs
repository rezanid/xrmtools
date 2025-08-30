namespace XrmTools.WebApi.Messages;

using System.Net.Http;
using XrmTools.WebApi.Types;

/// <summary>
/// Contains the response from the RetrieveCurrentOrganizationRequest 
/// </summary>
public sealed class RetrieveCurrentOrganizationResponse : HttpResponseMessage
{
    /// <summary>
    /// Gets detailed information about the current organization.
    /// </summary>
    public required OrganizationDetail Detail { get; set; }
}