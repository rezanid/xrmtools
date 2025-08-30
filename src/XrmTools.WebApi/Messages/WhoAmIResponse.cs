#nullable enable
namespace XrmTools.WebApi.Messages;

using System;

/// <summary>
/// Contains the response from the WhoAmIRequest
/// </summary>
public sealed class WhoAmIResponse : ODataResponse
{
    public Guid BusinessUnitId { get; set; }
    public Guid UserId { get; set; }
    public Guid OrganizationId { get; set; }
}
#nullable restore