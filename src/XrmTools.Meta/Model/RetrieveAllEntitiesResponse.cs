#nullable enable
namespace XrmTools.Meta.Model;

using System.Collections.Generic;
using Microsoft.Xrm.Sdk.Metadata;

public class RetrieveAllEntitiesResponse : ODataResponse
{
    public string? Timestamp { get; set; }
    public IList<EntityMetadata> EntityMetadata { get; set; } = [];
}
#nullable restore