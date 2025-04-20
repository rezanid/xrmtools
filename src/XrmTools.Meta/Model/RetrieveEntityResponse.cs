namespace XrmTools.Meta.Model;
using Microsoft.Xrm.Sdk.Metadata;

public class RetrieveEntityResponse : ODataResponse
{
    public required EntityMetadata EntityMetadata { get; set; }
}
