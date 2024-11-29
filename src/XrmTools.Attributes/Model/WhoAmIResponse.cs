namespace XrmTools.Meta.Model;

using System;

internal class WhoAmIResponse : ODataResponse
{
    public Guid BusinessUnitId { get; set; }
    public Guid UserId { get; set; }
    public Guid OrganizationId { get; set; }
}