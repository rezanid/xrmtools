using Microsoft.Xrm.Sdk.Metadata;
using System.Reflection;

namespace XrmGen.Extensions;
internal static class EntityMetadataExtensions
{
    public static EntityMetadata Clone(this EntityMetadata entityMetadata)
        => (EntityMetadata)entityMetadata.GetType()
        .GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic)
        .Invoke(entityMetadata, null);
}
