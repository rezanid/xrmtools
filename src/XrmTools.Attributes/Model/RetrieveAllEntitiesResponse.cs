namespace XrmTools.Meta.Model;

using Microsoft.Xrm.Sdk.Metadata;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.Json.Serialization;

public class RetrieveAllEntitiesResponse : ODataResponse
{
    public string Timestamp { get; set; }
    public IList<EntityMetadata> EntityMetadata { get; set; }
}
