namespace XrmTools.WebApi.Types;

using System;
using System.Runtime.Serialization;
using XrmTools.WebApi.Entities;

[KnownType(typeof(AttributeMetadata))]
[KnownType(typeof(EntityMetadata))]
[KnownType(typeof(OptionSetMetadata))]
[KnownType(typeof(RelationshipMetadataBase))]
public class MetadataBase
{
    public Guid? MetadataId { get; set; }
    public bool? HasChanged { get; set; }
}
