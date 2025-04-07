#nullable enable
namespace XrmTools.WebApi.Entities;

using Newtonsoft.Json;
using System;
using System.Linq;
using System.Text.Json.Serialization;
using XrmTools.WebApi.Entities.Attributes;

internal abstract class Entity<T> where T : Entity<T>
{
    private static readonly EntityMetadata _metadata;

    static Entity()
    {
        var attr = typeof(T).GetCustomAttributes(typeof(EntityMetadataAttribute), false)
                            .OfType<EntityMetadataAttribute>()
                            .FirstOrDefault() ?? throw new InvalidOperationException($"EntityMetadataAttribute is missing on {typeof(T).Name}");
        _metadata = attr.Metadata;
    }

    public static EntityMetadata Metadata => _metadata;

    public static EntityReference CreateReference(Guid id) => new(_metadata.EntitySetName, id);

    public EntityReference ToReference() => new(_metadata.EntitySetName, Id);

    [JsonPropertyName("@odata.etag")]
    [JsonProperty("@odata.etag")]
    public virtual string? ODataETag { get; set; }

    public abstract Guid? Id { get; set; }
}
#nullable restore