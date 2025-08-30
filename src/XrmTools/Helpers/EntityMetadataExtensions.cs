#nullable enable
namespace XrmTools.Helpers;

using System.Reflection;
using XrmTools.WebApi.Entities;

internal static class EntityMetadataExtensions
{
    public static EntityMetadata Clone(this EntityMetadata entityMetadata)
        => (EntityMetadata)entityMetadata.GetType()
        .GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic)
        .Invoke(entityMetadata, null);
}
#nullable restore