#nullable enable
namespace XrmTools.WebApi;

using System.Collections.Generic;
using XrmTools.WebApi.Entities;

public class RetrieveAllEntitiesResponse : ODataResponse
{
    public string? Timestamp { get; set; }
    public IList<EntityMetadata> EntityMetadata { get; set; } = [];
}
#nullable restore