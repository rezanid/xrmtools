namespace XrmTools.WebApi.Entities.Attributes;

using System;
using XrmTools.WebApi.Entities;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
internal sealed class EntityMetadataAttribute(string logicalName, string entitySetName) : Attribute
{
    public EntityMetadata Metadata { get; } = new EntityMetadata(logicalName, entitySetName);
}
