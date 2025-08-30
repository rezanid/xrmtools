#nullable enable
namespace XrmTools.Meta.Model;

using System.Collections.Generic;
using XrmTools.WebApi.Entities;

public class RetrieveAllEntitiesResponse : ODataResponse
{
    public string? Timestamp { get; set; }
    public IList<EntityMetadata> EntityMetadata { get; set; } = [];
}
#nullable restore