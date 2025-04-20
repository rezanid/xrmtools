namespace XrmTools.WebApi.Entities.Attributes;

using System;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
internal sealed class EntityMetadataAttribute(string logicalName, string entitySetName) : Attribute
{
    public EntityMetadata Metadata { get; } = new EntityMetadata(logicalName, entitySetName);
}
