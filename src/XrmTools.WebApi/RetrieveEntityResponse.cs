namespace XrmTools.WebApi;

using XrmTools.WebApi.Entities;

public class RetrieveEntityResponse : ODataResponse
{
    public required EntityMetadata EntityMetadata { get; set; }
}
