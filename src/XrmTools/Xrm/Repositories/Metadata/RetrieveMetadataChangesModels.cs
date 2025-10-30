namespace XrmTools.Core.Repositories;

using Newtonsoft.Json;
using System.Collections.Generic;
using XrmTools.WebApi;
using XrmTools.WebApi.Entities;

// Minimal DTOs to deserialize RetrieveMetadataChanges results via Web API
// We only model the parts we need: entities, attributes, relationships, and the timestamp stamp

internal sealed class RetrieveMetadataChangesResponse : ODataResponse
{
    [JsonProperty("Timestamp")]
    public string Timestamp { get; set; }

    [JsonProperty("EntityMetadata")]
    public List<EntityMetadata> EntityMetadata { get; set; } = new List<EntityMetadata>();

    [JsonProperty("DeletedMetadata")]
    public DeletedMetadataBlock DeletedMetadata { get; set; } = new DeletedMetadataBlock();
}

internal sealed class DeletedMetadataBlock
{
    // The API returns arrays of guids for deleted metadata ids in some shapes.
    // For our current need (entity+attributes), we will ignore deletes and prefer full refresh fallback.
    // Fields defined for completeness in case we extend later.
    public List<string> Entities { get; set; } = new List<string>();
    public List<string> Attributes { get; set; } = new List<string>();
}
