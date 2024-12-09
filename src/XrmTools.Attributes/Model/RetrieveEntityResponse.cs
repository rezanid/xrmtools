namespace XrmTools.Meta.Model;
using Microsoft.Xrm.Sdk.Metadata;

public class RetrieveEntityResponse : ODataResponse
{
    public EntityMetadata EntityMetadata { get; set; }
}
