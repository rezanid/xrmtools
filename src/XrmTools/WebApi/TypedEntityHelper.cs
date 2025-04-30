#nullable enable
namespace XrmTools.WebApi;
using XrmTools.Xrm.Model;

internal static class TypedEntityHelper<T> where T : TypedEntity<T>, new()
{
    public static EntityReference ToWebApiReference(T entity) => new(entity.GetEntitySetName(), entity.Id);
}
#nullable restore