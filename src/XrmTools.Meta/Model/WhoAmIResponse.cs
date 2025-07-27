namespace XrmTools.Meta.Model;

using System;

public class WhoAmIResponse : ODataResponse
{
    public Guid BusinessUnitId { get; set; }
    public Guid UserId { get; set; }
    public Guid OrganizationId { get; set; }
}